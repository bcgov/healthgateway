import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { calculateUserState, UserState } from "@/navigation/userState";
import { useAuthStore } from "@/stores/auth.store";
import { useUserStore } from "@/stores/user.store";

/** Deep links into Profile or Other record sources get Home beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "home" };

/** The pages the account menu itself opens: Back is the way out of them, so the menu is left off. */
const MENU_PAGES = ["profile", "security"];

/**
 * Whether Back leads somewhere from Profile and Security: registered (Home is beneath them), or signed out (the page
 * that opened Security). Otherwise the route guard holds the user on Profile (an account pending deletion, Terms of
 * Service to accept) with no tab bar and no Home to go back to, and the account menu's Log Out is the only way out.
 */
function useBackLeadsSomewhere(): boolean {
    // subscribe so the decision follows sign-in and the user's record
    useAuthStore((s) => s.isAuthenticated);
    useUserStore((s) => s.user);
    useUserStore((s) => s.patientRetrievalFailed);
    const state = calculateUserState();
    return (
        state === UserState.registered || state === UserState.unauthenticated
    );
}

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
        withoutMenu: useBackLeadsSomewhere() ? MENU_PAGES : [],
    });
    return (
        <Stack screenOptions={screenOptions}>
            <Stack.Screen name="security" dangerouslySingular />
        </Stack>
    );
}
