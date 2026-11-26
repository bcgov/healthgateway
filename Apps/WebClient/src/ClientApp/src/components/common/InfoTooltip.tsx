import {
    useCallback,
    useEffect,
    useId,
    useLayoutEffect,
    useRef,
    useState,
} from "react";
import { Pressable, View } from "react-native";

import { useEscapeKey } from "@/hooks/useEscapeKey";

import {
    InfoTooltipContent,
    InfoTooltipIcon,
    type InfoTooltipProps,
} from "./infoTooltip.shared";

export type { InfoTooltipProps } from "./infoTooltip.shared";

// the space between the icon and the bubble: the popover's padding, so the pointer can cross it onto the bubble
const GAP = 10;

// module-level variable storing the function that closes whichever tooltip is actively shown
// to ensure only one tooltip is shown at a time
let closeActiveTooltip: (() => void) | null = null;

// fallback behaviour: place below the icon, else above, measured once when it opens (it does not follow a scroll)
function placeBelowOrAbove(bubble: HTMLElement, trigger: HTMLElement | null) {
    if (!trigger) return;
    const icon = trigger.getBoundingClientRect();
    const { offsetWidth: width, offsetHeight: height } = bubble;
    const top =
        icon.bottom + height <= window.innerHeight
            ? icon.bottom
            : icon.top - height;
    const left = icon.left + icon.width / 2 - width / 2;
    bubble.style.inset = "auto";
    bubble.style.top = `${Math.max(0, top)}px`;
    bubble.style.left = `${Math.max(0, Math.min(left, window.innerWidth - width))}px`;
}

/**
 * Web: hovering the "?" icon shows the tooltip; a click (or Enter / Space) pins it open until the next click anywhere,
 * and hovering is ignored after that click until the pointer leaves the icon and returns. Only one tooltip is shown at a
 * time, so showing one, by any means, closes any other. The bubble is a popover in the top layer, placed beside the icon
 * by CSS anchor positioning and flipped to its other side when it does not fit. Browsers without anchor positioning
 * place it below the icon, or above when it does not fit, measured once when it opens.
 * iOS: `InfoTooltip.ios.tsx`; Android: `InfoTooltip.android.tsx`.
 */
export function InfoTooltip({
    text,
    children,
    size,
    maxWidth = 300,
    testID,
    nativeID,
    tooltipTestID,
    icon,
    color,
    accessibilityLabel = "More information",
}: InfoTooltipProps) {
    const [pinned, setPinned] = useState(false);
    const [hovered, setHovered] = useState(false);
    // the pointer may move onto the bubble and read it (WCAG 1.4.13)
    const [overBubble, setOverBubble] = useState(false);
    const bubbleRef = useRef<HTMLDivElement | null>(null);
    const triggerRef = useRef<HTMLElement | null>(null);
    const bubbleId = useId();
    const anchorName = `--info-tooltip-${bubbleId.replace(/[^\w-]/g, "")}`;
    const isOpen = pinned || hovered || overBubble;
    const close = useCallback(() => {
        setPinned(false);
        setHovered(false);
        setOverBubble(false);
    }, []);

    // Escape dismisses the tooltip without moving the pointer or the focus (WCAG 1.4.13)
    useEscapeKey(isOpen, close);

    // close any currently active tooltip before painting this one
    useLayoutEffect(() => {
        if (!isOpen) return;

        closeActiveTooltip?.();

        // since this tooltip is now active, store a reference to its close function
        closeActiveTooltip = close;

        return () => {
            // during cleanup, if this is the active tooltip, the module-level variable needs to be cleared
            // to allow this tooltip to be opened again in the future
            if (closeActiveTooltip === close) closeActiveTooltip = null;
        };
    }, [isOpen, close]);

    // a click anywhere unpins it (the next tick, so the pinning click does not). It listens in the capture phase because
    // a Pressable (such as another tooltip's icon) stops its click's propagation before it bubbles up to the document.
    useEffect(() => {
        if (!pinned) return;
        const unpin = (event: MouseEvent) => {
            // a click on its own icon is left to the icon's press, which toggles it
            if (
                event.target instanceof Node &&
                triggerRef.current?.contains(event.target)
            )
                return;
            setPinned(false);
            setOverBubble(false);
        };
        const t = setTimeout(() =>
            document.addEventListener("click", unpin, true)
        );
        return () => {
            clearTimeout(t);
            document.removeEventListener("click", unpin, true);
        };
    }, [pinned]);

    const showBubble = useCallback(
        (node: HTMLDivElement | null) => {
            bubbleRef.current = node;
            if (!node) return;

            // override default margin that centres it in the window
            node.style.setProperty("margin", "0");

            const anchorPositioningIsSupported =
                typeof CSS !== "undefined" &&
                CSS.supports("position-area", "right");

            if (anchorPositioningIsSupported) {
                node.style.setProperty("position-anchor", anchorName);
                node.style.setProperty("position-area", "right");
                node.style.setProperty("position-try-fallbacks", "flip-inline");
            }

            if ("showPopover" in node) node.showPopover();

            // after showing it, as a hidden popover has no size to measure
            if (!anchorPositioningIsSupported)
                placeBelowOrAbove(node, triggerRef.current);
        },
        [anchorName]
    );

    return (
        <>
            <Pressable
                ref={(node) => {
                    const element = node as unknown as HTMLElement | null;
                    triggerRef.current = element;
                    element?.style.setProperty("anchor-name", anchorName);
                }}
                testID={testID}
                nativeID={nativeID}
                accessibilityRole="button"
                accessibilityLabel={accessibilityLabel}
                aria-expanded={isOpen}
                {...(isOpen
                    ? ({ "aria-describedby": bubbleId } as object)
                    : {})}
                onPress={() => {
                    setPinned((v) => !v);
                    setHovered(false);
                }}
                onHoverIn={() => setHovered(true)}
                onHoverOut={(event) => {
                    // leaving towards the bubble keeps it open
                    const to = (event as unknown as MouseEvent).relatedTarget;
                    if (to instanceof Node && bubbleRef.current?.contains(to))
                        setOverBubble(true);
                    setHovered(false);
                }}
                className="cursor-pointer"
            >
                <InfoTooltipIcon icon={icon} size={size} color={color} />
            </Pressable>
            {isOpen ? (
                <div
                    ref={showBubble}
                    popover="manual"
                    onMouseEnter={() => setOverBubble(true)}
                    onMouseLeave={() => setOverBubble(false)}
                    style={{
                        border: "none",
                        padding: GAP,
                        background: "transparent",
                        overflow: "visible",
                    }}
                >
                    <View
                        testID={tooltipTestID}
                        nativeID={bubbleId}
                        role="tooltip"
                        className="bg-tooltip-background rounded px-4 py-[5px]"
                        style={{ maxWidth }}
                    >
                        <InfoTooltipContent text={text}>
                            {children}
                        </InfoTooltipContent>
                    </View>
                </div>
            ) : null}
        </>
    );
}
