import { useState } from "react";
import { Pressable, useWindowDimensions, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";

import { AppModal } from "./AppModal";
import { AppText } from "./AppText";
import { Backdrop } from "./Backdrop";
import { Icon } from "./Icon";
import { tooltipPosition } from "./infoTooltipPosition";

export interface InfoTooltipProps {
    text?: string;
    children?: React.ReactNode;
    /** icon size in px (v-icon small 1.25em ≈ 20, x-small 1em ≈ 14-16) */
    size?: number;
    maxWidth?: number;
    testID?: string;
    nativeID?: string;
    tooltipTestID?: string;
    /** the activator icon (default: the regular circle-question) */
    icon?: string;
    /** default: primary */
    color?: string;
    /** the trigger's name: say what it explains, e.g. "More information about the test status" */
    accessibilityLabel?: string;
}

/**
 * Port of InfoTooltipComponent.vue on native: a primary "?" icon; a tap opens the tooltip bubble beside it,
 * a tap anywhere closes it.
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
    const [activator, setActivator] = useState<React.ComponentRef<
        typeof View
    > | null>(null);
    const [anchor, setAnchor] = useState<{
        x: number;
        y: number;
        width: number;
        height: number;
    }>();
    const [bubble, setBubble] = useState<{ width: number; height: number }>();
    const [open, setOpen] = useState(false);

    const show = () => {
        setOpen(true);
        activator?.measureInWindow((x, y, w, h) =>
            setAnchor({ x, y, width: w, height: h })
        );
    };
    const position =
        anchor && bubble
            ? tooltipPosition(anchor, bubble, width, height)
            : undefined;

    return (
        <>
            <View ref={setActivator} collapsable={false}>
                <Pressable
                    testID={testID}
                    nativeID={nativeID}
                    accessibilityRole="button"
                    accessibilityLabel={accessibilityLabel}
                    hitSlop={8}
                    onPress={() => (open ? setOpen(false) : show())}
                >
                    <Icon
                        name={icon ?? "circle-question"}
                        solid={icon !== undefined}
                        size={size}
                        color={color ?? colors.primary}
                    />
                </Pressable>
            </View>
            <AppModal
                supportedOrientations={["portrait", "landscape"]}
                transparent
                visible={open}
                animationType="fade"
                onRequestClose={() => setOpen(false)}
            >
                <Backdrop className="flex-1" onPress={() => setOpen(false)}>
                    <View
                        testID={tooltipTestID}
                        onLayout={(e) =>
                            setBubble({
                                width: e.nativeEvent.layout.width,
                                height: e.nativeEvent.layout.height,
                            })
                        }
                        className="absolute bg-tooltip-background rounded px-4 py-[5px]"
                        style={{
                            maxWidth,
                            left: position?.left ?? 0,
                            top: position?.top ?? 0,
                            opacity: position ? 1 : 0,
                        }}
                    >
                        {text ? (
                            <AppText className="text-tooltip-text text-body-medium leading-[1.6]">
                                {text}
                            </AppText>
                        ) : (
                            children
                        )}
                    </View>
                </Backdrop>
            </AppModal>
        </>
    );
}
