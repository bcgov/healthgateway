import { describeError, logger } from "@/services/logger";

import NativeCleanup, {
    type LegacyCleanupGroup,
} from "../../modules/hg-legacy-cleanup";

// Names and counts only: the native code reports nothing from the data itself, and anything else it sent is not logged.
const describeGroup = ({ group, removed, failed }: LegacyCleanupGroup) =>
    `${/^\w{1,32}$/.test(group) ? group : "other"} ${Number(removed)} removed` +
    (failed > 0 ? `, ${Number(failed)} failed` : "");

function logGroups(
    cleanupSource: "Legacy cleanup" | "Legacy cleanup at reset",
    groups: LegacyCleanupGroup[]
): void {
    const found = groups.filter((group) => group.found > 0);
    if (found.length === 0) {
        logger.debug(`${cleanupSource}: nothing found`);
        return;
    }
    const cleanupSummary = found.map(describeGroup).join("; ");
    if (found.some((group) => group.failed > 0))
        logger.warn(`${cleanupSource}: ${cleanupSummary}`);
    else logger.info(`${cleanupSource}: ${cleanupSummary}`);
}

/**
 * Logs what this launch's cleanup of the old Health Gateway apps' data did (`modules/hg-legacy-cleanup`, legacy.md).
 * The cleanup itself runs in native code before JavaScript starts. Nothing to log on web, in a dev client built before
 * the module was added, or once an earlier launch has finished the job.
 */
export async function logLegacyCleanup(): Promise<void> {
    if (!NativeCleanup) return;
    try {
        const groups = await NativeCleanup.legacyCleanupReport();
        if (groups) logGroups("Legacy cleanup", groups);
    } catch (error) {
        logger.warn(`Legacy cleanup report failed: ${describeError(error)}`);
    }
}

/**
 * Reset App: removes the old apps' data again, for an install where the launch's cleanup could not delete something.
 * A failure is logged, and the next launch tries again.
 */
export async function removeLegacyData(): Promise<void> {
    if (!NativeCleanup) return;
    try {
        logGroups(
            "Legacy cleanup at reset",
            await NativeCleanup.removeLegacyData()
        );
    } catch (error) {
        logger.warn(`Legacy cleanup at reset failed: ${describeError(error)}`);
    }
}
