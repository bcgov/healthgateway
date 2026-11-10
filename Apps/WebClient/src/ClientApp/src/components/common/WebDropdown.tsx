/** Web only (`WebDropdown.web.tsx`): native menus open in a modal, which is their own window. */
export function WebDropdown(_props: {
    anchor: unknown;
    onClose: () => void;
    /** the list's role: "menu" (default) or "listbox" */
    role?: "menu" | "listbox";
    /** the list's name (a listbox needs one) */
    accessibilityLabel?: string;
    children: React.ReactNode;
}) {
    return null;
}
