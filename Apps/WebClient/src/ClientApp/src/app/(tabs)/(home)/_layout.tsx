import { Stack } from "expo-router";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { homeStackTitles } from "@/navigation/stackBack";
import { calculateUserState, UserState } from "@/navigation/userState";
import { useAuthStore } from "@/stores/auth.store";
import { useUserStore } from "@/stores/user.store";

/** Deep links into Profile or Other record sources get Home beneath them, so back leads somewhere. */
export const unstable_settings = { initialRouteName: "home" };

/** The pages the account menu itself opens: Back is the way out of them, so the menu is left off. */
const MENU_PAGES = ["profile"];

/**
 * Whether Back leads somewhere from Profile: registered (Home is beneath it). Otherwise the route guard holds the user on
 * Profile (an account pending deletion, Terms of Service to accept) with no tab bar and no Home to go back to, and the
 * account menu's Log Out is the only way out.
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

/**
 * Native: the Home tab is a stack so pages reached from it (Profile, Other record sources) keep the tab bar
 * and get the platform back gesture, as the web client keeps its drawer on them. Home shows the logo in the
 * navigation bar; the pages above it the back button and their title (`useNativeStackOptions`). Web: `_layout.web.tsx`.
 * Security and Terms of Service are not here: they go over whichever tab opened them (`app/(settings)`).
 */
export default function HomeStackLayout() {
    const screenOptions = useNativeStackOptions({
        roots: ["home"],
        titles: homeStackTitles,
        backTitle: "Home",
        withoutMenu: useBackLeadsSomewhere() ? MENU_PAGES : [],
    });
    return <Stack screenOptions={screenOptions} />;
}
