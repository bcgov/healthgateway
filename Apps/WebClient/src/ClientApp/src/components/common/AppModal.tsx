import { Modal, type ModalProps } from "react-native";

import { useMotion } from "@/hooks/useMotion";
import { useAppLockStore } from "@/stores/appLock.store";

interface Props extends ModalProps {
    /** Shown while locked too: a modal opened from the lock screen itself, holding nothing private (its gear menu). */
    overLock?: boolean;
}

/**
 * Every modal in the app. A native Modal is its own window, so it would sit above the lock screen: it hides while the
 * app is locked and comes back, with its state, once unlocked (unless `overLock`). Its fade or slide is dropped while
 * the system asks for reduced motion.
 */
export function AppModal({
    visible,
    animationType,
    overLock = false,
    ...rest
}: Props) {
    const locked = useAppLockStore((s) => s.locked);
    const motion = useMotion();
    return (
        <Modal
            {...rest}
            animationType={motion.modal(animationType)}
            visible={visible !== false && (overLock || !locked)}
        />
    );
}
