import { Group, Host, Popover, RNHostView } from "@expo/ui/swift-ui";
import { presentationBackground } from "@expo/ui/swift-ui/modifiers";
import { useState } from "react";
import { Pressable, View } from "react-native";

import { useThemeColors } from "@/hooks/useTheme";

import {
    InfoTooltipContent,
    InfoTooltipIcon,
    type InfoTooltipProps,
} from "./infoTooltip.shared";

export type { InfoTooltipProps } from "./infoTooltip.shared";

/**
 * iOS: a tap on the "?" icon presents a SwiftUI popover beside it, and a tap anywhere dismisses it.
 * Android: `InfoTooltip.android.tsx`; web: `InfoTooltip.tsx`.
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
    const [open, setOpen] = useState(false);

    return (
        <Host matchContents>
            <Popover isPresented={open} onIsPresentedChange={setOpen}>
                <Popover.Trigger>
                    <RNHostView matchContents>
                        <Pressable
                            testID={testID}
                            nativeID={nativeID}
                            accessibilityRole="button"
                            accessibilityLabel={accessibilityLabel}
                            accessibilityState={{ expanded: open }}
                            onPress={() => setOpen(true)}
                        >
                            <InfoTooltipIcon
                                icon={icon}
                                size={size}
                                color={color}
                            />
                        </Pressable>
                    </RNHostView>
                </Popover.Trigger>
                <Popover.Content>
                    <Group
                        // the content's text uses the tooltip colours, so the popover and its arrow take the tooltip background
                        modifiers={[
                            presentationBackground(
                                colors["tooltip-background"]
                            ),
                        ]}
                    >
                        <RNHostView matchContents>
                            <View
                                testID={tooltipTestID}
                                className="px-4 py-[5px]"
                                style={{ maxWidth }}
                            >
                                <InfoTooltipContent text={text}>
                                    {children}
                                </InfoTooltipContent>
                            </View>
                        </RNHostView>
                    </Group>
                </Popover.Content>
            </Popover>
        </Host>
    );
}
