import AsyncStorage from "@react-native-async-storage/async-storage";
import { DateTime } from "luxon";
import { Platform } from "react-native";
import { create } from "zustand";

import type {
    ExternalConfiguration,
    IdentityProviderConfiguration,
    OpenIdConnectConfiguration,
} from "@/models/configData";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { appBuild, isUpdateRequired } from "@/utils/appVersion";

import { selectEnvironment, useEnvironmentStore } from "./environment.store";

const SAVED_CONFIG_KEY = "hg.config.saved";

/** One per environment: a save still finishing from the last one must not be read as this one's. */
export const savedConfigKey = (url = getConfigUrl()) =>
    `${SAVED_CONFIG_KEY}:${url}`;

export type LoadStatus = "idle" | "loading" | "loaded" | "error";

/**
 * Native: the chosen environment's. Web (port of RestConfigService): a CLI run names its server in `.env`; a deployed
 * build asks the server that hosted it.
 */
export function getConfigUrl(): string {
    if (Platform.OS !== "web")
        return selectEnvironment(useEnvironmentStore.getState()).configUrl;
    return `${process.env.EXPO_PUBLIC_WEB_CLIENT_BASE_URI || "/"}configuration`;
}

let latestRetrieve = 0;

const isText = (value: unknown): value is string =>
    typeof value === "string" && value !== "";
const isObject = (value: unknown) =>
    typeof value === "object" && value !== null && !Array.isArray(value);
const isMissing = (value: unknown) => value === undefined || value === null;

/** Each service builds its address from one of these as it is created. */
const REQUIRED_ENDPOINTS = [
    "ClinicalDocument",
    "Encounter",
    "GatewayApi",
    "HospitalVisit",
    "Immunization",
    "Laboratory",
    "Medication",
    "Patient",
    "PatientData",
    "SpecialAuthority",
];
/** Read as `toggle.setting` by services as they are created and by the menus. */
const REQUIRED_TOGGLES = [
    "dependents",
    "homepage",
    "notificationCentre",
    "profile",
    "services",
    "timeline",
];

/** An optional maintenance window: read on every render by `selectIsOffline`, which relies on all of this. */
function isUsableOfflineMode(value: unknown) {
    if (isMissing(value)) return true;
    if (!isObject(value)) return false;
    const offline = value as Record<string, unknown>;
    return (
        isText(offline.startDateTime) &&
        Array.isArray(offline.whitelist) &&
        (isMissing(offline.endDateTime) || isText(offline.endDateTime))
    );
}

/** Everything start-up and rendering read without checking: a copy missing any of it could crash, so it is no copy at all. */
export function isUsableConfig(value: unknown): value is ExternalConfiguration {
    if (!isObject(value)) return false;
    const config = value as Partial<ExternalConfiguration>;
    const endpoints = (config.serviceEndpoints ?? {}) as Record<
        string,
        unknown
    >;
    const web = (config.webClient ?? {}) as unknown as Record<string, unknown>;
    const features = (web.featureToggleConfiguration ?? {}) as Record<
        string,
        unknown
    >;
    const services = (features.services ?? {}) as Record<string, unknown>;
    return (
        isText(config.openIdConnect?.authority) &&
        isText(config.openIdConnect?.clientId) &&
        isText(config.openIdConnect?.scope) &&
        Array.isArray(config.identityProviders) &&
        REQUIRED_ENDPOINTS.every((name) => isText(endpoints[name])) &&
        isObject(web.timeouts) &&
        isUsableOfflineMode(web.offlineMode) &&
        Array.isArray(features.datasets) &&
        REQUIRED_TOGGLES.every((name) => isObject(features[name])) &&
        Array.isArray(services.services)
    );
}

/** The configuration saved for offline launches. One that is damaged or incomplete is dropped, and counts as none. */
async function readSavedConfig(): Promise<ExternalConfiguration | undefined> {
    const key = savedConfigKey();
    const saved = await AsyncStorage.getItem(key).catch(() => null);
    if (!saved) return undefined;
    try {
        const config: unknown = JSON.parse(saved);
        if (isUsableConfig(config)) return config;
    } catch {
        logger.debug("Saved configuration unreadable");
    }
    await AsyncStorage.removeItem(key).catch(() => undefined);
    return undefined;
}

interface ConfigState {
    config?: ExternalConfiguration;
    status: LoadStatus;
    errorMessage?: string;
    /** `allowSaved`: with no connection, fall back on the configuration from the last launch. */
    retrieve: (allowSaved?: boolean) => Promise<ExternalConfiguration>;
    /** Quietly picks up a changed configuration while the app is running, for a required update. */
    refresh: () => Promise<void>;
}

