import { type ReactNode, useEffect, useState } from "react";
import { Pressable, ScrollView, useWindowDimensions, View } from "react-native";
import {
    GestureDetector,
    GestureHandlerRootView,
    usePanGesture,
} from "react-native-gesture-handler";
import Reanimated, {
    useAnimatedStyle,
    useSharedValue,
    withTiming,
} from "react-native-reanimated";
import { scheduleOnRN } from "react-native-worklets";

import { AppModal } from "@/components/common/AppModal";
import { ModalSafeArea } from "@/components/common/ModalSafeArea";
import { useMotion } from "@/hooks/useMotion";

const SLIDE_MS = 220;
/** A share of the window always stays uncovered, so there is something to tap to close. */
const MAX_WIDTH_SHARE = 0.85;
/** A swipe closes past this share of the panel, or at this speed. */
const CLOSE_DISTANCE = 0.35;
const CLOSE_VELOCITY = 500;

interface Props {
    visible: boolean;
    onClose: () => void;
    children: ReactNode;
    /** Opened from the lock screen: shown while locked (`AppModal`). */
    overLock?: boolean;
}

/**
 * Native header menu (stated deviation: web keeps the v-menu card, `HeaderMenu.web.tsx`): a full-height panel that slides
 * in from the right, sized by its content. Collapsed entries still count towards that width (`ExpandableMenuItem`), so
 * opening one never resizes it. Closes with a tap outside, a swipe right or Android's back.
 */
export function HeaderMenu({
    visible,
    onClose,
    children,
    overLock = false,
}: Props) {
    const { width: windowWidth } = useWindowDimensions();
    const slideMs = useMotion().ms(SLIDE_MS);
    const progress = useSharedValue(0);
    const panelWidth = useSharedValue(windowWidth);
    // stays mounted while it slides away after `visible` turns false
    const [rendered, setRendered] = useState(visible);
    if (visible && !rendered) setRendered(true);

    useEffect(() => {
        progress.set(
            withTiming(visible ? 1 : 0, { duration: slideMs }, (finished) => {
                if (finished && !visible) scheduleOnRN(setRendered, false);
            })
        );
    }, [visible, progress, slideMs]);

    // Vertical movement stays with the scroll view inside. The drag and the slide run on the UI thread.
    const swipe = usePanGesture({
        activeOffsetX: 10,
        failOffsetY: [-15, 15],
        onUpdate: ({ translationX }) => {
            "worklet";
            progress.set(
                Math.max(0, Math.min(1, 1 - translationX / panelWidth.get()))
            );
        },
        onDeactivate: ({ translationX, velocityX, canceled }) => {
            "worklet";
            if (
                !canceled &&
                (translationX > panelWidth.get() * CLOSE_DISTANCE ||
                    velocityX > CLOSE_VELOCITY)
            ) {
                scheduleOnRN(onClose);
                return;
            }
            progress.set(withTiming(1, { duration: slideMs }));
        },
    });

    const backdropStyle = useAnimatedStyle(() => ({ opacity: progress.get() }));
    const panelStyle = useAnimatedStyle(() => ({
        transform: [{ translateX: (1 - progress.get()) * panelWidth.get() }],
    }));

    return (
        <AppModal
            // iOS: a modal supports portrait only unless told otherwise, which would rotate a landscape screen
            supportedOrientations={["portrait", "landscape"]}
            transparent
            visible={rendered}
            overLock={overLock}
            animationType="none"
            onRequestClose={onClose}
        >
            <GestureHandlerRootView className="flex-1">
                <Reanimated.View
                    style={[
                        {
                            position: "absolute",
                            top: 0,
                            right: 0,
                            bottom: 0,
                            left: 0,
                            backgroundColor: "rgba(0, 0, 0, 0.4)",
                        },
                        backdropStyle,
                    ]}
                >
                    <Pressable
                        testID="headerMenuBackdrop"
                        accessibilityRole="button"
                        accessibilityLabel="Close menu"
                        className="flex-1"
                        onPress={onClose}
                    />
                </Reanimated.View>
                <GestureDetector gesture={swipe}>
                    <Reanimated.View
                        testID="headerMenu"
                        onLayout={(e) =>
                            panelWidth.set(e.nativeEvent.layout.width)
                        }
                        style={[
                            {
                                position: "absolute",
                                top: 0,
                                right: 0,
                                bottom: 0,
                                maxWidth: windowWidth * MAX_WIDTH_SHARE,
                            },
                            panelStyle,
                        ]}
                    >
                        <View className="flex-1 bg-surface shadow-lg">
                            <ModalSafeArea sides="right">
                                <ScrollView
                                    contentContainerClassName="py-2"
                                    alwaysBounceVertical={false}
                                >
                                    {children}
                                </ScrollView>
                            </ModalSafeArea>
                        </View>
                    </Reanimated.View>
                </GestureDetector>
            </GestureHandlerRootView>
        </AppModal>
    );
}
