import { useCallback, useEffect, useId, useRef, useState } from "react";
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

const supportsAnchorPositioning = () =>
    typeof CSS !== "undefined" && CSS.supports("position-area", "right");

/**
 * Web: hovering the "?" icon shows the tooltip; a click (or Enter / Space) pins it open until the next click anywhere,
 * and hovering is ignored after that click until the pointer leaves the icon and returns. The bubble is a popover in
 * the top layer, placed beside the icon by CSS anchor positioning and flipped to its other side when it does not fit.
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
    const bubbleId = useId();
    const anchorName = `--info-tooltip-${bubbleId.replace(/[^\w-]/g, "")}`;
    const open = pinned || hovered || overBubble;
    const closeAll = () => {
        setPinned(false);
        setHovered(false);
        setOverBubble(false);
    };
    // Escape dismisses it without moving the pointer or the focus (WCAG 1.4.13)
    useEscapeKey(open, closeAll);

    // a click anywhere unpins it (the next tick, so the pinning click does not)
    useEffect(() => {
        if (!pinned) return;
        const unpin = () => {
            setPinned(false);
            setOverBubble(false);
        };
        const t = setTimeout(() =>
            document.addEventListener("click", unpin, { once: true })
        );
        return () => {
            clearTimeout(t);
            document.removeEventListener("click", unpin);
        };
    }, [pinned]);

    const showBubble = useCallback(
        (node: HTMLDivElement | null) => {
            bubbleRef.current = node;
            if (!node) return;
            // without anchor positioning the popover keeps its default place, the middle of the window
            if (supportsAnchorPositioning()) {
                node.style.setProperty("position-anchor", anchorName);
                node.style.setProperty("position-area", "right");
                node.style.setProperty("position-try-fallbacks", "flip-inline");
                node.style.setProperty("margin", "0");
            }
            if ("showPopover" in node) node.showPopover();
        },
        [anchorName]
    );

    return (
        <>
            <Pressable
                ref={(node) => {
                    (node as unknown as HTMLElement | null)?.style.setProperty(
                        "anchor-name",
                        anchorName
                    );
                }}
                testID={testID}
                nativeID={nativeID}
                accessibilityRole="button"
                accessibilityLabel={accessibilityLabel}
                aria-expanded={open}
                {...(open ? ({ "aria-describedby": bubbleId } as object) : {})}
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
            {open ? (
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
