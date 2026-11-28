import { requireOptionalNativeModule } from "expo-modules-core";

/** One kind of old data: how many items were there, and how many of them were removed or could not be. */
export interface LegacyCleanupGroup {
    group: string;
    found: number;
    removed: number;
    failed: number;
}

/**
 * Removes what the old Health Gateway apps (myhealthBC) left on the device: see `legacy.md`, `ios/LegacyCleanup.swift`
 * and the Android `LegacyCleanup.kt`. The first launch of an install runs it in native code, before JavaScript starts.
 * Null where it is not built in.
 */
export interface HgLegacyCleanupNative {
    /** What this launch's cleanup did, once it has finished. Null when it did not run: it had already been done. */
    legacyCleanupReport(): Promise<LegacyCleanupGroup[] | null>;
    /** Runs the same deletes now, whether or not they ran before (Reset App). */
    removeLegacyData(): Promise<LegacyCleanupGroup[]>;
}

export default requireOptionalNativeModule<HgLegacyCleanupNative>(
    "HgLegacyCleanup"
);
