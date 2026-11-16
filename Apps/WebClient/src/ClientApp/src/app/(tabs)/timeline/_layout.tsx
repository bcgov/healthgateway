import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";

/**
 * Native: the Health Records tab is a one-page stack, so it gets the navigation bar every tab has: the logo, and the account
 * menu (`useNativeStackOptions`). Web: `_layout.web.tsx`.
 */
export default function TimelineStackLayout() {
    const screenOptions = useNativeStackOptions({ roots: ["index"] });
    return <Stack screenOptions={screenOptions} />;
}
