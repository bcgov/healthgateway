import { createContext, useContext } from "react";
import { View } from "react-native";
import {
    GestureDetector,
    type PanGesture,
    PointerType,
    usePanGesture,
} from "react-native-gesture-handler";

import { swipeStep } from "@/utils/swipe";

import type { HgTab } from "./HgTabs";

/**
 * v-window `next()` / `prev()`: the next tab in that direction that isn't disabled. Windows stop at either end
 * (`continuous` is off).
 */
export function neighbourTab(
    tabs: HgTab[],
    value: number,
    step: -1 | 1
): HgTab | undefined {
    const at = tabs.findIndex((t) => t.value === value);
    if (at < 0) return undefined;
    for (let i = at + step; i >= 0 && i < tabs.length; i += step)
        if (!tabs[i].disabled) return tabs[i];
    return undefined;
}

/** The enclosing window's swipe, so a nested window takes the swipe first (Vuetify stops the touch at the inner one). */
const ParentWindowSwipe = createContext<PanGesture | undefined>(undefined);

interface Props {
    /** the same tabs as the HgTabs above the window, in the same order */
    tabs: HgTab[];
    value: number;
    onChange: (value: number) => void;
    /** the window's test id; its swipe gesture gets `<testID>-swipe` */
    testID?: string;
    className?: string;
    children: React.ReactNode;
}

/**
 * Port of v-window's touch support: swiping the content sideways moves to the neighbouring tab, as tapping its header
 * does (it runs the tab's `onPress` too). A native pan gesture on iOS and Android; on web it reacts to touch and pen,
 * not the mouse, as Vuetify's touch events do, and leaves vertical scrolling and text selection to the browser.
 *
 * Stated deviation: a swipe skips disabled tabs. In the web client, DependentCardComponent's windows stay swipeable
 * when their headers are disabled (an aged-out dependent's Export and Profile), the bug #6948 fixed in the legacy card.
 */
export function HgWindow({
    tabs,
    value,
    onChange,
    testID,
    className,
    children,
}: Props) {
    const parent = useContext(ParentWindowSwipe);
    const swipe = usePanGesture({
        enabled: tabs.filter((t) => !t.disabled).length > 1,
        activeOffsetX: [-10, 10],
        failOffsetY: [-10, 10],
        runOnJS: true,
        testID: testID ? `${testID}-swipe` : undefined,
        block: parent,
        onDeactivate: (e) => {
            if (e.canceled || e.pointerType === PointerType.MOUSE) return;
            const step = swipeStep(e.translationX, e.translationY);
            const tab = step ? neighbourTab(tabs, value, step) : undefined;
            if (!tab) return;
            tab.onPress?.();
            onChange(tab.value);
        },
    });
    return (
        <ParentWindowSwipe.Provider value={swipe}>
            <GestureDetector
                gesture={swipe}
                touchAction="pan-y"
                userSelect="auto"
            >
                <View testID={testID} className={className}>
                    {children}
                </View>
            </GestureDetector>
        </ParentWindowSwipe.Provider>
    );
}
