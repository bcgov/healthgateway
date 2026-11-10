import type { ConfigContext, ExpoConfig } from "expo/config";

import withUserScriptSandboxingOff from "./plugins/withUserScriptSandboxingOff";
import appVersion from "./version.json";

type AppEnv = "dev" | "test" | "prod";
const appEnv = (process.env.APP_ENV ?? "dev") as AppEnv;

// `npm run version:bump` raises these. BUILD_NUMBER lets a build pipeline set the last part instead.
const build = Number(process.env.BUILD_NUMBER) || appVersion.build;

const idSuffix: Record<AppEnv, string> = {
    dev: ".dev",
    test: ".test",
    prod: "",
};
const nameSuffix: Record<AppEnv, string> = {
    dev: " Dev",
    test: " Test",
    prod: "",
};

export default ({ config }: ConfigContext): ExpoConfig =>
    withUserScriptSandboxingOff(expoConfig(config));

const expoConfig = (config: ConfigContext["config"]): ExpoConfig => ({
    ...config,
    name: `Health Gateway${nameSuffix[appEnv]}`,
    slug: "hgui",
    version: `${appVersion.major}.${appVersion.minor}.${build}`,
    // Shown after the build number ("Custom": a pipeline build not from dev). Not part of `version`: the stores take digits only.
    extra: {
        ...config.extra,
        versionLabel: (appVersion as { label?: string }).label,
    },
    orientation: "default",
    icon: "./assets/images/icon.png",
    // Custom URL scheme used for the native OIDC redirect (registered on the Keycloak "hg" client).
    scheme: "healthgateway",
    // Follows the OS appearance; the in-app Appearance picker can override it (see docs/theming.md).
    userInterfaceStyle: "automatic",
    ios: {
        bundleIdentifier: `ca.bc.gov.healthgateway${idSuffix[appEnv]}`,
        // Signing team for device builds (`expo run:ios --device`): local `.env`, so prebuild keeps it.
        appleTeamId: process.env.APPLE_TEAM_ID,
        buildNumber: String(build),
        supportsTablet: true,
        infoPlist: {
            ITSAppUsesNonExemptEncryption: false,
        },
    },
    android: {
        package: `ca.bc.gov.healthgateway${idSuffix[appEnv]}`,
        versionCode: build,
        predictiveBackGestureEnabled: false,
        // The old Android app's adaptive icon (myhealthBC-android `drawable/ic_launcher_foreground.xml`
        // on white): the full-bleed `icon` above was being cropped and masked by the launcher.
        adaptiveIcon: {
            foregroundImage:
                "./assets/images/android/adaptive-icon-foreground.png",
            backgroundColor: "#FFFFFF",
        },
    },
    web: {
        // the web client's fixed title; the native name above carries the environment
        name: "Health Gateway",
        output: "single",
        bundler: "metro",
        favicon: "./assets/images/favicon.png",
    },
    plugins: [
        "expo-router",
        [
            "expo-splash-screen",
            {
                backgroundColor: "#103362",
                image: "./assets/images/splash/splash-logo-with-title@3x.png",
                imageWidth: 192,
                resizeMode: "contain",
                // Android 12+ masks the system splash image to a circle, which cut the title to "ealth Gatewa":
                // give it the logo circle alone (navy-padded so the mask shows nothing else); the React splash
                // that follows draws the full artwork.
                android: {
                    image: "./assets/images/splash/splash-icon-android.png",
                    imageWidth: 110,
                },
            },
        ],
        [
            "expo-local-authentication",
            {
                faceIDPermission:
                    "Unlock Health Gateway with Face ID to keep your health information safe and secure.",
            },
        ],
        // playback only: no microphone permission on either platform
        [
            "expo-audio",
            { microphonePermission: false, recordAudioAndroid: false },
        ],
        "expo-secure-store",
        "expo-web-browser",
    ],
    experiments: {
        typedRoutes: true,
        reactCompiler: true,
    },
});
