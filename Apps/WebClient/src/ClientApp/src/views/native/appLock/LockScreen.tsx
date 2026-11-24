import { useEffect, useState } from "react";
import {
    AppState,
    BackHandler,
    Keyboard,
    Platform,
    ScrollView,
    View,
} from "react-native";

import { HgButton } from "@/components/common";
import { OverAppScreen } from "@/components/site/OverAppScreen";
import { useLoginLabel } from "@/navigation/login";
import { describeError, logger } from "@/services/logger";
import { useAppLockStore } from "@/stores/appLock.store";
import { signOutFromLock } from "@/stores/auth.store";
import { LandingIntro } from "@/views/public/landing/LandingIntro";

/** Old iOS app, ticket HAPP-637: the first Face ID prompt fails if asked for as the view appears. */
const AUTO_PROMPT_DELAY_MS = 500;

/**
 * Looks like the landing page, which the user recognises as the way in: the system prompt opens over it by itself, and
 * passing it uncovers the app where they left it. After a cancelled prompt, the login button asks again, as ending the login
 * takes the same prompt, and then signs out to a new login; restarting the app also asks again.
 */
function LockedView() {
    const unlock = useAppLockStore((s) => s.unlock);
    const authenticating = useAppLockStore((s) => s.authenticating);
    const [signingOut, setSigningOut] = useState(false);
    const loginLabel = useLoginLabel();

    useEffect(() => {
        Keyboard.dismiss();
        // Android back must not navigate the pages underneath
        const back = BackHandler.addEventListener(
            "hardwareBackPress",
            () => true
        );
        let asked = false;
        let timer: ReturnType<typeof setTimeout> | undefined;
        const askSoon = () => {
            clearTimeout(timer);
            timer = setTimeout(() => {
                if (asked || AppState.currentState !== "active") return;
                asked = true;
                unlock().catch((error: unknown) =>
                    logger.warn(`Could not unlock: ${describeError(error)}`)
                );
            }, AUTO_PROMPT_DELAY_MS);
        };
        askSoon();
        // locked while not in front: nothing else would ask once it is
        const foreground = AppState.addEventListener("change", (state) => {
            if (state === "active") askSoon();
        });
        return () => {
            back.remove();
            foreground.remove();
            clearTimeout(timer);
        };
        // once per lock
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <OverAppScreen>
            <ScrollView contentContainerClassName="p-8">
                <LandingIntro />
                <HgButton
                    testID="lockLogInAgainBtn"
                    text={loginLabel}
                    uppercase={false}
                    disabled={signingOut || authenticating}
                    onPress={() => {
                        setSigningOut(true);
                        // ending the login takes the same prompt as unlocking; turned down, the lock stays as it was
                        signOutFromLock()
                            .then((signedOut) => {
                                if (!signedOut) setSigningOut(false);
                            })
                            .catch((error: unknown) => {
                                logger.warn(
                                    `Could not sign out: ${describeError(error)}`
                                );
                                setSigningOut(false);
                            });
                    }}
                />
            </ScrollView>
        </OverAppScreen>
    );
}

/**
 * Covers the mounted app while locked, so navigation and form state survive. Rendered above everything at the root;
 * `AppModal` hides the modals, which would otherwise be above it. Web has no lock.
 */
export function LockScreen() {
    const locked = useAppLockStore((s) => s.locked);
    if (Platform.OS === "web" || !locked) return null;
    return (
        <View
            testID="lockScreen"
            className="absolute inset-0 z-50 bg-background"
        >
            <LockedView />
        </View>
    );
}

/** Keeps screen readers out of the app underneath while something covers it (the lock, or the intro: `covered`). */
export function LockGate({
    covered = false,
    children,
}: {
    covered?: boolean;
    children: React.ReactNode;
}) {
    const locked = useAppLockStore((s) => s.locked);
    if (Platform.OS === "web") return children;
    const hidden = locked || covered;
    return (
        <View
            className="flex-1"
            accessibilityElementsHidden={hidden}
            importantForAccessibility={hidden ? "no-hide-descendants" : "auto"}
        >
            {children}
        </View>
    );
}
