import { useCallback, useEffect, useRef, useState } from "react";
import { Platform } from "react-native";

/**
 * The shortest time a press shows: React Native's own minimum (Pressability's DEFAULT_MIN_PRESS_DURATION).
 * react-native-web starts and ends a quick tap in the same tick, so without it a tap on web shades nothing.
 */
export const MinPressDuration = 130;

/**
 * Native: the longest a touch taken over by a scroll stays shaded with no word of the finger lifting, in case a
 * scroll view that holds the card does not call `notifyScrollDragEnd`, or the system cancelled the touch.
 */
export const MaxCancelledTouchHold = 5000;

const dragEndListeners = new Set<() => void>();

/**
 * Native: the finger that dragged a scroll view has lifted. Pass it as `onScrollEndDrag` on a scroll view that holds
 * cards: once a native scroll takes over a touch, React Native cancels the touch for JS, and only the scroll view
 * hears the finger lift.
 */
export function notifyScrollDragEnd() {
    [...dragEndListeners].forEach((listener) => listener());
}

export interface PressHighlightHandlers {
    onHoverIn: () => void;
    onHoverOut: () => void;
    onPressIn: () => void;
    onPressOut: () => void;
    onTouchStart: () => void;
    onTouchEnd: () => void;
    onTouchCancel: () => void;
}

/**
 * The surfaceHover shade of a clickable card, for the mouse and for touch. The web client shades its cards with
 * `:hover`, which a mobile browser also applies on a tap; react-native-web ignores hover from touch, and native has
 * none, so the shade comes from the press as well. A touch keeps it from the moment the finger is down until it
 * lifts, though the finger moves off the card or scrolls the page (which ends the press itself).
 * Spread `handlers` on the card's Pressable; a Pressable inside it that takes the card's press needs only
 * `onPressIn` and `onPressOut`, as its touches reach the card.
 */
export function usePressHighlight(): {
    highlighted: boolean;
    pressed: boolean;
    handlers: PressHighlightHandlers;
} {
    const [hovered, setHovered] = useState(false);
    const [pressed, setPressed] = useState(false);
    const pressedAt = useRef(0);
    // a finger is down on the card: the press may end before it lifts (moved off, or scrolling)
    const touching = useRef(false);
    const release = useRef<ReturnType<typeof setTimeout>>(undefined);
    const stopWaiting = useRef<() => void>(undefined);

    useEffect(
        () => () => {
            clearTimeout(release.current);
            stopWaiting.current?.();
        },
        []
    );

    const start = useCallback(() => {
        clearTimeout(release.current);
        stopWaiting.current?.();
        pressedAt.current = Date.now();
        setPressed(true);
    }, []);
    const end = useCallback(() => {
        clearTimeout(release.current);
        stopWaiting.current?.();
        const remaining = MinPressDuration - (Date.now() - pressedAt.current);
        if (remaining > 0)
            release.current = setTimeout(() => setPressed(false), remaining);
        else setPressed(false);
    }, []);

    const onHoverIn = useCallback(() => setHovered(true), []);
    const onHoverOut = useCallback(() => setHovered(false), []);
    const onPressIn = start;
    const onPressOut = useCallback(() => {
        if (!touching.current) end();
    }, [end]);
    const onTouchStart = useCallback(() => {
        touching.current = true;
        start();
    }, [start]);
    const lifted = useCallback(() => {
        touching.current = false;
        end();
    }, [end]);
    const onTouchCancel = useCallback(() => {
        // web: the browser keeps sending touch events through a scroll, so a cancel is the end of the touch
        if (Platform.OS === "web") {
            lifted();
            return;
        }
        // native: a scroll took the touch over (its press ends too, before or after this); the finger is still down
        // until the scroll view's drag ends
        stopWaiting.current?.();
        dragEndListeners.add(lifted);
        const fallback = setTimeout(lifted, MaxCancelledTouchHold);
        stopWaiting.current = () => {
            dragEndListeners.delete(lifted);
            clearTimeout(fallback);
            stopWaiting.current = undefined;
        };
    }, [lifted]);

    return {
        highlighted: hovered || pressed,
        pressed,
        handlers: {
            onHoverIn,
            onHoverOut,
            onPressIn,
            onPressOut,
            onTouchStart,
            onTouchEnd: lifted,
            onTouchCancel,
        },
    };
}
