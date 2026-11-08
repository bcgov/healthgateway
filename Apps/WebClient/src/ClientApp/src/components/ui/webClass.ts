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

/** Web only: a callback ref that sets one HTML attribute on the DOM node (e.g. `data-url`, for Cypress). */
export function webAttributeRef(name: string, value: string) {
    return (node: unknown) => {
        if (Platform.OS !== "web" || !node) return;
        (
            node as { setAttribute?: (n: string, v: string) => void }
        ).setAttribute?.(name, value);
    };
}
