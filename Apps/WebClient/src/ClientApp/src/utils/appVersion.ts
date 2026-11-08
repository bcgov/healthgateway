const parse = (version: string | undefined): number[] | undefined => {
    if (!version || !/^\d+(\.\d+)*$/.test(version.trim())) return undefined;
    return version.trim().split(".").map(Number);
};

/**
 * "major.minor (build)" for display, from the app's "major.minor.build" (any other text as it is), then " - label" when
 * there is one. The pipeline names the run major.minor.build[-custom] instead: it is also the image tag, which cannot
 * hold spaces or brackets (Build/hg-steps-template.yaml, "Set app version").
 */
export function formatAppVersion(version: string, label?: string): string {
    const match = /^(\d+)\.(\d+)\.(\d+)$/.exec(version.trim());
    const shown = match ? `${match[1]}.${match[2]} (${match[3]})` : version;
    return label ? `${shown} - ${label}` : shown;
}

/** Whether `current` is older than `minimum`, comparing major, then minor, then build. Unreadable versions: no. */
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
