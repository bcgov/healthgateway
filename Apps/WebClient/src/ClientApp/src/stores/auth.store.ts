import { Platform } from "react-native";
import { create } from "zustand";

import { CommunicationType } from "@/models/communication";
import { isSessionEnded, SessionEndedError } from "@/models/errors";
import type { OidcTokenDetails, OidcUserInfo } from "@/models/user";
import { classifyAuthFailure } from "@/services/auth/authFailure";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { sessionGeneration } from "@/services/sessionFence";

import { useAppLockStore } from "./appLock.store";
import {
    useClinicalDocumentFileStore,
    useClinicalDocumentStore,
} from "./clinicalDocument.store";
import { useCommentStore } from "./comment.store";
import { useCommunicationStore } from "./communication.store";
import { selectCanFetch, useConnectivityStore } from "./connectivity.store";
import { useCovid19TestResultStore } from "./covid19TestResult.store";
import { useDependentStore } from "./dependent.store";
import { useHealthVisitStore } from "./healthVisit.store";
import { useHospitalVisitStore } from "./hospitalVisit.store";
import { useImmunizationStore } from "./immunization.store";
import { useLabResultStore } from "./labResult.store";
import { useMedicationStore } from "./medication.store";
import { useNoteStore } from "./note.store";
import { useNotificationStore } from "./notification.store";
import { usePatientDataStore } from "./patientData.store";
import { getRecordOwner, setRecordOwner } from "./recordOwner";
import { resetSavedCopyState } from "./savedCopyState";
import { useSpecialAuthorityRequestStore } from "./specialAuthorityRequest.store";
import { useUserStore } from "./user.store";

interface AuthState {
    tokenDetails?: OidcTokenDetails;
    isAuthenticated: boolean;
    /**
     * Native: the user asked to log in from a screen that first ends the session. The route guard acts on it once they
     * are signed out; a session that simply ended goes to the landing page instead, without starting a login.
     */
    loginRequest?: { idp?: string };
    /** Re-sync with the auth service; loads userinfo when a valid session exists. */
    /** `isStale`: this start-up has been replaced by a newer one, so it must change nothing. */
    checkStatus: (isStale?: () => boolean) => Promise<boolean>;
    signIn: (redirectPath: string, idpHint?: string) => Promise<void>;
    signOut: () => Promise<void>;
    clearStorage: () => void;
    /** Called by the auth service after a successful (re)authentication. */
    handleSuccessfulAuthentication: (details: OidcTokenDetails) => void;
}

/** Retry after a refresh that failed for lack of a connection. */
const RETRY_REFRESH_MS = 30000;

/** Failures that are neither a refusal nor a lost connection, before the login counts as refused. */
const MAX_UNEXPECTED_FAILURES = 3;
const MIN_REFRESH_DELAY_MS = 1000;
let unexpectedFailures = 0;

function resetRecordStores() {
    useImmunizationStore.getState().reset();
    useMedicationStore.getState().reset();
    useLabResultStore.getState().reset();
    useCovid19TestResultStore.getState().reset();
    useHealthVisitStore.getState().reset();
    useNoteStore.getState().reset();
    useSpecialAuthorityRequestStore.getState().reset();
    useClinicalDocumentStore.getState().reset();
    useClinicalDocumentFileStore.getState().reset();
    useHospitalVisitStore.getState().reset();
    usePatientDataStore.getState().reset();
    useCommentStore.getState().reset();
    useDependentStore.getState().reset();
    useNotificationStore.getState().reset();
    // the in-app message may be for the person who was signed in; the public banner stays
    useCommunicationStore.getState().clear(CommunicationType.InApp);
}

let refreshTimer: ReturnType<typeof setTimeout> | undefined;

/** The lock went on and the tokens are on their way out of memory (`sealSession`). */
let sealing: Promise<void> | undefined;

/** Locked with device security: nothing may use the tokens, or put them back in the HTTP client, until unsealed. */
const isSealed = () =>
    sealing !== undefined || services.auth.isSealed?.() === true;

export const isSessionSealed = () => services.isInitialized && isSealed();

