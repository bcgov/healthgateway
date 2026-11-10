import type { ReactNode } from "react";
import { ScrollView, useWindowDimensions } from "react-native";

import { AppModal } from "@/components/common/AppModal";
import { Backdrop } from "@/components/common/Backdrop";

/** The menu's `top-16`, and the gap it keeps from the bottom of the window. */
const MENU_TOP = 64;
const MENU_MARGIN = 16;

interface Props {
    visible: boolean;
    onClose: () => void;
    children: ReactNode;
}

/** The v-menu card under the avatar button; clicking outside closes it. Native: `HeaderMenu.tsx`, a slide-out panel. */
export function HeaderMenu({ visible, onClose, children }: Props) {
    const { height } = useWindowDimensions();
    return (
        <AppModal
            transparent
            visible={visible}
            animationType="fade"
            onRequestClose={onClose}
        >
            <Backdrop className="flex-1" onPress={onClose}>
                <ScrollView
                    // open entries can outgrow a short window
                    style={{ maxHeight: height - MENU_TOP - MENU_MARGIN }}
                    className="absolute right-2 top-16 bg-surface rounded-md shadow-lg border border-border-light min-w-[220px] grow-0"
                    contentContainerClassName="py-2"
                >
                    {children}
                </ScrollView>
            </Backdrop>
        </AppModal>
    );
}
