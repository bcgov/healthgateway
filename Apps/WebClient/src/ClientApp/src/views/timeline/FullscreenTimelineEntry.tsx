import { useEffect, useRef } from "react";
import {
    Platform,
    Pressable,
    ScrollView,
    useWindowDimensions,
    View,
} from "react-native";
import {
    GestureDetector,
    GestureHandlerRootView,
    usePanGesture,
} from "react-native-gesture-handler";
import Reanimated, {
    useAnimatedStyle,
    useSharedValue,
    withSpring,
    withTiming,
} from "react-native-reanimated";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { scheduleOnRN } from "react-native-worklets";

import { ErrorCard } from "@/components/site/ErrorCard";
import { Icon, ModalSafeArea } from "@/components/ui";
import { AppModal } from "@/components/ui/AppModal";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useMotion } from "@/hooks/useMotion";
import { useThemeColors } from "@/hooks/useTheme";
import { useTimelineStore } from "@/stores/timeline.store";

import { TimelineEntryView } from "./entries";

/**
 * iOS back gesture: a native pan on a strip along the left edge. The strip sits above the scroll view, so the two never
 * compete for a touch, and it matches the content's 24pt indent, so it covers nothing tappable. It starts under the
 * toolbar to leave the back button clear.
 */
const SWIPE_STRIP_WIDTH = 24;
const TOOLBAR_HEIGHT = 48;

/** Reaches back over the safe area's padding: in landscape the screen's edge is outside the padded content. */
function SwipeBackStrip({
    gesture,
}: {
    gesture: ReturnType<typeof usePanGesture>;
}) {
    const { left } = useSafeAreaInsets();
    return (
        <GestureDetector gesture={gesture}>
            <View
                testID="swipeBackStrip"
                collapsable={false}
                className="absolute bottom-0"
                style={{
                    left: -left,
                    top: TOOLBAR_HEIGHT,
                    width: SWIPE_STRIP_WIDTH + left,
                }}
            />
        </GestureDetector>
    );
}

/** The swipe closes the entry past a third of the width, or on a flick to the right. */
export function completesSwipeBack(
    dx: number,
    vx: number,
    width: number
): boolean {
    "worklet";
    return dx > width / 3 || vx > 0.5;
}

/**
 * Port of FullscreenTimelineEntryComponent.vue: on mobile an entry opens full screen with a back button. Web: the
 * dialog pushes a `?details` history entry so the browser's back button closes it, as the web client does.
 */
export function FullscreenTimelineEntry({
    hdid,
    commentsAreEnabled = false,
}: {
    hdid: string;
    commentsAreEnabled?: boolean;
}) {
    const colors = useThemeColors();
    const { width } = useWindowDimensions();
    const { inlineEntryDetails } = useBreakpoint();
    const entry = useTimelineStore((s) => s.fullscreenEntry);
    const close = useTimelineStore((s) => s.closeFullscreenEntry);
    const open = useTimelineStore((s) => s.openFullscreenEntry);
    const newestInlineEntry = useTimelineStore((s) => s.inlineEntries.at(-1));
    // read before the cards remove themselves from the list, which happens as the window narrows
    const newestInlineEntryRef = useRef(newestInlineEntry);
    const visible = entry !== undefined;

    // web: simulate a history push so Back closes the dialog
    useEffect(() => {
        if (Platform.OS !== "web" || !visible) return;
        globalThis.history.pushState({}, "Entry Details", "?details");
        const onPop = () => close();
        globalThis.addEventListener("popstate", onPop);
        return () => globalThis.removeEventListener("popstate", onPop);
    }, [visible, close]);

    function closeDialog() {
        if (!visible) return;
        if (Platform.OS === "web") globalThis.history.back();
        else close();
    }

    // iOS has no system back gesture for a modal, so a left-edge swipe closes the entry as a pushed screen's would;
    // Android's back gesture already arrives as onRequestClose. All of it runs natively, off the JavaScript thread.
    const dragX = useSharedValue(0);
    const motion = useMotion();
    const reduced = motion.reduced;
    const closeMs = motion.ms(150);
    useEffect(() => {
        if (visible) dragX.set(0);
    }, [visible, dragX]);
    const swipeBack = usePanGesture({
        activeOffsetX: 10,
        failOffsetY: [-20, 20],
        onUpdate: (e) => {
            "worklet";
            dragX.set(Math.max(0, e.translationX));
        },
        onDeactivate: (e) => {
            "worklet";
            if (
                !e.canceled &&
                completesSwipeBack(e.translationX, e.velocityX / 1000, width)
            )
                dragX.set(
                    withTiming(width, { duration: closeMs }, (finished) => {
                        if (finished) scheduleOnRN(close);
                    })
                );
            else
                dragX.set(
                    reduced
                        ? withTiming(0, { duration: 0 })
                        : withSpring(0, { overshootClamping: true })
                );
        },
    });
    const dragStyle = useAnimatedStyle(() => ({
        transform: [{ translateX: dragX.get() }],
    }));

    // leaving the mobile layout closes it (native: the entry's card has just opened itself, see TimelineEntryCard).
    // Native, stated deviation: when the window narrows the entry expanded last carries on here, full screen.
    useEffect(() => {
        if (inlineEntryDetails) {
            if (visible) closeDialog();
        } else if (Platform.OS !== "web" && newestInlineEntryRef.current)
            open(newestInlineEntryRef.current);
        // eslint-disable-next-line react-hooks/exhaustive-deps -- closeDialog only reads current values
    }, [inlineEntryDetails]);
    useEffect(() => {
        newestInlineEntryRef.current = newestInlineEntry;
    }, [newestInlineEntry]);

    return (
        <AppModal
            supportedOrientations={["portrait", "landscape"]}
            visible={visible}
            animationType="slide"
            onRequestClose={closeDialog}
        >
            <GestureHandlerRootView style={{ flex: 1 }}>
                {/* the surface also fills the safe areas around the padded content */}
                <Reanimated.View
                    style={[
                        { flex: 1, backgroundColor: colors.surface },
                        dragStyle,
                    ]}
                >
                    <ModalSafeArea>
                        <View
                            testID="entryDetailsModal"
                            nativeID="entry-details-modal"
                            className="flex-1 bg-surface"
                        >
                            {/* v-toolbar density="compact" with the back button */}
                            <View className="min-h-12 flex-row items-center px-1 border-b border-border-divider">
                                <Pressable
                                    testID="backBtn"
                                    accessibilityRole="button"
                                    accessibilityLabel="Back"
                                    onPress={closeDialog}
                                    className="w-11 h-11 items-center justify-center"
                                >
                                    <Icon
                                        name="arrow-left"
                                        size={20}
                                        color={colors["bc-black"]}
                                    />
                                </Pressable>
                            </View>
                            {entry ? (
                                <ScrollView
                                    className="flex-1"
                                    contentContainerClassName="flex-grow"
                                >
                                    <View className="mx-2">
                                        <ErrorCard />
                                    </View>
                                    <TimelineEntryView
                                        testID="entryDetailsCard"
                                        entry={entry}
                                        hdid={hdid}
                                        isMobileDetails
                                        commentsAreEnabled={commentsAreEnabled}
                                    />
                                </ScrollView>
                            ) : null}
                            {Platform.OS === "ios" ? (
                                <SwipeBackStrip gesture={swipeBack} />
                            ) : null}
                        </View>
                    </ModalSafeArea>
                </Reanimated.View>
            </GestureHandlerRootView>
        </AppModal>
    );
}
