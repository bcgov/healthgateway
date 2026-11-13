import { useEffect } from "react";
import { BackHandler, ScrollView, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { AppText, Dialog, HgButton } from "@/components/common";
import { BrandHeader } from "@/components/site/BrandHeader";
import { storeListingUrl } from "@/config/storeListings";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { openExternal } from "@/utils/openExternal";
import { LandingIntro } from "@/views/public/landing/LandingIntro";

/**
 * This app's build is below `mobile.minimumBuild`: a dialog that cannot be dismissed; Update opens the store listing.
 * Once the update is installed the app starts on the new build, which passes the check. Native only (stated deviation:
 * web always runs the latest).
 *
 * It is drawn over the screen (`Dialog` `inline`), below the lock screen, rather than in a modal window of its own: iOS
 * shows one modal window at a time, so one left open (a filter, a menu) could hide it. Those hide while an update is
 * required (`AppModal`). Signed in (the minimum rose while the app was open), it covers the user's page; signed out,
 * `UpdateRequiredLanding`.
 */
export function UpdateRequiredView() {
    const storeUrl = storeListingUrl();
    useEffect(() => {
        // Signed out there is nothing behind this to protect, so the lock is cleared, also after the idle timeout locks
        // again. Signed in, the lock screen covers the user's page as usual, and this shows once unlocked.
        const clear = () => {
            const lock = useAppLockStore.getState();
            if (lock.locked && !useAuthStore.getState().isAuthenticated)
                lock.clearLock();
        };
        clear();
        const unsubscribe = useAppLockStore.subscribe(clear);
        // Android's back must not move the page underneath (a modal window would have taken it)
        const back = BackHandler.addEventListener(
            "hardwareBackPress",
            () => true
        );
        return () => {
            unsubscribe();
            back.remove();
        };
    }, []);
    return (
        <View testID="updateRequiredOverlay" className="absolute inset-0 z-40">
            <Dialog
                visible
                inline
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
        </View>
    );
}

/**
 * Signed out with an update required: the landing page's text under the dialog, as on the lock screen. No page of the
 * app is mounted, so none can start a login (a `healthgateway://login` link opening the app, say) or load anything
 * before the update; start-up stops before sign-in.
 */
export function UpdateRequiredLanding() {
    const insets = useSafeAreaInsets();
    return (
        <View
            testID="updateRequiredLanding"
            className="flex-1 bg-background"
            style={{ paddingTop: insets.top }}
        >
            <BrandHeader />
            <ScrollView contentContainerClassName="p-8">
                <LandingIntro />
            </ScrollView>
        </View>
    );
}
