import { useState } from "react";
import { Pressable, useWindowDimensions, View } from "react-native";

import { useBreakpoint } from "@/hooks/useBreakpoint";
import { useThemeColors } from "@/hooks/useTheme";

import { AppModal } from "./AppModal";
import { AppText } from "./AppText";
import { Backdrop } from "./Backdrop";
import { Icon } from "./Icon";

interface Props {
    buttonText: string;
    buttonTestID?: string;
    popoverTestID?: string;
    children: React.ReactNode;
}

/** Port of InfoPopoverComponent.vue: a transparent primary button that opens a card beside it (250 / 472 wide). */
export function InfoPopover({
    buttonText,
    buttonTestID,
    popoverTestID,
    children,
}: Props) {
    const colors = useThemeColors();
    const { isMobile } = useBreakpoint();
    const { width: windowWidth } = useWindowDimensions();
    const cardWidth = isMobile ? 250 : 472;
    const [open, setOpen] = useState(false);
    const [anchor, setAnchor] = useState<{ top: number; left: number }>();
    const [activator, setActivator] = useState<React.ComponentRef<
        typeof View
    > | null>(null);

    const show = () => {
        setAnchor(undefined);
        setOpen(true);
        activator?.measureInWindow((x, y, _w, h) =>
            setAnchor({
                top: y + h,
                left: Math.max(8, Math.min(x, windowWidth - cardWidth - 8)),
            })
        );
    };

    return (
        <>
            <View ref={setActivator} collapsable={false} className="self-start">
                <Pressable
                    testID={buttonTestID}
                    accessibilityRole="button"
                    accessibilityLabel={buttonText}
                    onPress={show}
                    className="flex-row items-center min-h-[28px] px-2 mt-2 rounded active:bg-surface-hover"
                >
                    <Icon
                        name="circle-info"
                        size={14}
                        color={colors.primary}
                        style={{ marginRight: 6 }}
                    />
                    <AppText className="font-sans text-[12px] leading-5 tracking-[1.25px] text-primary">
                        {buttonText}
                    </AppText>
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
                        testID={popoverTestID}
                        className="absolute bg-surface rounded shadow-lg border border-border-light p-2"
                        style={{
                            top: anchor?.top ?? 0,
                            left: anchor?.left ?? 0,
                            width: cardWidth,
                            opacity: anchor ? 1 : 0,
                        }}
                    >
                        {children}
                    </View>
                </Backdrop>
            </AppModal>
        </>
    );
}
