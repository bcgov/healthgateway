import { useIsFocused } from "expo-router";
import { HeaderHeightContext } from "expo-router/react-navigation";
import { use, useCallback, useEffect, useRef, useState } from "react";
import {
    Animated,
    Platform,
    RefreshControl,
    ScrollView,
    View,
} from "react-native";
import {
    SafeAreaView,
    useSafeAreaInsets,
} from "react-native-safe-area-context";

import { AppText, LiveRegion } from "@/components/common";
import { useMotion } from "@/hooks/useMotion";
import { useRouteFeaturesEnabled } from "@/hooks/useRouteFeaturesEnabled";
import { useThemeColors } from "@/hooks/useTheme";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useNavBarStore } from "@/stores/navBar.store";
import { usePageRefreshStore } from "@/stores/pageRefresh.store";
import { playRefreshFeedback } from "@/utils/refreshFeedback";

import { PageContainer } from "./PageContainer";
import {
    PageRefreshContext,
    PageRetrievingContext,
    type RefreshHandler,
    ScrollToTopContext,
    ScrollToViewContext,
} from "./PageScrollContext";

/** Long enough to read, however quickly the refresh finishes. */
const REFRESH_INDICATOR_MIN_MS = 1200;
/** How long a pill stays: it says the work started; the refresh spinner or the page's placeholders show it carrying on. */
const PILL_MS = 1200;

/** `set(true)` shows the pill for `PILL_MS`; `set(false)` hides it sooner, when the work ends first. */
function usePillFlash() {
    const [shown, setShown] = useState(false);
    const timer = useRef<ReturnType<typeof setTimeout>>(undefined);
    useEffect(() => () => clearTimeout(timer.current), []);
    const set = useCallback((active: boolean) => {
        clearTimeout(timer.current);
        setShown(active);
        if (active) timer.current = setTimeout(() => setShown(false), PILL_MS);
    }, []);
    return [shown, set] as const;
}

/** Floats over the page: the short visual half of the refresh feedback (`playRefreshFeedback` is the rest), or the first fetch. */
function StatusPill({ testID, label }: { testID: string; label: string }) {
    const [opacity] = useState(() => new Animated.Value(0));
    const fadeMs = useMotion().ms(150);
    useEffect(() => {
        Animated.timing(opacity, {
            toValue: 1,
            duration: fadeMs,
            useNativeDriver: true,
        }).start();
    }, [opacity, fadeMs]);
    return (
        <Animated.View
            testID={testID}
            pointerEvents="none"
            className="absolute top-2 self-center z-10 rounded-full bg-primary px-4 py-1.5 shadow-lg"
            style={{ opacity }}
        >
            {/* a status, not an alert: said when the reader is free, without interrupting */}
            <LiveRegion politeness="polite">
                <AppText variant="body-medium" className="text-on-primary">
                    {label}
                </AppText>
            </LiveRegion>
        </Animated.View>
    );
}

/**
 * Native: each page owns its scroll view so the native tab bar and navigation bar can read the scroll (Liquid Glass
 * minimises on scroll down, content runs behind the translucent bar).
 * No footer on native: its links live in the header menus' Support entry (`useHeaderMenu`).
 * Web (`PageScroll.web.tsx`): a pass-through; the shell's single scroll view scrolls the page.
 * Both hold the page's content back on a route whose feature is switched off (`useRouteFeaturesEnabled`).
 * Native: while the page is on screen, its pull to refresh is also the navigation bar's Refresh button
 * (`usePageRefreshStore`).
 */
