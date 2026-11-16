import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";

/**
 * Native: the Services tab is a one-page stack, so it gets the navigation bar every tab has: the logo, and the account
 * menu (`useNativeStackOptions`). Web: `_layout.web.tsx`.
 */
export default function ServicesStackLayout() {
    const screenOptions = useNativeStackOptions({ roots: ["index"] });
    return <Stack screenOptions={screenOptions} />;
}
