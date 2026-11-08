import { Path } from "@/constants/path";
import type { FeatureToggleConfiguration } from "@/models/configData";
import { useAuthStore } from "@/stores/auth.store";
import { selectIsOffline, useConfigStore } from "@/stores/config.store";
import {
    selectHasTermsOfServiceUpdated,
    selectIsValidIdentityProvider,
    selectUserIsActive,
    selectUserIsRegistered,
    useUserStore,
} from "@/stores/user.store";

export enum UserState {
    offline = "offline",
    unauthenticated = "unauthenticated",
    invalidIdentityProvider = "invalidIdentityProvider",
    noPatient = "noPatient",
    notRegistered = "notRegistered",
    pendingDeletion = "pendingDeletion",
    acceptTermsOfService = "acceptTermsOfService",
    registered = "registered",
}

export interface RouteMeta {
    validStates?: UserState[];
    requiredFeaturesEnabled?: (config: FeatureToggleConfiguration) => boolean;
    stateless?: boolean;
}

/** Port of calculateUserState() from the web client's beforeEachGuard. */
export function calculateUserState(): UserState {
    const config = useConfigStore.getState();
    const auth = useAuthStore.getState();
    const user = useUserStore.getState();

    if (selectIsOffline(config)) return UserState.offline;
    if (!auth.isAuthenticated) return UserState.unauthenticated;
    if (!selectIsValidIdentityProvider(user))
        return UserState.invalidIdentityProvider;
    if (user.patientRetrievalFailed) return UserState.noPatient;
    if (!selectUserIsRegistered(user)) return UserState.notRegistered;
    if (!selectUserIsActive(user)) return UserState.pendingDeletion;
    if (selectHasTermsOfServiceUpdated(user))
        return UserState.acceptTermsOfService;
    return UserState.registered;
}

export function getDefaultPath(
    state: UserState,
    requiredFeaturesEnabled: boolean
): string {
    switch (state) {
        case UserState.offline:
            return Path.Root;
        case UserState.pendingDeletion:
            return Path.Profile;
        case UserState.registered:
            return requiredFeaturesEnabled ? Path.Home : Path.Unauthorized;
        case UserState.notRegistered:
            return Path.Registration;
        case UserState.invalidIdentityProvider:
            return Path.IdirLoggedIn;
        case UserState.noPatient:
            return Path.PatientRetrievalError;
        case UserState.unauthenticated:
            return requiredFeaturesEnabled ? Path.Login : Path.Unauthorized;
        case UserState.acceptTermsOfService:
            return Path.AcceptTermsOfService;
        default:
            return Path.Unauthorized;
    }
}

/** True when the signed-in user may see the full shell (sidebar, breadcrumbs, in-app banner). */
export function isFullyRegisteredUser(): boolean {
    return calculateUserState() === UserState.registered;
}
