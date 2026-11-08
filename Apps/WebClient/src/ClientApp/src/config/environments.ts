import { Platform } from "react-native";

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

/** The Mac running the services, as an emulator or simulator reaches it. A physical phone needs the Mac's address. */
const LOCAL_HOST = Platform.OS === "android" ? "10.0.2.2" : "localhost";

/** Native. Everything else about an environment comes from its `/configuration`. */
export const environments: Environment[] = [
    {
        id: "local",
        name: "Local",
        banner: "Local Development",
        // The .NET WebClient (3025) answers /configuration; Metro (5002) only serves the bundle.
        configUrl:
            process.env.EXPO_PUBLIC_LOCAL_CONFIG_URL ||
            `http://${LOCAL_HOST}:3025/configuration`,
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

/** Local builds start on Development; anything else, on Production. */
export const defaultEnvironmentId = (): EnvironmentId =>
    __DEV__ ? "dev" : "prod";

/** Release builds allow no plain http, so Local only works, and only appears, in development builds. */
export const selectableEnvironments = (): Environment[] =>
    __DEV__ ? environments : environments.filter((e) => e.id !== "local");
