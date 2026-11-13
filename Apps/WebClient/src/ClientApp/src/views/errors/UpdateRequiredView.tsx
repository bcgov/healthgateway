import { usePathname, useRouter } from "expo-router";
import { useEffect } from "react";

import { AppText, Dialog, HgButton } from "@/components/common";
import { storeListingUrl } from "@/config/storeListings";
import { Path } from "@/constants/path";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { openExternal } from "@/utils/openExternal";

/**
 * This app's build is below `mobile.minimumBuild`: a dialog over the page the app is on (the landing page at start-up),
 * which cannot be dismissed; Update opens the store listing. Once the update is installed the app starts on the new
 * build, which passes the check. Native only (stated deviation: web always runs the latest).
 */
export function UpdateRequiredView() {
    const storeUrl = storeListingUrl();
    const router = useRouter();
    const pathname = usePathname();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    // Start-up stops before sign-in, and the route guard is off until the update: a link that opened the app on another
    // page shows the landing page behind the dialog instead
    useEffect(() => {
        if (!isAuthenticated && pathname !== Path.Root)
            router.replace(Path.Root);
    }, [isAuthenticated, pathname, router]);
    useEffect(() => {
        // A modal hides while locked. Signed out (start-up stops before sign-in), there is nothing behind this one to
        // protect, so the lock is cleared, also after the idle timeout locks again. Signed in (the minimum rose while
        // the app was open), the lock screen covers the user's page as usual, and the dialog shows once unlocked.
        const clear = () => {
            const lock = useAppLockStore.getState();
            if (lock.locked && !useAuthStore.getState().isAuthenticated)
                lock.clearLock();
        };
        clear();
        return useAppLockStore.subscribe(clear);
    }, []);
    return (
        <Dialog
            visible
            persistent
            testID="updateRequired"
            title="Update required"
            onRequestClose={() => undefined}
            actions={
                storeUrl ? (
                    <HgButton
                        testID="updateRequiredButton"
                        text="Update"
                        onPress={() => void openExternal(storeUrl)}
                    />
                ) : undefined
            }
        >
            <AppText testID="updateRequiredText">
                A new version of Health Gateway is available and must be
                installed to continue.
            </AppText>
        </Dialog>
    );
}
