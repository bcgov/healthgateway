import { Platform } from "react-native";

/**
 * Web only: a callback ref that sets Vuetify class names on the DOM node (e.g. `v-chip--selected`), so the
 * web client's Cypress selectors find the same elements. No styling comes from these classes.
 */
export function webClassRef(classes: Record<string, boolean>) {
    return (node: unknown) => {
        if (Platform.OS !== "web" || !node) return;
        const list = (node as { classList?: DOMTokenList }).classList;
        if (!list) return;
        for (const [name, on] of Object.entries(classes)) list.toggle(name, on);
    };
}

/**
 * Web only: a callback ref that sets one HTML attribute on the DOM node (e.g. `data-url`, for Cypress), or removes it
 * when `value` is false.
 */
export function webAttributeRef(name: string, value: string | false) {
    return (node: unknown) => {
        if (Platform.OS !== "web" || !node) return;
        const element = node as {
            setAttribute?: (n: string, v: string) => void;
            removeAttribute?: (n: string) => void;
        };
        if (value === false) element.removeAttribute?.(name);
        else element.setAttribute?.(name, value);
    };
}
