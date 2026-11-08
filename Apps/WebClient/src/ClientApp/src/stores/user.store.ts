import { DateTime } from "luxon";
import { useMemo } from "react";
import { create } from "zustand";

import {
    AppErrorType,
    ErrorSourceType,
    ErrorType,
} from "@/constants/errorType";
import { UserPreferenceType } from "@/constants/userPreferenceType";
import { ResultError, rethrowIfSessionEnded } from "@/models/errors";
import type { Patient } from "@/models/patient";
import {
    type QuickLink,
    quickLinksFromString,
    quickLinksToString,
} from "@/models/quickLink";
import { type OidcUserInfo, User } from "@/models/user";
import type { UserPreference } from "@/models/userPreference";
import type {
    UserProfile,
    UserProfileNotificationSettingModel,
} from "@/models/userProfile";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { sessionGeneration } from "@/services/sessionFence";
import { setDefaultPreference } from "@/utils/preferenceUtil";

import { useErrorStore } from "./error.store";

interface UserState {
    user: User;
    oidcUserInfo?: OidcUserInfo;
    patient?: Patient;
    patientRetrievalFailed: boolean;
    isLoading: boolean;
    /** true once patient + profile retrieval has completed (or failed) for the signed-in user */
    essentialDataLoaded: boolean;
    /** ISO timestamp of the last SMS verification send, for the resend cooldown. */
    smsResendDateTime?: string;

    setOidcUserInfo: (info: OidcUserInfo) => void;
    setProfile: (profile: UserProfile) => void;
    clearUserData: () => void;
    retrieveEssentialData: (forLogin?: boolean) => Promise<void>;
    retrieveProfile: () => Promise<void>;
    updateUserEmail: (email: string) => Promise<void>;
    updateSmsNumber: (sms: string) => Promise<void>;
    validateSms: (digits: string) => Promise<boolean>;
    updateNotificationSettings: (
        setting: UserProfileNotificationSettingModel
    ) => Promise<void>;
    closeUserAccount: () => Promise<void>;
    recoverUserAccount: () => Promise<void>;
    setSmsResendDateTime: (iso: string | undefined) => void;
    /** Create or update a user preference and merge the result into the user (Pinia setUserPreference). */
    setUserPreference: (preference: UserPreference) => Promise<void>;
    updateQuickLinks: (quickLinks: QuickLink[]) => Promise<void>;
    createProfile: (profile: UserProfile) => Promise<void>;
    updateAcceptedTerms: (termsOfServiceId: string) => Promise<void>;
    /** Resolves true when verified; rejects with a 409 ResultError when already verified. */
    validateEmail: (inviteKey: string) => Promise<boolean>;
}

function profileToUser(current: User, profile: UserProfile): User {
    const user = new User();
    user.hdid = current.hdid;
    user.acceptedTermsOfService = profile.acceptedTermsOfService;
    user.hasTermsOfServiceUpdated = profile.hasTermsOfServiceUpdated ?? false;
    user.lastLoginDateTime = profile.lastLoginDateTime;
    user.lastLoginDateTimes = profile.lastLoginDateTimes ?? [];
    user.closedDateTime = profile.closedDateTime;
    user.preferences = { ...profile.preferences };
    // setProfileUserData in stores/user.ts: without these, hiding a card posts a preference with no name, which is refused
    for (const type of [
        UserPreferenceType.HideOrganDonorQuickLink,
        UserPreferenceType.HideImmunizationRecordQuickLink,
        UserPreferenceType.HideHealthConnectRegistryQuickLink,
        UserPreferenceType.HideRecommendationsQuickLink,
    ])
        setDefaultPreference(user.preferences, type, "false");
    user.blockedDataSources = profile.blockedDataSources ?? [];
    user.betaFeatures = profile.betaFeatures ?? [];
    user.email = profile.email ?? "";
    user.hasEmail = Boolean(profile.email);
    user.verifiedEmail = profile.isEmailVerified === true;
    user.sms = profile.smsNumber ?? "";
    user.hasSms = Boolean(profile.smsNumber);
    user.verifiedSms = profile.isSMSNumberVerified === true;
    user.hasTourUpdated = profile.hasTourUpdated === true;
    user.notificationSettings = profile.notificationSettings ?? [];
    return user;
}

let essentialDataPromise: Promise<void> | undefined;

