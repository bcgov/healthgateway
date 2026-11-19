import type { NativeStackNavigationOptions } from "expo-router/native-stack";

import type { OuterBack } from "@/navigation/stackBack";

const noBack = (_target: OuterBack): NativeStackNavigationOptions => ({});

/**
 * The navigation bar's logo, menu and drawn back button (`outerBack`), platform by platform:
 * `nativeHeaderItems.ios.tsx` (UIMenu) and `nativeHeaderItems.android.tsx` (Compose). Web has no native navigation bar: its header is `Header.web.tsx`.
 */
export function useNativeHeaderItems(_options: { overApp?: boolean } = {}): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
    back: (target: OuterBack) => NativeStackNavigationOptions;
} {
    return { logo: {}, menu: {}, back: noBack };
}
