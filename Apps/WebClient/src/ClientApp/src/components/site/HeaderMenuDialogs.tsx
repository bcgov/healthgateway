import { useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform } from "react-native";

import { toHref } from "@/navigation/href";
import { useAuthStore } from "@/stores/auth.store";
import { useHeaderMenuStore } from "@/stores/headerMenu.store";
import { NOTIFICATION_CENTRE_SWITCHED_ON } from "@/stores/notification.store";

import { FeedbackDialog } from "./FeedbackDialog";
import { NotificationCentre } from "./NotificationCentre";
import { RatingDialog } from "./RatingDialog";
import { logOut } from "./useHeaderMenu";

/**
 * Native: what the header menu opens, mounted once in the shell (the menu itself is the platform's, inside each
 * navigation bar): Feedback, the rating asked for before logging out, and the notification centre. Signed in only, as
 * the account menu that opens them.
 */
export function HeaderMenuDialogs() {
    const router = useRouter();
    const dialog = useHeaderMenuStore((s) => s.dialog);
    const hideDialog = useHeaderMenuStore((s) => s.hideDialog);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

    // Signed out (logged out, or the session ended while a dialog was open): the dialogs close and unmount, so nothing,
    // a Feedback draft included, carries over to the next sign-in; that sign-in starts with the bell's badge showing.
    useEffect(() => {
        if (isAuthenticated) return;
        useHeaderMenuStore.setState({ dialog: undefined, bellSeen: false });
    }, [isAuthenticated]);

    if (!isAuthenticated) return null;
    return (
        <>
            <FeedbackDialog
                visible={dialog === "feedback"}
                onClose={hideDialog}
            />
            <RatingDialog
                visible={dialog === "rating"}
                onClose={() => {
                    hideDialog();
                    // let the modal finish dismissing before the route changes (iOS otherwise can leave a stuck
                    // invisible modal window that blocks touches)
                    setTimeout(
                        () => logOut((path) => router.push(toHref(path))),
                        Platform.OS === "ios" ? 400 : 0
                    );
                }}
            />
            {NOTIFICATION_CENTRE_SWITCHED_ON ? <NotificationCentre /> : null}
        </>
    );
}
