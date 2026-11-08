import { Path } from "@/constants/path";
import type { FeatureToggleConfiguration } from "@/models/configData";

import { type RouteMeta, UserState } from "./userState";

const S = UserState;

/** Port of the `meta` blocks in the web client's router/index.ts. Keys are path prefixes. */
export const routeMeta: Record<string, RouteMeta> = {
    [Path.Root]: { validStates: [S.unauthenticated, S.registered, S.offline] },
    [Path.Dependents]: {
        validStates: [S.registered],
        requiredFeaturesEnabled: (c) => c.dependents.enabled,
    },
    [Path.IdirLoggedIn]: { validStates: [S.invalidIdentityProvider] },
    [Path.Home]: { validStates: [S.registered] },
    [Path.OtherRecordSources]: {
        validStates: [S.registered],
        requiredFeaturesEnabled: (c) =>
            c.homepage?.otherRecordSources?.enabled ?? false,
    },
    [Path.Profile]: {
        validStates: [S.registered, S.pendingDeletion, S.acceptTermsOfService],
    },
    // native: Device Security and Reset App are for anyone, logged in or not
    [Path.Security]: { stateless: true },
    [Path.Login]: { validStates: [S.unauthenticated] },
    [Path.LoginCallback]: { stateless: true },
    [Path.Logout]: { stateless: true },
    [Path.LogoutComplete]: { stateless: true },
    [Path.Registration]: { validStates: [S.notRegistered] },
    [Path.Services]: {
        validStates: [S.registered],
        requiredFeaturesEnabled: (c) => c.services.enabled,
    },
    [Path.PatientRetrievalError]: { validStates: [S.noPatient] },
    [Path.Unauthorized]: { stateless: true },
    [Path.NotFound]: { stateless: true },
    [Path.Reports]: { validStates: [S.registered] },
    [Path.Timeline]: { validStates: [S.registered] },
    [Path.TermsOfService]: {
        validStates: [
            S.unauthenticated,
            S.invalidIdentityProvider,
            S.noPatient,
            S.registered,
            S.pendingDeletion,
        ],
    },
    [Path.AcceptTermsOfService]: { validStates: [S.acceptTermsOfService] },
    [Path.ValidateEmail]: { validStates: [S.registered] },
    [Path.VppLogin]: { validStates: [S.unauthenticated, S.registered] },
};

/** `/dependents/:id/timeline` also needs the dependent timeline feature (router/index.ts). */
const dependentTimelineMeta: RouteMeta = {
    validStates: [S.registered],
    requiredFeaturesEnabled: (c) =>
        c.dependents.enabled && c.dependents.timelineEnabled,
};

/**
 * Whether the features a route needs are switched on (router/index.ts `requiredFeaturesEnabled`). Routes without a
 * requirement, and unknown ones, pass; a gated route fails until the configuration is loaded. The route guard and
 * `useRouteFeaturesEnabled` both decide with this, so a page is never shown where the guard would send the user on.
 */
export function routeFeaturesEnabled(
    pathname: string,
    toggles: FeatureToggleConfiguration | undefined
): boolean {
    const meta = getRouteMeta(pathname);
    return (
        meta?.requiredFeaturesEnabled === undefined ||
        (toggles !== undefined && meta.requiredFeaturesEnabled(toggles))
    );
}

export function getRouteMeta(pathname: string): RouteMeta | undefined {
    const normalized = pathname.toLowerCase().replace(/\/+$/, "") || "/";
    if (/^\/dependents\/[^/]+(\/timeline)?$/.test(normalized))
        return dependentTimelineMeta;
    const exact = Object.keys(routeMeta).find(
        (p) => p.toLowerCase() === normalized
    );
    if (exact) return routeMeta[exact];
    const prefix = Object.keys(routeMeta)
        .filter(
            (p) => p !== "/" && normalized.startsWith(p.toLowerCase() + "/")
        )
        .sort((a, b) => b.length - a.length)[0];
    return prefix ? routeMeta[prefix] : undefined;
}
