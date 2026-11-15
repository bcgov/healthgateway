import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";

/** Deep links into Profile or Other record sources get Home beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "home" };

const titles = {
    otherRecordSources: "Other record sources",
    profile: "Profile",
    security: "Security",
    termsOfService: "Terms of Service",
};

/**
 * Native: the Home tab is a stack so pages reached from it (Profile, Other record sources) keep the tab bar
 * and get the platform back gesture, as the web client keeps its drawer on them. Home shows the logo in the
 * navigation bar; the pages above it the back button and their title (`useNativeStackOptions`). Web: `_layout.web.tsx`.
 *
 * Security is singular: only one is ever in the stack. Opening it from the account menu while it is open does nothing,
 * and while it is beneath another page it moves up to the top instead of stacking a second copy.
 */
export default function HomeStackLayout() {
    const screenOptions = useNativeStackOptions({
        roots: ["home"],
        titles,
        backTitle: "Home",
        // the account menu would only open these again: back is the way out
        withoutMenu: ["profile", "security"],
    });
    return (
        <Stack screenOptions={screenOptions}>
            <Stack.Screen name="security" dangerouslySingular />
        </Stack>
    );
}
