import type { NativeStackNavigationOptions } from "expo-router/native-stack";

/**
 * The navigation bar's logo and menu, platform by platform: `nativeHeaderItems.ios.tsx` (UIMenu) and
 * `nativeHeaderItems.android.tsx` (Compose). Web has no native navigation bar: its header is `Header.tsx`.
 */
export function useNativeHeaderItems(): {
    logo: NativeStackNavigationOptions;
    menu: NativeStackNavigationOptions;
} {
    return { logo: {}, menu: {} };
}
