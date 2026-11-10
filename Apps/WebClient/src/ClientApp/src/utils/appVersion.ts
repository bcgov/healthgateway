const parse = (version: string | undefined): number[] | undefined => {
    if (!version || !/^\d+(\.\d+)*$/.test(version.trim())) return undefined;
    return version.trim().split(".").map(Number);
};

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

/** Whether `current` is older than `minimum` (both major.minor.fix), comparing major, then minor, then fix. Unreadable versions: no. */
export function isUpdateRequired(
    current: string | undefined,
    minimum: string | undefined
): boolean {
    const have = parse(current);
    const need = parse(minimum);
    if (!have || !need) return false;
    for (let i = 0; i < Math.max(have.length, need.length); i++) {
        const difference = (have[i] ?? 0) - (need[i] ?? 0);
        if (difference !== 0) return difference < 0;
    }
    return false;
}
