import { Stack } from "expo-router";

/** Deep links into Profile or Other record sources get Home beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "home" };

/**
 * Native: the Home tab is a stack so pages reached from it (Profile, Other record sources) keep the tab bar
 * and get the platform back gesture, as the web client keeps its drawer on them. The shell's own header stays;
 * the stack draws none. Web: `_layout.web.tsx`.
 *
 * Security is singular: only one is ever in the stack. Opening it from the gear menu while it is open does nothing,
 * and while it is beneath another page it moves up to the top instead of stacking a second copy.
 */
export default function HomeStackLayout() {
    return (
        <Stack screenOptions={{ headerShown: false }}>
            <Stack.Screen name="security" dangerouslySingular />
        </Stack>
    );
}
