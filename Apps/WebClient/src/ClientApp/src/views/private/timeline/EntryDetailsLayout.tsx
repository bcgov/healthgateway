import {
    createContext,
    type ReactNode,
    useContext,
    useEffect,
    useState,
} from "react";
import { Platform, type ScrollViewProps, View } from "react-native";
import {
    KeyboardAwareScrollView,
    useFocusedInputHandler,
    useKeyboardHandler,
    useReanimatedKeyboardAnimation,
} from "react-native-keyboard-controller";
import Animated, {
    measure,
    useAnimatedRef,
    useAnimatedStyle,
    useSharedValue,
} from "react-native-reanimated";
import {
    SafeAreaView,
    useSafeAreaFrame,
    useSafeAreaInsets,
} from "react-native-safe-area-context";
import { scheduleOnRN } from "react-native-worklets";

import { useThemeColors } from "@/hooks/useTheme";

const PinnedFooterContext = createContext<
    ((footer: ReactNode) => void) | undefined
>(undefined);
const ClearanceBelowFieldContext = createContext<
    ((heightFromFieldTop: number | undefined) => void) | undefined
>(undefined);

/**
 * Within an `EntryDetailsLayout`, keeps the area from a focused field's top down to a given depth in sight above the
 * keyboard and pinned footer. Outside a layout, it does nothing.
 * @param heightFromFieldTop Distance from the field's top to the bottom of the area to keep in sight, such as the
 * Add Comment text input and the Save and Cancel buttons below it.
 * @param fieldShown Whether the field is rendered; pass false when it is hidden, since it then never fires onBlur.
 * @returns onFocus and onBlur handlers to spread onto the field.
 */
export function useKeyboardClearanceBelowField(
    heightFromFieldTop: number,
    fieldShown: boolean
) {
    const setHeightFromFieldTop = useContext(ClearanceBelowFieldContext);
    const [focused, setFocused] = useState(false);
    // a field unmounted while focused never fires onBlur
    if (!fieldShown && focused) setFocused(false);
    const reserving =
        setHeightFromFieldTop !== undefined && fieldShown && focused;
    useEffect(() => {
        if (!reserving) return;
        setHeightFromFieldTop(heightFromFieldTop);
        return () => setHeightFromFieldTop(undefined);
    }, [reserving, setHeightFromFieldTop, heightFromFieldTop]);
    return {
        onFocus: () => setFocused(true),
        onBlur: () => setFocused(false),
    };
}

/**
 * Pins a footer under the enclosing `EntryDetailsLayout`'s scroll view while mounted, as CommentSectionComponent.vue's
 * `fixed-bottom` does.
 * @param footer Content to pin, or undefined to pin nothing.
 * @returns True when pinned; otherwise the caller renders the footer in place.
 */
export function usePinnedFooter(footer: ReactNode | undefined): boolean {
    const setFooter = useContext(PinnedFooterContext);
    const pinned = setFooter !== undefined && footer !== undefined;
    useEffect(() => {
        if (!setFooter || footer === undefined) return;
        setFooter(footer);
        return () => setFooter(undefined);
    }, [setFooter, footer]);
    return pinned;
}

// The keyboard library keeps the caret in sight, so the space a field asks for from its top is reduced by how far down
// the field the caret is.
function useSpaceBelowCaret() {
    const [heightFromFieldTop, setHeightFromFieldTop] = useState<number>();
    const [caretBottom, setCaretBottom] = useState(0);
    useFocusedInputHandler(
        {
            onSelectionChange: (e) => {
                "worklet";
                // Bottom of the caret's line, from the field's top. Tracked for every field, since a field reports its
                // caret as it gains focus, before its onFocus reaches JS and asks for space.
                scheduleOnRN(setCaretBottom, e.selection.end.y);
            },
        },
        []
    );
    return {
        space:
            heightFromFieldTop === undefined
                ? 0
                : Math.max(heightFromFieldTop - caretBottom, 0),
        setHeightFromFieldTop,
    };
}

