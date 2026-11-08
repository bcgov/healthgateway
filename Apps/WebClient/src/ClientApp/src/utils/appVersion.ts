const parse = (version: string | undefined): number[] | undefined => {
    if (!version || !/^\d+(\.\d+)*$/.test(version.trim())) return undefined;
    return version.trim().split(".").map(Number);
};

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
