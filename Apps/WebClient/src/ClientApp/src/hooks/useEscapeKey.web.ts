import { useEffect, useEffectEvent } from "react";

// An open react-native-web Modal (dialogs, menus, the notification centre) closes itself on Escape through
// onRequestClose and marks its content aria-modal; while one is open, Escape belongs to it.
const OpenModalSelector = '[aria-modal="true"]';

/**
 * Port of utility/useEscapeKey.ts: runs `onEscape` when Escape is pressed while `isOpen`, for elements that are not a
 * Modal and so do not close on Escape themselves, such as the temporary navigation drawer. Escape is ignored while a
 * modal is open, so only the top-most layer closes.
 */
export function useEscapeKey(isOpen: boolean, onEscape: () => void): void {
    const escape = useEffectEvent(onEscape);
    useEffect(() => {
        if (!isOpen) return;
        function onKeydown(event: KeyboardEvent): void {
            if (
                event.key !== "Escape" ||
                document.querySelector(OpenModalSelector) !== null
            )
                return;
            escape();
        }
        globalThis.addEventListener("keydown", onKeydown);
        return () => globalThis.removeEventListener("keydown", onKeydown);
    }, [isOpen]);
}