// How far the footer rises to sit on the keyboard, less what is below the view (Android's tab bar) and restInset, the
// footer's bottom padding the keyboard may cover. What is below is measured as the keyboard opens because at layout
// Android can read the page's position before it is on screen.
function useFooterOnKeyboard(restInset: number) {
    const viewRef = useAnimatedRef<View>();
    const { height: keyboardHeight } = useReanimatedKeyboardAnimation();
    const frame = useSafeAreaFrame();
    const windowBottom = frame.y + frame.height;
    const belowView = useSharedValue(0);
    const [belowViewOnJs, setBelowViewOnJs] = useState(0);
    useKeyboardHandler(
        {
            onStart: (e) => {
                "worklet";
                if (e.height === 0) return;
                const layout = measure(viewRef);
                if (!layout) return;
                const below = Math.max(
                    windowBottom - layout.pageY - layout.height,
                    0
                );
                if (below === belowView.value) return;
                belowView.value = below;
                scheduleOnRN(setBelowViewOnJs, below);
            },
        },
        [windowBottom]
    );
    const footerStyle = useAnimatedStyle(() => ({
        transform: [
            {
                // The library's keyboard height is negative while the keyboard is up
                translateY: -Math.max(
                    -keyboardHeight.value - belowView.value - restInset,
                    0
                ),
            },
        ],
    }));
    return {
        viewRef,
        footerStyle,
        keyboardNotOverContent: belowViewOnJs + restInset,
    };
}

// While the keyboard is up on native, the pinned footer rides on it and the scroll view gains matching room past the
// entry, so a focused field scrolls clear of both. iOS draws the page behind the tab bar and home indicator, so the
// footer rests on their inset; Android's tab page already ends above them. On web the browser keeps the field in sight.
export function EntryDetailsLayout({
    children,
    ...scrollProps
}: ScrollViewProps & { children: ReactNode }) {
    const colors = useThemeColors();
    const insets = useSafeAreaInsets();
    const [footer, setFooter] = useState<ReactNode>();
    const [footerHeight, setFooterHeight] = useState(0);
    const spaceBelowCaret = useSpaceBelowCaret();
    const restInset =
        footer !== undefined && Platform.OS === "ios" ? insets.bottom : 0;
    const { viewRef, footerStyle, keyboardNotOverContent } =
        useFooterOnKeyboard(restInset);
    const footerContent = (
        <View
            testID="entryDetailsFooter"
            className="p-4"
            onLayout={(e) => setFooterHeight(e.nativeEvent.layout.height)}
        >
            {footer}
        </View>
    );
    return (
        <Animated.View
            ref={viewRef}
            style={{ flex: 1, backgroundColor: colors.surface }}
        >
            <KeyboardAwareScrollView
                style={{ flex: 1 }}
                contentContainerStyle={{ flexGrow: 1 }}
                keyboardShouldPersistTaps="handled"
                // Keeps the caret clear of the footer, plus the space the field asked for
                bottomOffset={
                    (footer === undefined ? 0 : footerHeight) +
                    spaceBelowCaret.space
                }
                // The library pads past the content by the keyboard's height; less this, it matches the footer's rise
                extraKeyboardSpace={-keyboardNotOverContent}
                {...scrollProps}
            >
                <PinnedFooterContext.Provider value={setFooter}>
                    <ClearanceBelowFieldContext.Provider
                        value={spaceBelowCaret.setHeightFromFieldTop}
                    >
                        {children}
                    </ClearanceBelowFieldContext.Provider>
                </PinnedFooterContext.Provider>
            </KeyboardAwareScrollView>
            {footer === undefined ? null : (
                // Opaque, so content scrolling under the rising footer is hidden
                <Animated.View
                    style={[
                        {
                            backgroundColor: colors.surface,
                            paddingBottom: restInset,
                        },
                        footerStyle,
                    ]}
                >
                    {Platform.OS === "web" ? (
                        // The dialog already keeps clear of the screen's edges (ModalSafeArea)
                        footerContent
                    ) : (
                        // Landscape: clear of the notch and the side navigation bar
                        <SafeAreaView edges={["left", "right"]}>
                            {footerContent}
                        </SafeAreaView>
                    )}
                </Animated.View>
            )}
        </Animated.View>
    );
}
