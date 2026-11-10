import { Stack } from "expo-router";
import {
    CommonActions,
    type NavigationProp,
    type ParamListBase,
} from "expo-router/react-navigation";

import { useThemeColors } from "@/hooks/useTheme";
import { useAuthStore } from "@/stores/auth.store";

const PAGES = "(pages)";
const TABS = "(tabs)";
/** The login sheet (`src/app/loginProviders.tsx`): presented over the page that opened it. */
const LOGIN_SHEET = "loginProviders";

interface RootRoute {
    name: string;
}

/**
 * Whether the root stack keeps the screen beneath the top one. Two cases: the login sheet, which is presented over the
 * page that opened it; and signed out, the tabs group pushed over the signed-out pages (the gear menu's Security, the
 * Support menu's Terms of Service), so the platform's back swipe returns there. Everything else keeps just the top
 * screen, as the Slot this replaced did: signing out must not leave the signed-in tabs mounted under the landing page.
 */
export function keepsScreenBeneath(
    routes: readonly RootRoute[],
    index: number,
    signedOut: boolean
): boolean {
    if (index > 0 && routes[index]?.name === LOGIN_SHEET) return true;
    return (
        signedOut &&
        routes.length === 2 &&
        index === 1 &&
        routes[0]?.name === PAGES &&
        routes[1]?.name === TABS
    );
}

/** On each focus: drops the screens beneath the top one unless `keepsScreenBeneath` says to keep them. */
function pruneRootStack(navigation: NavigationProp<ParamListBase>) {
    const state = navigation.getState();
    if (state.routes.length < 2) return;
    const signedOut = !useAuthStore.getState().isAuthenticated;
    if (keepsScreenBeneath(state.routes, state.index, signedOut)) return;
    const top = state.routes[state.index];
    if (!top) return;
    // same key: the top screen stays mounted; only the ones beneath go
    navigation.dispatch(
        CommonActions.reset({ ...state, index: 0, routes: [top] })
    );
}

/**
 * Native root navigator: a native stack, so a signed-out user who opens a page in the tabs group gets the platform's
 * push and back swipe over the page they came from, as a signed-in user does in the Home tab's stack. Every other
 * change between the signed-out pages and the tabs (sign in, sign out, the route guard) stays instant, with no swipe,
 * as it was with the Slot. Web: `RootNavigator.web.tsx`, where browser history is the back.
 */
export function RootNavigator() {
    const colors = useThemeColors();
    const signedOut = useAuthStore((s) => !s.isAuthenticated);
    return (
        <Stack
            screenOptions={{
                headerShown: false,
                animation: "none",
                gestureEnabled: false,
                contentStyle: { backgroundColor: colors.background },
            }}
            screenListeners={({ navigation }) => ({
                focus: () =>
                    pruneRootStack(navigation as NavigationProp<ParamListBase>),
            })}
        >
            <Stack.Screen
                name={TABS}
                options={{
                    animation: signedOut ? "default" : "none",
                    gestureEnabled: signedOut,
                }}
            />
            <Stack.Screen
                name={LOGIN_SHEET}
                options={{
                    presentation: "formSheet",
                    sheetAllowedDetents: "fitToContents",
                    sheetGrabberVisible: true,
                    // drag down to close, as the close button does
                    gestureEnabled: true,
                    animation: "default",
                    contentStyle: { backgroundColor: colors.surface },
                }}
            />
        </Stack>
    );
}
