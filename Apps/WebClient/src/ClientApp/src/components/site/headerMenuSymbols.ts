import type { ButtonProps } from "@expo/ui/swift-ui";

import type { HeaderMenuGlyph } from "@/navigation/headerMenu";

type SFSymbol = NonNullable<ButtonProps["systemImage"]>;

/** iOS: each header menu glyph's SF Symbol, for the navigation bar's UIMenu and `NativeSettingsMenu`'s SwiftUI menu. */
export const sfSymbols: Record<HeaderMenuGlyph, SFSymbol> = {
    bell: "bell",
    darkMode: "moon",
    document: "doc.text",
    email: "envelope",
    external: "arrow.up.right.square",
    feedback: "bubble.left",
    logout: "rectangle.portrait.and.arrow.right",
    profile: "person",
    refresh: "arrow.clockwise",
    security: "lock.shield",
    settings: "gearshape",
    support: "questionmark.circle",
};
