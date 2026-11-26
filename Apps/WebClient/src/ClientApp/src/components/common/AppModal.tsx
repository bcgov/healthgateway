import { useLayoutEffect } from "react";
import { Modal, type ModalProps } from "react-native";

import { useMotion } from "@/hooks/useMotion";
import { useNativeWindowsHidden } from "@/hooks/useNativeWindowsHidden";

import { closeActiveInfoTooltip } from "./activeInfoTooltip";

interface Props extends ModalProps {
    /** Shown while locked too: a modal opened from the lock screen itself, holding nothing private (its gear menu). */
    overLock?: boolean;
}

/**
 * Every modal in the app. A native Modal is its own window, so it would sit above the lock screen: it hides while the
 * app is locked and comes back, with its state, once unlocked (unless `overLock`). It also hides while an update is
 * required, so nothing covers the Update required dialog, which is drawn over the page (`Dialog` `inline`): iOS shows
 * one modal window at a time. Its fade or slide is dropped while the system asks for reduced motion. On the web, it
 * closes the open tooltip as it opens, as the tooltip's popover would cover it; a tooltip inside it opens after it.
 */
export function AppModal({
    visible,
    animationType,
    overLock = false,
    ...rest
}: Props) {
    const hidden = useNativeWindowsHidden();
    const motion = useMotion();
    // a lock screen's own modal shows over it, the lock screen being above the Update required dialog
    const shown = visible !== false && (overLock || !hidden);
    // before painting, so the tooltip does not flash over it
    useLayoutEffect(() => {
        if (shown) closeActiveInfoTooltip();
    }, [shown]);
    return (
        <Modal
            {...rest}
            animationType={motion.modal(animationType)}
            visible={shown}
        />
    );
}
