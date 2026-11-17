import { Stack } from "expo-router";

import { LazyTab } from "@/components/site/LazyTab";
import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { dependentsStackTitles } from "@/navigation/stackBack";
import { useTimelineStore } from "@/stores/timeline.store";

/** Deep links into a dependent's timeline get the list beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "index" };

/**
 * Native: the Dependents tab is a stack so a dependent's timeline keeps the tab bar and gets the platform back
 * gesture. The list shows the logo in the navigation bar; a dependent's timeline the back button and its title
 * (`useNativeStackOptions`). Web: `_layout.web.tsx`.
 */
export default function DependentsStackLayout() {
    const screenOptions = useNativeStackOptions({
        roots: ["index"],
        titles: dependentsStackTitles,
        backTitle: "Dependents",
        withoutMenu: ["[id]/entry"],
    });
    // a dependent's entry comes and goes without the animation when the window folds or unfolds
    const instant = useTimelineStore((s) => s.entryPageInstant);
    return (
        <LazyTab>
            <Stack screenOptions={screenOptions}>
                <Stack.Screen
                    name="[id]/entry"
                    options={{ animation: instant ? "none" : "default" }}
                />
            </Stack>
        </LazyTab>
    );
}
