import type { ConfigContext, ExpoConfig } from "expo/config";

import withUserScriptSandboxingOff from "./plugins/withUserScriptSandboxingOff";
import appVersion from "./version.json";

type AppEnv = "dev" | "test" | "prod";
const appEnv = (process.env.APP_ENV ?? "dev") as AppEnv;

// `npm run version:bump` raises these. BUILD_NUMBER lets a build pipeline set the build number instead.
const build = Number(process.env.BUILD_NUMBER) || appVersion.build;
// The stores' version is major.minor.fix, and the build number is separate (ios.buildNumber, android.versionCode)
const fix = (appVersion as { fix?: number }).fix ?? 0;

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
    version: `${appVersion.major}.${appVersion.minor}.${fix}`,
    extra: {
        ...config.extra,
        // The app shows v<version>.<build> (AppVersion), the build zero-padded as in the pipeline's run name; web has no
        // native build number to read it from
        build,
        // Shown after the version ("Custom": a pipeline build not from dev). Not part of `version`: the stores take digits only.
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
        // Gradle rejects a versionCode below 1, and local builds have build 0 (version.json) unless BUILD_NUMBER is set
        versionCode: Math.max(build, 1),
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