function clearRefreshTimer() {
    unexpectedFailures = 0;
    if (refreshTimer !== undefined) {
        clearTimeout(refreshTimer);
        refreshTimer = undefined;
    }
}

export const useAuthStore = create<AuthState>((set, get) => ({
    tokenDetails: undefined,
    isAuthenticated: false,
    loginRequest: undefined,

    handleSuccessfulAuthentication: (details) => {
        // a refresh that finished as the lock went on: its tokens are sealed, not put back to work
        if (isSealed()) return;
        useConnectivityStore.getState().setSessionStatus("live");
        services.http.setAuthorizationHeader(details.accessToken);
        set({
            tokenDetails: details,
            isAuthenticated:
                details.idToken.length > 0 && details.accessToken.length > 0,
        });
        clearRefreshTimer();
        // already due (a token close to expiry at launch): soon, not never
        const delay = details.refreshTokenTime * 1000 - Date.now();
        refreshTimer = setTimeout(
            () => void refresh(),
            Math.max(delay, MIN_REFRESH_DELAY_MS)
        );
    },

    checkStatus: async (isStale = () => false) => {
        if (isStale() || !services.isInitialized) return false;
        const details = services.auth.getOidcTokenDetails();
        if (!details || details.expired) {
            if (get().isAuthenticated) {
                logger.verbose("Authentication status was not in sync");
                get().clearStorage();
            }
            return false;
        }
        const session = sessionGeneration();
        let info: OidcUserInfo;
        try {
            info = await services.auth.getOidcUserInfo();
        } catch (error) {
            if (isSessionEnded(error)) return false;
            throw error;
        }
        if (isStale() || session !== sessionGeneration()) return false;
        useUserStore.getState().setOidcUserInfo(info);
        get().handleSuccessfulAuthentication(details);
        return true;
    },

    signIn: async (redirectPath, idpHint) => {
        const session = sessionGeneration();
        const details = await services.auth.signIn(redirectPath, idpHint);
        const info = await services.auth.getOidcUserInfo();
        // signed out during the login: its callers must not carry on as though it worked
        if (session !== sessionGeneration()) throw new SessionEndedError();
        // an expired session's records are still in the stores: they must not reach a different person
        const owner = getRecordOwner();
        if (owner && info.hdid !== owner) {
            resetRecordStores();
            resetSavedCopyState();
        }
        setRecordOwner(info.hdid);
        useUserStore.getState().setOidcUserInfo(info);
        get().handleSuccessfulAuthentication(details);
        logger.verbose("Successfully signed in");
    },

    signOut: async () => {
        clearRefreshTimer();
        services.http.endSession();
        useConnectivityStore.getState().setSessionStatus("live");
        useUserStore.getState().clearUserData();
        resetRecordStores();
        try {
            await services.auth.signOut();
            logger.verbose("Successfully signed out");
        } catch {
            logger.warn("Failed to sign out");
        } finally {
            services.http.unsetAuthorizationHeader();
            set({ tokenDetails: undefined, isAuthenticated: false });
        }
    },

    clearStorage: () => {
        clearRefreshTimer();
        services.http.endSession();
        useConnectivityStore.getState().setSessionStatus("live");
        services.auth.clearState();
        services.http.unsetAuthorizationHeader();
        set({ tokenDetails: undefined, isAuthenticated: false });
        useUserStore.getState().clearUserData();
        resetRecordStores();
    },
}));

