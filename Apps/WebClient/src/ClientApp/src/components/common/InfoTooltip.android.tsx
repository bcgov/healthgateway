import {
    Host,
    RNHostView,
    TooltipBox,
    type TooltipBoxRef,
} from "@expo/ui/jetpack-compose";
import { useRef } from "react";
import { Pressable, useWindowDimensions, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";

import {
    InfoTooltipContent,
    InfoTooltipIcon,
    type InfoTooltipProps,
} from "./infoTooltip.shared";

export type { InfoTooltipProps } from "./infoTooltip.shared";

// Material's rich tooltip is at most 320dp wide, with 16dp of padding on each side
const RICH_TOOLTIP_PADDING = 16;
const RICH_TOOLTIP_CONTENT_WIDTH = 320 - 2 * RICH_TOOLTIP_PADDING;

/**
 * Android: a tap on the "?" icon shows a Material rich tooltip above it (below when there is no room above), and a tap
 * anywhere dismisses it.
 * iOS: `InfoTooltip.ios.tsx`; web: `InfoTooltip.tsx`.
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
    const colors = useThemeColors();
    const tooltip = useRef<TooltipBoxRef>(null);
    const { width: windowWidth } = useWindowDimensions();
    // Material aligns the tooltip with the icon's left edge, else its right edge, and otherwise centres it on the icon,
    // where it can run off the screen. One side always has at least half the window, so a tooltip that fits in half
    // always takes one of the edges.
    const contentWidth = Math.min(
        maxWidth,
        RICH_TOOLTIP_CONTENT_WIDTH,
        windowWidth / 2 - 2 * RICH_TOOLTIP_PADDING
    );

    return (
        <Host matchContents>
            <TooltipBox
                ref={tooltip}
                isPersistent
                // only the icon's press opens it, not a long press
                enableUserInput={false}
                // the tooltip takes the tap that dismisses it, so a tap on the icon closes it rather than reopening it
                focusable
            >
                <RNHostView matchContents>
                    <Pressable
                        testID={testID}
                        nativeID={nativeID}
                        accessibilityRole="button"
                        accessibilityLabel={accessibilityLabel}
                        onPress={() => void tooltip.current?.show()}
                    >
                        <InfoTooltipIcon
                            icon={icon}
                            size={size}
                            color={color}
                        />
                    </Pressable>
                </RNHostView>
                <TooltipBox.RichTooltip
                    containerColor={colors["tooltip-background"]}
                    contentColor={colors["tooltip-text"]}
                >
                    <TooltipBox.RichTooltip.Text>
                        <RNHostView matchContents>
                            <View
                                testID={tooltipTestID}
                                className="py-[5px]"
                                style={{ maxWidth: contentWidth }}
                            >
                                <InfoTooltipContent text={text}>
                                    {children}
                                </InfoTooltipContent>
                            </View>
                        </RNHostView>
                    </TooltipBox.RichTooltip.Text>
                </TooltipBox.RichTooltip>
            </TooltipBox>
        </Host>
    );
}
