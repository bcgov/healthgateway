import { useEffect, useState } from "react";
import {
    AppState,
    BackHandler,
    Keyboard,
    Platform,
    ScrollView,
    useWindowDimensions,
    View,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { HgButton } from "@/components/common";
import { DevBanner } from "@/components/site/DevBanner";
import { useColorScheme } from "@/hooks/useTheme";
import { useLoginLabel } from "@/navigation/login";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { decorativeProps } from "@/utils/decorative";
import { LandingIntro } from "@/views/public/landing/LandingIntro";

/** Old iOS app, ticket HAPP-637: the first Face ID prompt fails if asked for as the view appears. */
const AUTO_PROMPT_DELAY_MS = 500;

/**
 * Looks like the landing page, which the user recognises as the way in: the system prompt opens over it by itself, and
 * passing it uncovers the app where they left it. The full login is the way out, for a failed or cancelled prompt; so is
 * restarting the app, which asks again.
 */
function LockedView() {
    const unlock = useAppLockStore((s) => s.unlock);
    const authenticating = useAppLockStore((s) => s.authenticating);
    const signOut = useAuthStore((s) => s.signOut);
    const insets = useSafeAreaInsets();
    const { width } = useWindowDimensions();
    const Logo = useColorScheme() === "dark" ? HgLogoDark : HgLogo;
    const logoWidth = Math.min(224, width - 180);
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
                void unlock();
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
        <View className="flex-1" style={{ paddingTop: insets.top }}>
            <DevBanner />
            <View className="bg-app-bar border-b-2 border-border-light flex-row items-center h-16 px-2">
                <Logo
                    width={logoWidth}
                    height={logoWidth / 3.1924}
                    // decorative: the page text names the app
                    {...decorativeProps()}
                />
            </View>
            <ScrollView contentContainerClassName="p-8">
                <LandingIntro />
                <HgButton
                    testID="lockLogInAgainBtn"
                    text={loginLabel}
                    uppercase={false}
                    disabled={signingOut || authenticating}
                    onPress={() => {
                        setSigningOut(true);
                        useAppLockStore.setState({ beforeUnlock: undefined });
                        // the lock lifts once the session is gone (useAppLockLifecycle); the guard goes to login
                        useAuthStore.setState({ loginRequest: {} });
                        void signOut();
                    }}
                />
            </ScrollView>
        </View>
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
