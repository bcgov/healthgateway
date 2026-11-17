import { Stack } from "expo-router";

import { LazyTab } from "@/components/site/LazyTab";
import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { dependentsStackTitles } from "@/navigation/stackBack";

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
    });
    return (
        <LazyTab>
            <Stack screenOptions={screenOptions} />
        </LazyTab>
    );
}
