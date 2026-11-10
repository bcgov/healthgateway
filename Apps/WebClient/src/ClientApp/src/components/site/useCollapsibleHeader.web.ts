import { usePathname } from "expo-router";
import { useEffect, useRef, useState } from "react";
import {
    Animated,
    type LayoutChangeEvent,
    type NativeScrollEvent,
    type NativeSyntheticEvent,
} from "react-native";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useMotion } from "@/hooks/useMotion";

import {
    initialWebScrollState,
    nextWebHeaderState,
    type WebScrollState,
} from "./headerScroll";

/** Vuetify's app bar hide/show transition. */
const HEADER_TRANSITION_MS = 200;

/**
 * v-app-bar scroll-behavior="hide" as HeaderComponent.vue sets it up (rules in `nextWebHeaderState`): the bar
 * slides away while scrolling down past the threshold and returns on a scroll up or near the top; the layout
 * reclaims its height meanwhile, as Vuetify's v-main padding does. The bar is always back after a route change,
 * and never hides at phone width.
 */
export function useCollapsibleHeader() {
    const pathname = usePathname();
    const { isMobile } = useBreakpoint();
    const transitionMs = useMotion().ms(HEADER_TRANSITION_MS);
    // created once; held in state rather than a ref so the compiler lint allows reading it in render
    const [headerShown] = useState(() => new Animated.Value(1));
    const state = useRef<WebScrollState>(initialWebScrollState);
    // how much of the bar shows (0 to 1), followed through the slide so the rule can measure the page with the bar shown
    const shownFraction = useRef(1);
    const [headerHeight, setHeaderHeight] = useState(64);

    useEffect(() => {
        const id = headerShown.addListener(({ value }) => {
            shownFraction.current = value;
        });
        return () => headerShown.removeListener(id);
    }, [headerShown]);

    const setHeaderShown = (show: boolean) => {
        Animated.timing(headerShown, {
            toValue: show ? 1 : 0,
            duration: transitionMs,
            useNativeDriver: false,
        }).start();
    };

    useEffect(() => {
        state.current = initialWebScrollState;
        setHeaderShown(true);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- reset the bar only when the route changes
    }, [pathname]);

    useEffect(() => {
        if (!isMobile || state.current.shown) return;
        state.current = { ...state.current, shown: true };
        setHeaderShown(true);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- show the bar when the window narrows to phone width
    }, [isMobile]);

    const onScroll = (e: NativeSyntheticEvent<NativeScrollEvent>) => {
        const { contentOffset, contentSize, layoutMeasurement } = e.nativeEvent;
        const previous = state.current;
        state.current = nextWebHeaderState(previous, {
            y: contentOffset.y,
            contentHeight: contentSize.height,
            viewportHeight: layoutMeasurement.height,
            headerHeight,
            barHeight: headerHeight * shownFraction.current,
            alwaysShown: isMobile,
        });
        if (state.current.shown !== previous.shown)
            setHeaderShown(state.current.shown);
    };

    const onLayout = (e: LayoutChangeEvent) => {
        // ignore layouts reported while collapsed; a 0 here would pin the bar shut
        const h = Math.round(e.nativeEvent.layout.height);
        if (h > 0) setHeaderHeight(h);
    };

    const style = {
        overflow: "hidden" as const,
        height: headerShown.interpolate({
            inputRange: [0, 1],
            outputRange: [0, headerHeight],
        }),
    };

    return { onScroll, onLayout, style };
}
