import { usePathname } from "expo-router";
import { useEffect } from "react";
import type { LayoutChangeEvent } from "react-native";
import {
    useAnimatedScrollHandler,
    useAnimatedStyle,
    useSharedValue,
    withTiming,
} from "react-native-reanimated";

import { useMotion } from "@/hooks/useMotion";

import { type NativeScrollState, nextNativeHeaderState } from "./headerScroll";

/** Vuetify's app bar hide/show transition. */
const HEADER_TRANSITION_MS = 200;

/**
 * v-app-bar scroll-behavior="hide" on native: the bar slides away while scrolling down past the threshold and returns on
 * a scroll back up or near the top (`nextNativeHeaderState`); the layout reclaims its height meanwhile, as Vuetify's v-main
 * padding does. The bar is always back after a route change. Tracking the scroll and animating the height both run on the
 * UI thread, so the bar keeps up even when JavaScript is busy. Web: `useCollapsibleHeader.web.ts`.
 */
export function useCollapsibleHeader() {
    const pathname = usePathname();
    const transitionMs = useMotion().ms(HEADER_TRANSITION_MS);
    const headerShown = useSharedValue(1);
    const headerHeight = useSharedValue(64);
    const scrollState = useSharedValue<NativeScrollState>({
        shown: true,
        anchorY: 0,
        contentHeight: 0,
        viewportHeight: 0,
    });

    useEffect(() => {
        scrollState.set((state) => ({ ...state, shown: true, anchorY: 0 }));
        headerShown.set(withTiming(1, { duration: transitionMs }));
    }, [pathname, scrollState, headerShown, transitionMs]);

    const onScroll = useAnimatedScrollHandler((e) => {
        const previous = scrollState.get();
        const next = nextNativeHeaderState(
            previous,
            e.contentOffset.y,
            e.contentSize.height,
            e.layoutMeasurement.height
        );
        scrollState.set(next);
        if (next.shown !== previous.shown)
            headerShown.set(
                withTiming(next.shown ? 1 : 0, {
                    duration: transitionMs,
                })
            );
    });

    const onLayout = (e: LayoutChangeEvent) => {
        // ignore layouts reported while collapsed; a 0 here would pin the bar shut
        const h = Math.round(e.nativeEvent.layout.height);
        if (h > 0) headerHeight.set(h);
    };

    const style = useAnimatedStyle(() => ({
        overflow: "hidden",
        height: headerShown.get() * headerHeight.get(),
    }));

    return { onScroll, onLayout, style };
}
