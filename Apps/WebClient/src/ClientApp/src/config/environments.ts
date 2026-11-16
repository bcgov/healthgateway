import type { TrackingConfig } from "@/services/tracking/trackingService";

export type EnvironmentId = "local" | "dev" | "test" | "prod";

export interface Environment {
    id: EnvironmentId;
    /** in the Environment Selector */
    name: string;
    /** the banner on mobile; production has none */
    banner?: string;
    configUrl: string;
    /** Where this environment's analytics go */
    tracking: TrackingConfig;
}

/**
 * The web client's values (`public/snowplow.js`), the same on every environment: its analytics are told apart by the
 * page's host, so native reports the matching host too.
 */
const tracking = (host: string): TrackingConfig => ({
    enabled: true,
    collector: "spt.apps.gov.bc.ca",
    appId: "Snowplow_standalone",
    origin: `https://${host}`,
});

/** Native. Everything else about an environment comes from its `/configuration`. */
export const environments: Environment[] = [
    {
        id: "local",
        name: "Local",
        banner: "Local Development",
        // The .NET WebClient (3025) answers /configuration; Metro (5002) only serves the bundle. It and the service
        // endpoints the configuration lists are localhost on every platform: an Android emulator or a phone on USB
        // reaches them through `adb reverse` (scripts/mobile-dev.js; README, "Android port forwarding").
        configUrl:
            process.env.EXPO_PUBLIC_LOCAL_CONFIG_URL ||
            "http://localhost:3025/configuration",
        // reports as Development
        tracking: tracking("dev.healthgateway.gov.bc.ca"),
    },
    {
        id: "dev",
        name: "Development",
        banner: "Development",
        configUrl: "https://dev.healthgateway.gov.bc.ca/configuration",
        tracking: tracking("dev.healthgateway.gov.bc.ca"),
    },
    {
        id: "test",
        name: "Test",
        banner: "Test",
        configUrl: "https://test.healthgateway.gov.bc.ca/configuration",
        tracking: tracking("test.healthgateway.gov.bc.ca"),
    },
    {
        id: "prod",
        name: "Production",
        configUrl: "https://www.healthgateway.gov.bc.ca/configuration",
        tracking: tracking("www.healthgateway.gov.bc.ca"),
    },
];

/**
 * Production's gateway says which build was last released to the public on both stores (Apps/MobileEnvironment,
 * environmentSelector.md). Always Production's, whichever environment the app is on, so a lower environment can never
 * widen the choice. `EXPO_PUBLIC_ENVIRONMENTS_URL` points a test build at another gateway (hg.dev.api.gov.bc.ca).
 */
export const ENVIRONMENTS_URL =
    process.env.EXPO_PUBLIC_ENVIRONMENTS_URL ||
    "https://hg.api.gov.bc.ca/api/mobileservice/environment";

/** What a release build above the released one offers (store reviewers and staff), and starts on. */
export const UNRELEASED_ENVIRONMENTS: readonly EnvironmentId[] = [
    "dev",
    "test",
    "prod",
];
export const UNRELEASED_DEFAULT: EnvironmentId = "test";

/** Development builds: every environment, Local (the services this computer runs, e.g. the Aspire host) first. */
export const DEVELOPMENT_ENVIRONMENTS: readonly EnvironmentId[] =
    environments.map((e) => e.id);
export const DEVELOPMENT_DEFAULT: EnvironmentId = "local";

/** These environments, in the selector's order. */
export const environmentsOf = (ids: readonly EnvironmentId[]): Environment[] =>
    environments.filter((e) => ids.includes(e.id));

export const isEnvironmentId = (value: unknown): value is EnvironmentId =>
    environments.some((e) => e.id === value);
