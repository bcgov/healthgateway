import { useState } from "react";
import { Platform, Pressable, useWindowDimensions, View } from "react-native";

import { AppModal } from "./AppModal";
import { AppText } from "./AppText";
import { Backdrop } from "./Backdrop";
import { menuKeyDown } from "./menuKeys";
import { webClassRef } from "./webClass";

export interface HgMenuItem {
    title: string;
    testID?: string;
    onPress: () => void;
}

interface Props {
    items: HgMenuItem[];
    /** the activator; call `open` from its onPress, and tell its `aria-expanded` from `isOpen` */
    children: (open: () => void, isOpen: boolean) => React.ReactNode;
    /** fixed menu width; default: at least the activator's width (v-menu) */
    width?: number;
    /** "end": right edges aligned (location="bottom end"); "start" (default): left edges aligned */
    align?: "start" | "end";
    /** test id on the menu list */
    testID?: string;
}

/**
 * Port of Vuetify's v-menu with a v-list of items, as the web client uses it (card ⋮ menu, Download Record
 * formats): opens under its activator in a transparent modal, closes on an outside tap or after an item runs.
 */
export function HgMenu({
    items,
    children,
    width,
    align = "start",
    testID,
}: Props) {
    const [open, setOpen] = useState(false);
    // undefined until the activator has been measured: the menu stays invisible until it can be placed
    const [anchor, setAnchor] = useState<{
        top: number;
        left: number;
        width: number;
    }>();
    // the activator, held in state through a callback ref (render never reads a ref)
    const [activator, setActivator] = useState<React.ComponentRef<
        typeof View
    > | null>(null);
    const { width: windowWidth } = useWindowDimensions();

    const show = () => {
        setAnchor(undefined);
        setOpen(true);
        activator?.measureInWindow((x, y, w, h) => {
            const menuWidth = width ?? w;
            const preferred = align === "end" ? x + w - menuWidth : x;
            setAnchor({
                top: y + h,
                left: Math.max(
                    8,
                    Math.min(preferred, windowWidth - menuWidth - 8)
                ),
                width: menuWidth,
            });
        });
    };

    return (
        <>
            <View ref={setActivator} collapsable={false}>
                {children(show, open)}
            </View>
            <AppModal
                // iOS: a modal supports portrait only unless told otherwise, which would rotate a landscape screen
                supportedOrientations={["portrait", "landscape"]}
                transparent
                visible={open}
                animationType="fade"
                onRequestClose={() => setOpen(false)}
            >
                <Backdrop className="flex-1" onPress={() => setOpen(false)}>
                    <View
                        ref={webClassRef({
                            "v-menu": true,
                            "v-overlay--active": true,
                        })}
                        testID={testID}
                        accessibilityRole="menu"
                        // web: arrow keys move between the items; the Modal's focus trap has already focused the first
                        {...(Platform.OS === "web"
                            ? ({ onKeyDown: menuKeyDown } as object)
                            : {})}
                        className="absolute bg-surface rounded shadow-lg border border-border-light py-1"
                        style={{
                            top: anchor?.top ?? 0,
                            left: anchor?.left ?? 0,
                            minWidth: anchor?.width,
                            opacity: anchor ? 1 : 0,
                        }}
                    >
                        {items.map((item) => (
                            <Pressable
                                ref={webClassRef({ "v-list-item": true })}
                                key={item.title}
                                testID={item.testID}
                                accessibilityRole="menuitem"
                                onPress={() => {
                                    setOpen(false);
                                    item.onPress();
                                }}
                                className="px-4 py-3 hover:bg-surface-hover active:bg-surface-hover"
                            >
                                <AppText>{item.title}</AppText>
                            </Pressable>
                        ))}
                    </View>
                </Backdrop>
            </AppModal>
        </>
    );
}