export const useUserStore = create<UserState>((set, get) => ({
    user: new User(),
    oidcUserInfo: undefined,
    patient: undefined,
    patientRetrievalFailed: false,
    isLoading: false,
    essentialDataLoaded: false,
    smsResendDateTime: undefined,

    setOidcUserInfo: (info) => {
        const user = { ...get().user, hdid: info.hdid ?? "" };
        set({ user, oidcUserInfo: info });
    },

    setProfile: (profile) => {
        set({ user: profileToUser(get().user, profile) });
    },

    clearUserData: () => {
        essentialDataPromise = undefined;
        set({
            user: new User(),
            oidcUserInfo: undefined,
            patient: undefined,
            patientRetrievalFailed: false,
            isLoading: false,
            essentialDataLoaded: false,
            smsResendDateTime: undefined,
        });
    },

    retrieveEssentialData: (forLogin = false) => {
        if (essentialDataPromise) return essentialDataPromise;
        const session = sessionGeneration();
        // signed out meanwhile: none of this belongs to whoever is here now
        const ended = () => session !== sessionGeneration();
        let run: Promise<void> | undefined = undefined;
        run = (async () => {
            const hdid = get().user.hdid;
            const errorStore = useErrorStore.getState();
            set({ isLoading: true });
            try {
                const patient = await services.patient.getPatient(hdid);
                if (ended()) return;
                if (!patient) {
                    set({ patientRetrievalFailed: true });
                    logger.debug("Patient retrieval failed");
                    return;
                }
                set({ patient });
                try {
                    const profile = forLogin
                        ? await services.userProfile.getProfileForLogin(hdid)
                        : await services.userProfile.getProfile(hdid);
                    if (ended()) return;
                    get().setProfile(profile);
                } catch (error) {
                    if (ended()) return;
                    set({ patientRetrievalFailed: true });
                    if (
                        error instanceof ResultError &&
                        error.statusCode === 429
                    ) {
                        errorStore.setAppError(AppErrorType.TooManyRequests);
                    } else {
                        errorStore.addError(
                            ErrorType.Retrieve,
                            ErrorSourceType.User,
                            error instanceof ResultError
                                ? error.traceId
                                : undefined
                        );
                    }
                }
            } catch (error) {
                if (ended()) return;
                set({ patientRetrievalFailed: true });
                if (error instanceof ResultError && error.statusCode === 429) {
                    errorStore.setAppError(AppErrorType.TooManyRequests);
                }
                logger.debug("Patient retrieval failed");
            } finally {
                if (!ended())
                    set({ isLoading: false, essentialDataLoaded: true });
                if (essentialDataPromise === run)
                    essentialDataPromise = undefined;
            }
        })();
        essentialDataPromise = run;
        return run;
    },

    retrieveProfile: async () => {
        const profile = await services.userProfile.getProfile(get().user.hdid);
        get().setProfile(profile);
    },

    updateUserEmail: async (email) => {
        try {
            await services.userProfile.updateEmail(get().user.hdid, email);
            await get().retrieveProfile();
        } catch (error) {
            rethrowIfSessionEnded(error);
            if (!(error instanceof ResultError && error.statusCode === 409)) {
                useErrorStore
                    .getState()
                    .handleResultError(
                        error,
                        ErrorType.Update,
                        ErrorSourceType.User
                    );
            }
            throw error;
        }
    },

    updateSmsNumber: async (sms) => {
        try {
            await services.userProfile.updateSmsNumber(get().user.hdid, sms);
            await get().retrieveProfile();
        } catch (error) {
            rethrowIfSessionEnded(error);
            useErrorStore
                .getState()
                .handleResultError(
                    error,
                    ErrorType.Update,
                    ErrorSourceType.User
                );
            throw error;
        }
    },

    validateSms: async (digits) => {
        const ok = await services.userProfile.validateSms(
            get().user.hdid,
            digits
        );
        if (ok) await get().retrieveProfile();
        return ok;
    },

    updateNotificationSettings: async (setting) => {
        try {
            await services.userProfile.updateNotificationSettings(
                get().user.hdid,
                setting
            );
            await get().retrieveProfile();
        } catch (error) {
            rethrowIfSessionEnded(error);
            useErrorStore
                .getState()
                .handleResultError(
                    error,
                    ErrorType.Update,
                    ErrorSourceType.Profile
                );
            throw error;
        }
    },

    closeUserAccount: async () => {
        await services.userProfile.closeAccount(get().user.hdid);
        await get().retrieveProfile();
    },

    recoverUserAccount: async () => {
        await services.userProfile.recoverAccount(get().user.hdid);
        await get().retrieveProfile();
    },

    setSmsResendDateTime: (iso) => set({ smsResendDateTime: iso }),

    createProfile: async (profile) => {
        try {
            const created = await services.userProfile.createProfile({
                profile,
            });
            get().setProfile(created);
        } catch (error) {
            rethrowIfSessionEnded(error);
            useErrorStore
                .getState()
                .handleResultError(
                    error,
                    ErrorType.Create,
                    ErrorSourceType.Profile
                );
            throw error;
        }
    },

    updateAcceptedTerms: async (termsOfServiceId) => {
        await services.userProfile.updateAcceptedTerms(
            get().user.hdid,
            termsOfServiceId
        );
        await get().retrieveProfile();
    },

    validateEmail: async (inviteKey) => {
        try {
            const result = await services.userProfile.validateEmail(
                get().user.hdid,
                inviteKey
            );
            await get().retrieveProfile();
            return result;
        } catch (error) {
            rethrowIfSessionEnded(error);
            if (!(error instanceof ResultError && error.statusCode === 409)) {
                useErrorStore
                    .getState()
                    .handleResultError(
                        error,
                        ErrorType.Update,
                        ErrorSourceType.User
                    );
            }
            throw error;
        }
    },

    updateQuickLinks: async (quickLinks) => {
        const existing = get().user.preferences[UserPreferenceType.QuickLinks];
        const value = quickLinksToString(quickLinks);
        const preference: UserPreference = existing
            ? { ...existing, value }
            : {
                  preference: UserPreferenceType.QuickLinks,
                  value,
                  version: 0,
                  createdDateTime: DateTime.now().toISO() ?? "",
              };
        await get().setUserPreference(preference);
    },

    setUserPreference: async (preference) => {
        const hdid = get().user.hdid;
        const result =
            preference.hdId === undefined
                ? await services.userProfile.createUserPreference(hdid, {
                      ...preference,
                      hdId: hdid,
                  })
                : await services.userProfile.updateUserPreference(
                      hdid,
                      preference
                  );
        if (result) {
            set({
                user: {
                    ...get().user,
                    preferences: {
                        ...get().user.preferences,
                        [result.preference]: result,
                    },
                },
            });
        }
    },
}));