export const useConfigStore = create<ConfigState>((set) => ({
    config: undefined,
    status: "idle",
    errorMessage: undefined,
    refresh: async () => {
        const run = latestRetrieve;
        try {
            const config =
                await services.config.getConfiguration(getConfigUrl());
            if (run === latestRetrieve) set({ config });
        } catch {
            logger.debug("Could not refresh configuration");
        }
    },
    retrieve: async (allowSaved = false) => {
        // a retry or another environment may start while this is still waiting: only the newest may answer
        latestRetrieve += 1;
        const run = latestRetrieve;
        set({ status: "loading", errorMessage: undefined });
        try {
            const config =
                await services.config.getConfiguration(getConfigUrl());
            if (run !== latestRetrieve) return config;
            set({ config, status: "loaded" });
            if (Platform.OS !== "web")
                AsyncStorage.setItem(
                    savedConfigKey(),
                    JSON.stringify(config)
                ).catch(() => undefined);
            return config;
        } catch (error) {
            const saved = allowSaved ? await readSavedConfig() : undefined;
            if (run !== latestRetrieve) throw error;
            if (saved) {
                set({ config: saved, status: "loaded" });
                return saved;
            }
            set({
                config: undefined,
                status: "error",
                errorMessage:
                    error instanceof Error ? error.message : String(error),
            });
            throw error;
        }
    },
}));

// ---- selectors (mirror the Pinia config store getters) ----

// A selector must return a stable reference: a fresh `[]` per call re-renders subscribers forever.
const NO_PROVIDERS: IdentityProviderConfiguration[] = [];

/** The login picker's providers: web from `identityProviders`, native from `webClient.mobile.authentication`. */
export const selectIdentityProviders = (
    s: ConfigState
): IdentityProviderConfiguration[] =>
    (Platform.OS === "web"
        ? s.config?.identityProviders
        : s.config?.webClient?.mobile?.authentication?.identityProviders) ??
    NO_PROVIDERS;

export const selectWebConfig = (s: ConfigState) => s.config?.webClient;

export const selectFeatureToggles = (s: ConfigState) =>
    s.config?.webClient?.featureToggleConfiguration;

/** `webClient.mobile.timeouts.idle` when the configuration does not set a usable one. */
export const DEFAULT_MOBILE_IDLE_MS = 20 * 60 * 1000;

/** Milliseconds without activity before logging out or locking: native from `webClient.mobile`, web from `webClient`. */
export const selectIdleTimeout = (s: ConfigState): number => {
    if (Platform.OS === "web") return s.config?.webClient?.timeouts?.idle ?? 0;
    const idle = s.config?.webClient?.mobile?.timeouts?.idle;
    return typeof idle === "number" && idle > 0 ? idle : DEFAULT_MOBILE_IDLE_MS;
};

/** Native: this app's build number is below `webClient.mobile.minimumBuild`. */
export const selectUpdateRequired = (s: ConfigState): boolean =>
    Platform.OS !== "web" &&
    isUpdateRequired(appBuild(), s.config?.webClient?.mobile?.minimumBuild);

/** The server's configuration leaves out a setting the native app cannot sign in without. */
export class MobileConfigurationError extends Error {
    constructor(readonly missing: string[]) {
        super(`The configuration is missing ${missing.join(" and ")}`);
        this.name = "MobileConfigurationError";
    }
}

/**
 * iOS and Android sign in with their own Keycloak client: the realm is `webClient.mobile.authentication.endpoint`, the
 * client ID is its `clientIdPrefix` plus "-ios" or "-android", and the login picker offers its `identityProviders`.
 * None falls back to the web client's `openIdConnect` or `identityProviders`: a missing one is a server configuration
 * error, and start-up stops on it. Only the scope is shared with the web client.
 */
export function nativeOidcConfig(
    config: ExternalConfiguration,
    os: string
): OpenIdConnectConfiguration {
    const auth = config.webClient?.mobile?.authentication;
    const endpoint = auth?.endpoint;
    const prefix = auth?.clientIdPrefix;
    const providers = auth?.identityProviders;
    const hasProviders = Array.isArray(providers) && providers.length > 0;
    if (!isText(endpoint) || !isText(prefix) || !hasProviders) {
        const missing: string[] = [];
        if (!isText(endpoint))
            missing.push("webClient.mobile.authentication.endpoint");
        if (!isText(prefix))
            missing.push("webClient.mobile.authentication.clientIdPrefix");
        if (!hasProviders)
            missing.push("webClient.mobile.authentication.identityProviders");
        throw new MobileConfigurationError(missing);
    }
    return {
        authority: endpoint,
        clientId: `${prefix}-${os}`,
        scope: config.openIdConnect.scope,
        callbacks: null,
    };
}

export const selectIsOffline = (s: ConfigState): boolean => {
    const webConfig = s.config?.webClient;
    if (!webConfig) return true;
    const offline = webConfig.offlineMode;
    if (!offline) return false;
    const clientIP = webConfig.clientIP ?? "";
    const start = DateTime.fromISO(offline.startDateTime);
    const end = offline.endDateTime
        ? DateTime.fromISO(offline.endDateTime)
        : DateTime.fromObject({ year: 2050, month: 12, day: 31 });
    const now = DateTime.now();
    return (
        now >= start &&
        now <= end &&
        !(offline.whitelist ?? []).includes(clientIP)
    );
};
