import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { useTimelineStore } from "@/stores/timeline.store";

/** An entry page opened by a link (or the app restarting on it) gets the timeline beneath it, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "index" };

/**
 * Native: the Health Records tab is a stack, so it gets the navigation bar every tab has: the logo, and the account menu
 * (`useNativeStackOptions`). An entry opens over it on a page of its own (`entry`, titled with its record type): the
 * platform's back button, and no menu or Refresh. It comes and goes without the animation when the window folds or
 * unfolds. Web: `_layout.web.tsx`.
 */
export default function TimelineStackLayout() {
    const screenOptions = useNativeStackOptions({
        roots: ["index"],
        backTitle: "Health Records",
        withoutMenu: ["entry"],
    });
    const instant = useTimelineStore((s) => s.entryPageInstant);
    return (
        <Stack screenOptions={screenOptions}>
            <Stack.Screen
                name="entry"
                options={{ animation: instant ? "none" : "default" }}
            />
        </Stack>
    );
}