async function refresh() {
    logger.info("Refreshing access token");
    const session = sessionGeneration();
    try {
        const refreshed = await services.auth.refreshToken();
        if (session !== sessionGeneration() || isSealed()) return;
        unexpectedFailures = 0;
        if (refreshed) {
            const details = services.auth.getOidcTokenDetails();
            if (details)
                useAuthStore.getState().handleSuccessfulAuthentication(details);
            logger.verbose("Refreshed access token");
        } else {
            logger.verbose("Access token refresh not required");
            // the next refresh is still to come: without this the refreshing would stop here
            const details = services.auth.getOidcTokenDetails();
            if (details?.refreshToken && !details.expired)
                refreshTimer = setTimeout(
                    () => void refresh(),
                    Math.max(
                        details.refreshTokenTime * 1000 - Date.now(),
                        MIN_REFRESH_DELAY_MS
                    )
                );
        }
    } catch (error) {
        if (session !== sessionGeneration() || isSealed()) return;
        logger.warn(
            `Could not refresh the access token: ${describeError(error)}`
        );
        const failure = classifyAuthFailure(error);
        if (Platform.OS !== "web") {
            // No answer from Keycloak says nothing about the login: try again for as long as that lasts
            if (failure === "connection") {
                const connectivity = useConnectivityStore.getState();
                if (connectivity.isOnline)
                    connectivity.setServiceUnavailable(true);
                refreshTimer = setTimeout(
                    () => void refresh(),
                    RETRY_REFRESH_MS
                );
                return;
            }
            // Anything else but a refusal might pass, but must not leave an old token in use forever
            unexpectedFailures += 1;
            if (
                failure === "unexpected" &&
                unexpectedFailures < MAX_UNEXPECTED_FAILURES
            ) {
                refreshTimer = setTimeout(
                    () => void refresh(),
                    RETRY_REFRESH_MS
                );
                return;
            }
        }
        unexpectedFailures = 0;
        // Device security on: the records stay, behind the expired banner
        if (useAppLockStore.getState().enabled) {
            useConnectivityStore.getState().setSessionStatus("expired");
            return;
        }
        await useAuthStore.getState().signOut();
    }
}

/**
 * Timers wait while the app is in the background, so on returning the access token may be out of date with its refresh
 * still to fire. Call before relying on the token. Quietly does nothing while the login is on hold.
 */
export async function refreshTokenIfDue(): Promise<void> {
    const auth = useAuthStore.getState();
    if (
        !auth.isAuthenticated ||
        !services.isInitialized ||
        isSealed() ||
        !selectCanFetch(useConnectivityStore.getState())
    )
        return;
    const details = services.auth.getOidcTokenDetails();
    if (!details || details.refreshTokenTime * 1000 > Date.now()) return;
    if (refreshTimer !== undefined) clearTimeout(refreshTimer);
    refreshTimer = undefined;
    await refresh();
}

/**
 * Device security, the lock going on: the tokens leave memory, sealed by the vault until the prompt passes again
 * (`unsealSession`). The signed-in screens and records stay under the lock.
 */
export function sealSession(): Promise<void> {
    if (
        !services.isInitialized ||
        !services.auth.seal ||
        !useAuthStore.getState().isAuthenticated
    )
        return Promise.resolve();
    clearRefreshTimer();
    services.http.unsetAuthorizationHeader();
    useAuthStore.setState({ tokenDetails: undefined });
    const run: Promise<void> = services.auth
        .seal()
        .catch((error: unknown) =>
            logger.warn(`Could not seal the login: ${describeError(error)}`)
        )
        .finally(() => {
            if (sealing === run) sealing = undefined;
        });
    sealing = run;
    return run;
}

/** Resolves once a lock that went on has finished taking the tokens out of memory. */
export async function sealed(): Promise<void> {
    await sealing;
}

/** Device security turned on or off: the stored login is sealed by the vault, or no longer. */
export async function protectSession(on: boolean): Promise<void> {
    if (services.isInitialized) await services.auth.setSealed?.(on);
}

/**
 * The lock screen's way to a new login. Ending the login takes the same prompt as unlocking: turned down, or not shown for a
 * reason that may pass, nothing changes. Passed, the login is unsealed first, so Keycloak hears of the sign-out. With no
 * device security left on the device the key has gone with it and nothing can read the login, so it is only deleted here.
 */
export async function signOutFromLock(): Promise<boolean> {
    const result = await useAppLockStore.getState().authenticate();
    if (result === "cancelled" || result === "unavailable") return false;
    if (result === "unlocked" && services.isInitialized)
        await services.auth.unseal?.();
    useAppLockStore.setState({ beforeUnlock: undefined });
    // the lock lifts once the session is gone (useAppLockLifecycle); the guard goes to login
    useAuthStore.setState({ loginRequest: {} });
    await useAuthStore.getState().signOut();
    return true;
}
