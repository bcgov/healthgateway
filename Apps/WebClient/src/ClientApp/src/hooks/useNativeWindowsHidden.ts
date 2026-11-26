import { useAppLockStore } from "@/stores/appLock.store";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";

/**
 * Reports whether native windows (modals, popovers, tooltips) must be hidden. Each is its own window above the app,
 * so it would cover the lock screen and the Update required dialog, which are drawn over the page.
 * @returns true while the app is locked or an update is required
 */
export function useNativeWindowsHidden(): boolean {
    const locked = useAppLockStore((s) => s.locked);
    const updateRequired = useConfigStore(selectUpdateRequired);
    return locked || updateRequired;
}
