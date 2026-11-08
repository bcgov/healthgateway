import { Stack } from "expo-router";

import { LazyTab } from "@/components/site/LazyTab";

/** Deep links into a dependent's timeline get the list beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "index" };

/**
 * Native: the Dependents tab is a stack so a dependent's timeline keeps the tab bar and gets the platform back
 * gesture. The shell's own header stays; the stack draws none. Web: `_layout.web.tsx`.
 */
export default function DependentsStackLayout() {
    return (
        <LazyTab>
            <Stack screenOptions={{ headerShown: false }} />
        </LazyTab>
    );
}