export function PageScroll({ children }: { children: React.ReactNode }) {
    const scroll = useRef<React.ComponentRef<typeof ScrollView>>(null);
    const scrollToTop = useCallback(
        () => scroll.current?.scrollTo({ y: 0, animated: false }),
        []
    );
    const scrollToView = useCallback(
        (view: React.ComponentRef<typeof View>) => {
            const content = scroll.current;
            const inner = content?.getInnerViewRef();
            if (!content || !inner) return;
            view.measureLayout(inner, (_x, y) =>
                content.scrollTo({ y, animated: true })
            );
        },
        []
    );
    const colors = useThemeColors();
    const enabled = useRouteFeaturesEnabled();
    const [refresh, setRefresh] = useState<RefreshHandler>();
    const [refreshing, setRefreshing] = useState(false);
    const [refreshPill, flashRefreshPill] = usePillFlash();
    const [retrieving, setRetrieving] = usePillFlash();
    const syncPill = useConnectivityStore((s) => s.syncPill);
    // a function in state must be wrapped, or React calls it as an updater
    const register = useCallback(
        (handler: RefreshHandler | undefined) => setRefresh(() => handler),
        []
    );
    // set at once, so a second pull or Refresh press in the same moment cannot start another before state catches up
    const inFlight = useRef(false);
    const onRefresh = async () => {
        if (inFlight.current) return;
        inFlight.current = true;
        setRefreshing(true);
        flashRefreshPill(true);
        useConnectivityStore.setState({ pullInProgress: true });
        void playRefreshFeedback();
        const shownLongEnough = new Promise((resolve) =>
            setTimeout(resolve, REFRESH_INDICATOR_MIN_MS)
        );
        try {
            await refresh?.();
            await shownLongEnough;
        } finally {
            inFlight.current = false;
            setRefreshing(false);
            // this change is also what saves the copy
            useConnectivityStore.setState({ pullInProgress: false });
        }
    };

    // the navigation bar's Refresh button runs the same refresh, while this page is the one on screen
    const focused = useIsFocused();
    // the bar's height, for `BrandHeader`'s bar over the app; 0 or none where the page has no bar. Android: React
    // Navigation counts the status bar into it, which our bars sit below (`disableTopInsetApplication`)
    const headerHeight = use(HeaderHeightContext);
    const statusBarInset = useSafeAreaInsets().top;
    useEffect(() => {
        if (!focused || !headerHeight) return;
        const height =
            Platform.OS === "android"
                ? headerHeight - statusBarInset
                : headerHeight;
        if (height > 0) useNavBarStore.setState({ height });
    }, [focused, headerHeight, statusBarInset]);
    const latestRefresh = useRef(onRefresh);
    useEffect(() => {
        latestRefresh.current = onRefresh;
    });
    const refreshable = refresh !== undefined;
    useEffect(() => {
        if (!focused || !refreshable) return;
        const run = () => void latestRefresh.current();
        usePageRefreshStore.setState({ refresh: run });
        return () => {
            if (usePageRefreshStore.getState().refresh === run)
                usePageRefreshStore.setState({ refresh: undefined });
        };
    }, [focused, refreshable]);
    useEffect(() => {
        if (!focused || !refreshing) return;
        usePageRefreshStore.setState({ refreshing: true });
        return () => usePageRefreshStore.setState({ refreshing: false });
    }, [focused, refreshing]);
    return (
        <View className="flex-1">
            <ScrollView
                ref={scroll}
                testID="pageScroll"
                refreshControl={
                    refresh ? (
                        <RefreshControl
                            refreshing={refreshing}
                            onRefresh={onRefresh}
                            tintColor={colors.primary}
                            colors={[colors.primary]}
                        />
                    ) : undefined
                }
                style={{ flex: 1, backgroundColor: colors.background }}
                contentContainerStyle={{
                    flexGrow: 1,
                    justifyContent: "space-between",
                }}
                contentInsetAdjustmentBehavior="automatic"
                keyboardShouldPersistTaps="handled"
            >
                <ScrollToViewContext.Provider value={scrollToView}>
                    <ScrollToTopContext.Provider value={scrollToTop}>
                        <PageRefreshContext.Provider value={register}>
                            <PageRetrievingContext.Provider
                                value={setRetrieving}
                            >
                                {/* inside the scroll view, so its background still reaches the sides */}
                                <SafeAreaView
                                    edges={["left", "right"]}
                                    style={{
                                        flexGrow: 1,
                                        justifyContent: "space-between",
                                    }}
                                >
                                    <PageContainer>
                                        {enabled ? children : null}
                                    </PageContainer>
                                </SafeAreaView>
                            </PageRetrievingContext.Provider>
                        </PageRefreshContext.Provider>
                    </ScrollToTopContext.Provider>
                </ScrollToViewContext.Provider>
            </ScrollView>
            {refreshPill ? (
                <StatusPill testID="pageRefreshIndicator" label="Refreshing…" />
            ) : retrieving ? (
                <StatusPill testID="loading-toast" label="Retrieving…" />
            ) : syncPill ? (
                <StatusPill testID="syncingPill" label="Syncing…" />
            ) : null}
        </View>
    );
}
