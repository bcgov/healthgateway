import { MaybeRefOrGetter, onBeforeUnmount, toValue, watch } from "vue";

// Active Vuetify overlays (dialogs, menus) handle Escape themselves; tooltips are ignored.
const ActiveOverlaySelector = ".v-overlay--active:not(.v-tooltip)";

/**
 * Invokes the callback when the Escape key is pressed while the component is open.
 * Intended for elements that Vuetify does not close on Escape, such as temporary navigation drawers.
 * Escape is ignored while a Vuetify dialog or menu is open so only the top-most layer closes.
 * @param isOpen Whether the component is currently open.
 * @param onEscape Callback that closes the component.
 */
export function useEscapeKey(
    isOpen: MaybeRefOrGetter<boolean>,
    onEscape: () => void
): void {
    function onKeydown(event: KeyboardEvent): void {
        if (
            event.key !== "Escape" ||
            document.querySelector(ActiveOverlaySelector) !== null
        ) {
            return;
        }
        onEscape();
    }

    function removeListener(): void {
        globalThis.removeEventListener("keydown", onKeydown);
    }

    watch(
        () => toValue(isOpen),
        (open: boolean) => {
            if (open) {
                globalThis.addEventListener("keydown", onKeydown);
            } else {
                removeListener();
            }
        },
        { immediate: true }
    );

    onBeforeUnmount(removeListener);
}
