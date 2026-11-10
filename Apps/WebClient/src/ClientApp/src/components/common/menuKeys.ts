const ITEMS = '[role="menuitem"], [role="menuitemcheckbox"], [role="option"]';

/** The items of a menu or listbox that can take focus, in order. */
export function menuItems(container: HTMLElement): HTMLElement[] {
    return [...container.querySelectorAll<HTMLElement>(ITEMS)].filter(
        (item) => item.getAttribute("aria-disabled") !== "true"
    );
}

/** Put the focus on a menu's first item, as a menu that has just opened should. */
export function focusFirstMenuItem(container: HTMLElement | null) {
    if (container) menuItems(container)[0]?.focus();
}

interface KeyEvent {
    key: string;
    currentTarget: unknown;
    preventDefault: () => void;
}

/**
 * Web keyboard support for a `menu` or `listbox` (WAI-ARIA menu pattern): Down and Up move between the items and
 * wrap, Home and End jump to the ends, Escape closes. Attach to the container's `onKeyDown`; the items must be
 * focusable (react-native-web gives every Pressable a tab index).
 */
export function menuKeyDown(event: KeyEvent, onClose?: () => void) {
    const items = menuItems(event.currentTarget as HTMLElement);
    if (items.length === 0) return;
    const current = items.indexOf(document.activeElement as HTMLElement);
    let next: number;
    switch (event.key) {
        case "ArrowDown":
            next = (current + 1) % items.length;
            break;
        case "ArrowUp":
            next = (current - 1 + items.length) % items.length;
            break;
        case "Home":
            next = 0;
            break;
        case "End":
            next = items.length - 1;
            break;
        case "Escape":
            if (onClose) {
                event.preventDefault();
                onClose();
            }
            return;
        default:
            return;
    }
    event.preventDefault();
    items[next].focus();
}
