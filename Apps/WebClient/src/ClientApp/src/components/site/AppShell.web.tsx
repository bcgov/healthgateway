import { usePathname } from "expo-router";
import {
    useCallback,
    useEffect,
    useLayoutEffect,
    useRef,
    useState,
} from "react";
import { Animated, ScrollView, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { selectIsLoading, useLoadingStore } from "@/stores/loading.store";

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
 * Whether the page has put anything in its container yet. Each route's code loads on its own (Expo Router's async
 * routes, with an empty Suspense fallback), so the shell draws first with nothing in it. A MutationObserver rather
 * than onLayout, which needs ResizeObserver.
 */
function usePageDrawn() {
    const [content, setContent] = useState<React.ComponentRef<
        typeof View
    > | null>(null);
    const [drawn, setDrawn] = useState(false);
    useLayoutEffect(() => {
        const node = content as unknown as HTMLElement | null;
        if (!node) return;
        const update = () => setDrawn(node.childElementCount > 0);
        update();
        const observer = new MutationObserver(update);
        observer.observe(node, { childList: true });
        return () => observer.disconnect();
    }, [content]);
    return [setContent, drawn] as const;
}

/**
 * Whether the phone footer may be shown. It is part of the scroll content, so on a page that is still empty (its
 * code loading) or still loading its data (`Loading`, the overlay) it would sit at the bottom of the screen and then
 * be pushed down when the page arrived: a layout shift of about 0.23 on the landing page and 0.3 on Terms of Service.
 * Once shown it stays for that path, so a dialog's loading does not take it away again.
 */
function usePhoneFooterReady(pathname: string) {
    const [contentRef, drawn] = usePageDrawn();
    const loading = useLoadingStore(selectIsLoading);
    const [readyOn, setReadyOn] = useState<string>();
    // set while rendering (React's "adjusting state when a prop changes"), so the footer comes in the same paint
    if (drawn && !loading && readyOn !== pathname) setReadyOn(pathname);
    return [contentRef, readyOn === pathname] as const;
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
    const pathname = usePathname();
    useFocusMainOnRouteChange(pathname);
    const [pageContentRef, phoneFooterReady] = usePhoneFooterReady(pathname);

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
                                <PageContainer contentRef={pageContentRef}>
                                    {children}
                                </PageContainer>
                            </View>
                        </ScrollToTopContext.Provider>
                        {/* not before the page is in: see usePhoneFooterReady (wider windows keep it outside the scroll) */}
                        {isMobile && !hideFooter && phoneFooterReady
                            ? footer
                            : null}
                    </ScrollView>
                    {!isMobile && !hideFooter ? footer : null}
                </View>
                {isMobile && !hideSidebar ? <Sidebar /> : null}
            </View>
            <IdleTimeout />
        </SafeAreaView>
    );
}
