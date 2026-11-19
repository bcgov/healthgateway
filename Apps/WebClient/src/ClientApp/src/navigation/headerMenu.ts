/** The glyphs a header menu entry can show; each platform maps them to its own (SF Symbols on iOS, vectors on Android). */
export type HeaderMenuGlyph =
    | "bell"
    | "darkMode"
    | "document"
    | "email"
    | "external"
    | "feedback"
    | "logout"
    | "profile"
    | "refresh"
    | "security"
    | "settings"
    | "support";

export interface HeaderMenuAction {
    type: "action";
    /** Stable identity: React keys, and the test hook where the platform takes one (Android); the panels' testIDs. */
    key: string;
    label: string;
    glyph?: HeaderMenuGlyph;
    /** Second line under the label (iOS only: Android's menu items have none). */
    subtitle?: string;
    /** Shown as the selected choice (a checkmark). */
    checked?: boolean;
    destructive?: boolean;
    /** Shown, greyed, and not selectable: the version line. */
    disabled?: boolean;
    onPress?: () => void;
}

export interface HeaderMenuSubmenu {
    type: "submenu";
    key: string;
    label: string;
    glyph?: HeaderMenuGlyph;
    items: HeaderMenuAction[];
}

export type HeaderMenuEntry = HeaderMenuAction | HeaderMenuSubmenu;

/**
 * Native header menu, drawn by each platform's own menu (UIMenu on iOS, a Material dropdown on Android): signed in, the
 * account menu under the user's initials; signed out, the settings menu under a gear. Sections are separated by
 * dividers. Built by `useHeaderMenu` (the screens over the app: `useSettingsMenu`); drawn in the navigation bar by
 * `nativeHeaderItems.ios.tsx` and `nativeHeaderItems.android.tsx`.
 */
export interface HeaderMenu {
    kind: "account" | "settings";
    /** The button's accessible name. */
    buttonLabel: string;
    /** Account: the initials the button shows. */
    initials?: string;
    /** Shown at the top of the menu: the user's name. */
    title?: string;
    sections: HeaderMenuEntry[][];
}

/** The notification centre's bell beside the menu, when the notification centre is on. */
export interface HeaderBell {
    /** New notifications not yet seen, shown as a badge. */
    count: number;
    accessibilityLabel: string;
    onPress: () => void;
}

/** The page's pull to refresh as a button, left of the menu, on a page that has one. */
export interface HeaderRefresh {
    /** A refresh is running: the button waits. */
    disabled: boolean;
    onPress: () => void;
}

/** Sections without entries are dropped, so no divider is drawn around nothing. */
export function nonEmptySections(menu: HeaderMenu): HeaderMenuEntry[][] {
    return menu.sections.filter((section) => section.length > 0);
}
