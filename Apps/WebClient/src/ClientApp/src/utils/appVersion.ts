import Constants from "expo-constants";

const isBuild = (value: unknown): value is number =>
    typeof value === "number" && Number.isInteger(value) && value >= 0;

/**
 * This app's build number (`extra.build`, baked in by app.config.ts: the 57 in v3.0.0.0057, the same on iOS and Android),
 * or undefined when there is none to read.
 */
export function appBuild(): number | undefined {
    const build = (
        Constants.expoConfig?.extra as { build?: unknown } | undefined
    )?.build;
    return isBuild(build) ? build : undefined;
}

/**
 * "v<major.minor.fix>.<build>" for display, the build zero-padded to 4 digits: the same as the WebClient pipeline's run
 * name (Build/hg-steps-template.yaml, "Set app version"), so a build can be matched to its run. Then " - label" when
 * there is one. A version that is not major.minor.fix, or no build number, shows what there is.
 */
export function formatAppVersion(
    version: string,
    build?: number,
    label?: string
): string {
    const trimmed = version.trim();
    const isVersion = /^\d+\.\d+\.\d+$/.test(trimmed);
    const hasBuild =
        typeof build === "number" && Number.isInteger(build) && build >= 0;
    const shown = isVersion
        ? `v${trimmed}${hasBuild ? `.${String(build).padStart(4, "0")}` : ""}`
        : version;
    return label ? `${shown} - ${label}` : shown;
}

/**
 * Whether this app's build number (`extra.build`, the 0057 in v3.0.0.0057, the same on iOS and Android) is below
 * `minimum`. A missing or unreadable build or minimum: no.
 */
export function isUpdateRequired(build: unknown, minimum: unknown): boolean {
    return isBuild(build) && isBuild(minimum) && build < minimum;
}
