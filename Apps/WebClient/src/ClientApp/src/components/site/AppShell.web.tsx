import { usePathname } from "expo-router";
import { useCallback, useEffect, useRef } from "react";
import { Animated, ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useBreakpoint } from "@/hooks/useBreakpoint";

import { DevBanner } from "./DevBanner";
import { Footer } from "./Footer";
import { Header } from "./Header";
import { IdleTimeout } from "./IdleTimeout";
import { usePageChrome } from "./pageChrome";
import { PageContainer } from "./PageContainer";
import { ScrollToTopContext } from "./PageScrollContext";
import { Sidebar } from "./Sidebar";
import { SkipLink } from "./SkipLink";
import { useCollapsibleHeader } from "./useCollapsibleHeader.web";

/**
 * After a client-side route change the focus would stay on the link that was pressed, in the drawer; move it to the
 * main landmark so the keyboard and the screen reader start at the new page (WCAG 2.4.3). `usePageTitle` announces
 * the new title. Not on the first render: that is the page load.
 */
function useFocusMainOnRouteChange(pathname: string) {
    const first = useRef(true);
    useEffect(() => {
        if (first.current) {
            first.current = false;
            return;
        }
        const frame = requestAnimationFrame(() =>
            document.getElementById("main")?.focus({ preventScroll: true })
        );
        return () => cancelAnimationFrame(frame);
    }, [pathname]);
}

/**
 * Port of App.vue: dev banner, app bar, navigation drawer, main container, footer. With the landmarks a screen reader
 * lists (`banner`, `navigation`, `main`, `contentinfo`) and a skip link first (stated deviations: the web client has
 * no skip link and no `<main>`).
 */
export function AppShell({ children }: { children: React.ReactNode }) {
    const { isMobile } = useBreakpoint();
    const { hideChrome, hideFooter, hideSidebar } = usePageChrome();
    const header = useCollapsibleHeader();
    const scroll = useRef<React.ComponentRef<typeof ScrollView>>(null);
    const scrollToTop = useCallback(
        () => scroll.current?.scrollTo({ y: 0, animated: false }),
        []
    );
    useFocusMainOnRouteChange(usePathname());

    const footer = (
        <View accessibilityRole="contentinfo">
            <Footer />
        </View>
    );
    return (
        <SafeAreaView
            className="flex-1 bg-background"
            edges={["top", "left", "right"]}
        >
            <SkipLink />
            <DevBanner />
            {!hideChrome ? (
                <Animated.View style={header.style}>
                    <View onLayout={header.onLayout} accessibilityRole="banner">
                        <Header />
                    </View>
                </Animated.View>
            ) : null}
            <View className="flex-1 flex-row">
                {!isMobile && !hideSidebar ? <Sidebar /> : null}
                <View className="flex-1">
                    <ScrollView
                        ref={scroll}
                        className="flex-1"
                        contentContainerClassName="flex-grow justify-between"
                        keyboardShouldPersistTaps="handled"
                        onScroll={header.onScroll}
                        scrollEventThrottle={16}
                    >
                        <ScrollToTopContext.Provider value={scrollToTop}>
                            {/* the skip link's target, focused after a route change; not a tab stop itself */}
                            <View
                                accessibilityRole="main"
                                nativeID="main"
                                tabIndex={-1}
                                style={{ outlineStyle: "none" } as object}
                            >
                                <PageContainer>{children}</PageContainer>
                            </View>
                        </ScrollToTopContext.Provider>
                        {isMobile && !hideFooter ? footer : null}
                    </ScrollView>
                    {!isMobile && !hideFooter ? footer : null}
                </View>
                {isMobile && !hideSidebar ? <Sidebar /> : null}
            </View>
            <IdleTimeout />
        </SafeAreaView>
    );
}
