import { View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { DevBanner } from "./DevBanner";
import { HeaderMenuDialogs } from "./HeaderMenuDialogs";
import { notifyIdleActivity } from "./idleActivity";
import { IdleTimeout } from "./IdleTimeout";
import { usePageChrome } from "./pageChrome";
import { SessionBanner } from "./SessionBanner";

/**
 * Native shell: the environment banner and the session banner above the routes. Each stack draws the platform's own
 * navigation bar below them (`useNativeStackOptions`: logo, back, title, the account or settings menu), and main
 * navigation is the platform bottom tab bar (`app/(tabs)/_layout.tsx`) instead of the web client's drawer, so there is
 * no app bar, sidebar or hamburger here; each page scrolls itself (`PageScroll`) so the tab bar can react to it.
 * The routes run to the screen's sides: the tab bar only moves into iPhone Duo's vertical rail if its controller reaches
 * that edge, so the shell pads its own banners and `PageScroll` pads each page's content. Web: `AppShell.web.tsx`.
 */
export function AppShell({ children }: { children: React.ReactNode }) {
    const { hideChrome } = usePageChrome();

    return (
        <SafeAreaView
            className="flex-1 bg-background"
            edges={["top"]}
            // every touch anywhere counts as activity for the idle timeout (document events on web)
            onTouchStart={notifyIdleActivity}
        >
            <SafeAreaView edges={["left", "right"]}>
                <DevBanner />
            </SafeAreaView>
            {!hideChrome ? (
                <SafeAreaView edges={["left", "right"]}>
                    <SessionBanner />
                </SafeAreaView>
            ) : null}
            <View className="flex-1">{children}</View>
            <HeaderMenuDialogs />
            <IdleTimeout />
        </SafeAreaView>
    );
}
