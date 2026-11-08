import { useEffect, useId, useRef, useState } from "react";
import { Pressable, useWindowDimensions, View } from "react-native";

import { useEscapeKey } from "@/hooks/useEscapeKey";
import { useThemeColors } from "@/hooks/useTheme";

import { AppText } from "./AppText";
import { Icon } from "./Icon";
import type { InfoTooltipProps } from "./InfoTooltip";
import { tooltipPosition } from "./infoTooltipPosition";

export type { InfoTooltipProps } from "./InfoTooltip";

/**
 * Port of InfoTooltipComponent.vue on web: hovering the primary "?" icon shows the tooltip; a click pins it
 * open until the next click anywhere. The bubble is fixed-positioned so hovering never fights an overlay.
 */
export function InfoTooltip({
    text,
    children,
    size = 20,
    maxWidth = 300,
    testID,
    nativeID,
    tooltipTestID,
    icon,
    color,
    accessibilityLabel = "More information",
}: InfoTooltipProps) {
    const colors = useThemeColors();
    const { width, height } = useWindowDimensions();
    const [activator, setActivator] = useState<HTMLElement | null>(null);
    const [bubble, setBubble] = useState<{ width: number; height: number }>();
    const [openedFromClick, setOpenedFromClick] = useState(false);
    const [openedFromHover, setOpenedFromHover] = useState(false);
    // the keyboard: focus shows it as hover does (WCAG 1.4.13)
    const [openedFromFocus, setOpenedFromFocus] = useState(false);
    // the pointer may move onto the bubble and read it; the bubble's padding bridges the gap to the trigger
    const [overBubble, setOverBubble] = useState(false);
    const bubbleRef = useRef<HTMLElement | null>(null);
    const bubbleId = useId();
    const open =
        openedFromClick || openedFromHover || openedFromFocus || overBubble;
    const closeAll = () => {
        setOpenedFromClick(false);
        setOpenedFromHover(false);
        setOpenedFromFocus(false);
        setOverBubble(false);
    };
    // Escape dismisses it without moving the focus (WCAG 1.4.13)
    useEscapeKey(open, closeAll);

    // a click anywhere closes a click-opened tooltip (the next tick, so the opening click does not)
    useEffect(() => {
        if (!openedFromClick) return;
        const close = () => setOpenedFromClick(false);
        const t = setTimeout(() =>
            document.addEventListener("click", close, { once: true })
        );
        return () => {
            clearTimeout(t);
            document.removeEventListener("click", close);
        };
    }, [openedFromClick]);

    const rect = open ? activator?.getBoundingClientRect() : undefined;
    const position =
        rect && bubble
            ? tooltipPosition(
                  {
                      x: rect.left,
                      y: rect.top,
                      width: rect.width,
                      height: rect.height,
                  },
                  bubble,
                  width,
                  height
              )
            : undefined;

    return (
        <>
            <Pressable
                ref={(node) =>
                    setActivator(node as unknown as HTMLElement | null)
                }
                testID={testID}
                nativeID={nativeID}
                accessibilityRole="button"
                accessibilityLabel={accessibilityLabel}
                aria-expanded={open}
                {...(open ? ({ "aria-describedby": bubbleId } as object) : {})}
                onPress={() => {
                    setOpenedFromClick((v) => !v);
                    setOpenedFromHover(false);
                }}
                onFocus={() => setOpenedFromFocus(true)}
                onBlur={() => setOpenedFromFocus(false)}
                onHoverIn={() => setOpenedFromHover(true)}
                onHoverOut={(event) => {
                    // leaving towards the bubble keeps it open
                    const to = (event as unknown as MouseEvent).relatedTarget;
                    if (to instanceof Node && bubbleRef.current?.contains(to))
                        setOverBubble(true);
                    setOpenedFromHover(false);
                }}
                className="cursor-pointer"
            >
                <Icon
                    name={icon ?? "circle-question"}
                    solid={icon !== undefined}
                    size={size}
                    color={color ?? colors.primary}
                />
            </Pressable>
            {open ? (
                // the 10px padding covers the gap to the trigger, so the pointer can cross it
                <View
                    ref={(node) => {
                        bubbleRef.current =
                            node as unknown as HTMLElement | null;
                    }}
                    style={
                        {
                            position: "fixed",
                            zIndex: 2000,
                            padding: 10,
                            left: (position?.left ?? 0) - 10,
                            top: (position?.top ?? 0) - 10,
                            opacity: position ? 1 : 0,
                        } as object
                    }
                    {...{
                        onMouseEnter: () => setOverBubble(true),
                        onMouseLeave: () => setOverBubble(false),
                    }}
                >
                    <View
                        testID={tooltipTestID}
                        nativeID={bubbleId}
                        role="tooltip"
                        onLayout={(e) =>
                            setBubble({
                                width: e.nativeEvent.layout.width,
                                height: e.nativeEvent.layout.height,
                            })
                        }
                        className="bg-tooltip-background rounded px-4 py-[5px]"
                        style={{ maxWidth }}
                    >
                        {text ? (
                            <AppText className="text-tooltip-text text-body-medium leading-[1.6]">
                                {text}
                            </AppText>
                        ) : (
                            children
                        )}
                    </View>
                </View>
            ) : null}
        </>
    );
}