// ---- selectors (mirror the Pinia user store getters) ----

export const selectIsValidIdentityProvider = (s: UserState) =>
    s.oidcUserInfo === undefined
        ? false
        : s.oidcUserInfo.idp === "BCSC" || s.oidcUserInfo.idp === undefined;

export const selectUserIsRegistered = (s: UserState) =>
    s.user.acceptedTermsOfService;

/** The previous login: the first entry is the current login, the second the actual last one. */
export const selectLastLoginDateTime = (s: UserState) =>
    s.user.lastLoginDateTimes.length > 1
        ? s.user.lastLoginDateTimes[1]
        : undefined;

export const selectUserIsActive = (s: UserState) => !s.user.closedDateTime;

export const selectHasTermsOfServiceUpdated = (s: UserState) =>
    s.user.hasTermsOfServiceUpdated;

export const selectUserInitials = (s: UserState) => {
    const first = s.oidcUserInfo?.given_name;
    const last = s.oidcUserInfo?.family_name;
    if (first && last) return first.charAt(0) + last.charAt(0);
    if (first) return first.charAt(0);
    if (last) return last.charAt(0);
    return "?";
};

export const selectUserName = (s: UserState) =>
    s.oidcUserInfo === undefined
        ? ""
        : `${s.oidcUserInfo.given_name ?? ""} ${s.oidcUserInfo.family_name ?? ""}`.trim();

/** Raw quick-links preference JSON (stable string reference; parse with useQuickLinks). */
export const selectQuickLinksJson = (s: UserState): string | undefined =>
    s.user.preferences[UserPreferenceType.QuickLinks]?.value;

/** Parsed quick links, memoised on the preference string so the selector never returns a fresh array. */
export function useQuickLinks(): QuickLink[] | undefined {
    const json = useUserStore(selectQuickLinksJson);
    return useMemo(() => {
        if (json === undefined) return undefined;
        try {
            return quickLinksFromString(json);
        } catch {
            return [];
        }
    }, [json]);
}
