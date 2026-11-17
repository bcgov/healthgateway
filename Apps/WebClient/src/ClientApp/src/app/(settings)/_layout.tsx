import { Stack, useRouter } from "expo-router";
import {
    useNavigation,
    useNavigationState,
    useRoute,
} from "expo-router/react-navigation";
import { useMemo } from "react";

import { SettingsPage, WayBackContext } from "@/components/site/SettingsPage";
import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { Path } from "@/constants/path";
import { type OuterBack, shownTitle } from "@/navigation/stackBack";
import { useAuthStore } from "@/stores/auth.store";

/** The pages this stack holds, by route name. */
const settingsTitles: Readonly<Record<string, string>> = {
    security: "Security",
    termsOfService: "Terms of Service",
};

/** The page the account menu itself opens: Back is the way out of it, so the menu is left off. */
const MENU_PAGES = ["security"];

/**
 * Native: Security and Terms of Service go over whatever page opened them, in any tab or outside the tabs. The root
 * stack keeps that page beneath (`RootNavigator`), and the back button returns there, named for it (`shownTitle`).
 * Opened with nothing beneath (a link that starts the app there), the way back is to the start instead: Home signed in,
 * Log In signed out.
 *
 * Security is singular: only one is ever in the stack. Opening it from Terms of Service's menu while it is beneath moves
 * it up to the top instead of stacking a second copy. Web: `_layout.web.tsx`.
 */
export default function SettingsStackLayout() {
    const router = useRouter();
    const rootNavigation = useNavigation();
    const groupRoute = useRoute();
    const signedOut = useAuthStore((s) => !s.isAuthenticated);
    // the page under this stack in the root stack, by what Back calls it; none when nothing is beneath
    const beneathTitle = useNavigationState((state) => {
        const index = state.routes.findIndex((r) => r.key === groupRoute.key);
        const beneath = index > 0 ? state.routes[index - 1] : undefined;
        return beneath ? shownTitle(beneath) : undefined;
    });
    const wayBack = useMemo<OuterBack>(
        () =>
            beneathTitle !== undefined
                ? { label: beneathTitle, goBack: () => rootNavigation.goBack() }
                : {
                      label: signedOut ? "Log In" : "Home",
                      goBack: () =>
                          router.replace(signedOut ? Path.Root : Path.Home),
                  },
        [beneathTitle, rootNavigation, router, signedOut]
    );
    const screenOptions = useNativeStackOptions({
        roots: [],
        titles: settingsTitles,
        withoutMenu: MENU_PAGES,
    });

    return (
        <WayBackContext.Provider value={wayBack}>
            <Stack
                screenOptions={screenOptions}
                screenLayout={({ children }) => (
                    <SettingsPage titles={settingsTitles}>
                        {children}
                    </SettingsPage>
                )}
            >
                <Stack.Screen name="security" dangerouslySingular />
            </Stack>
        </WayBackContext.Provider>
    );
}
