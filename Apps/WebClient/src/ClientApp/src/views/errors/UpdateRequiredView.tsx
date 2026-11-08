import { useEffect } from "react";
import { View } from "react-native";

import { AppText, Dialog } from "@/components/ui";
import { useAppLockStore } from "@/stores/appLock.store";

/**
 * `mobile.minimumVersion` is newer than this app. Replaces the whole app and cannot be dismissed. Native only (stated
 * deviation: web always runs the latest).
 */
export function UpdateRequiredView() {
    useEffect(() => {
        // A modal hides while locked, the root layout shows no lock screen in its place, and there is nothing
        // behind this one to protect. Keep clearing while shown: returning after the idle timeout locks again.
        const clear = () => {
            const lock = useAppLockStore.getState();
            if (lock.locked) lock.clearLock();
        };
        clear();
        return useAppLockStore.subscribe(clear);
    }, []);
    return (
        <View testID="updateRequired" className="flex-1 bg-background">
            <Dialog
                visible
                persistent
                title="Update required"
                onRequestClose={() => undefined}
            >
                <AppText testID="updateRequiredText">
                    A new version of Health Gateway is available and must be
                    installed to continue.
                </AppText>
            </Dialog>
        </View>
    );
}
