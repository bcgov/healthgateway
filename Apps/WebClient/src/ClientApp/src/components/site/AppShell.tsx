import { View } from "react-native";
import Reanimated from "react-native-reanimated";
import { SafeAreaView } from "react-native-safe-area-context";

import { DevBanner } from "./DevBanner";
import { Header } from "./Header";
import { notifyIdleActivity } from "./idleActivity";
import { IdleTimeout } from "./IdleTimeout";
import { usePageChrome } from "./pageChrome";
import { PageScrollContext } from "./PageScrollContext";
import { SessionBanner } from "./SessionBanner";
import { useCollapsibleHeader } from "./useCollapsibleHeader";

/**
 * Native shell: dev banner and the collapsible app bar above the routes. Main navigation is the platform
 * bottom tab bar (`app/(tabs)/_layout.tsx`) instead of the web client's drawer, so there is no sidebar or
 * hamburger here, and each page scrolls itself (`PageScroll`) so the tab bar can react to it.
 * The routes run to the screen's sides: the tab bar only moves into iPhone Duo's vertical rail if its controller reaches
 * that edge, so the shell pads its own bars and `PageScroll` pads each page's content. Web: `AppShell.web.tsx`.
 */
export function AppShell({ children }: { children: React.ReactNode }) {
    const { hideChrome } = usePageChrome();
    const header = useCollapsibleHeader();

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
                <Reanimated.View style={header.style}>
                    <View onLayout={header.onLayout}>
                        <SafeAreaView edges={["left", "right"]}>
                            <Header />
                        </SafeAreaView>
                    </View>
                </Reanimated.View>
            ) : null}
            {!hideChrome ? (
                <SafeAreaView edges={["left", "right"]}>
                    <SessionBanner />
                </SafeAreaView>
            ) : null}
            <PageScrollContext.Provider value={header.onScroll}>
                <View className="flex-1">{children}</View>
            </PageScrollContext.Provider>
            <IdleTimeout />
        </SafeAreaView>
    );
}
