import { useCallback, useContext, useEffect, useRef, useState } from "react";
import { Animated, RefreshControl, type ScrollView, View } from "react-native";
import Reanimated from "react-native-reanimated";
import { SafeAreaView } from "react-native-safe-area-context";

import { AppText, LiveRegion } from "@/components/ui";
import { useMotion } from "@/hooks/useMotion";
import { notifyScrollDragEnd } from "@/hooks/usePressHighlight";
import { useRouteFeaturesEnabled } from "@/hooks/useRouteFeaturesEnabled";
import { useThemeColors } from "@/hooks/useTheme";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { playRefreshFeedback } from "@/utils/refreshFeedback";

import { PageContainer } from "./PageContainer";
import {
    PageRefreshContext,
    PageRetrievingContext,
    PageScrollContext,
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
 * Native: each page owns its scroll view so the native tab bar can sit below the shell's header and
 * read the scroll (Liquid Glass minimises on scroll down, content runs behind the translucent bar).
 * No footer on native: its links live in the header menus' Support entry (`SupportMenuItem.tsx`).
 * Web (`PageScroll.web.tsx`): a pass-through; the shell's single scroll view scrolls the page.
 * Both hold the page's content back on a route whose feature is switched off (`useRouteFeaturesEnabled`).
 */
export function PageScroll({ children }: { children: React.ReactNode }) {
    const onScroll = useContext(PageScrollContext);
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
    const onRefresh = async () => {
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
            setRefreshing(false);
            // this change is also what saves the copy
            useConnectivityStore.setState({ pullInProgress: false });
        }
    };
    return (
        <View className="flex-1">
            <Reanimated.ScrollView
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
                onScroll={onScroll}
                scrollEventThrottle={16}
                // a card touched as the drag began stays shaded until the finger lifts (usePressHighlight)
                onScrollEndDrag={notifyScrollDragEnd}
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
            </Reanimated.ScrollView>
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
