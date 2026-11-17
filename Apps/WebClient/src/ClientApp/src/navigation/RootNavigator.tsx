import { Stack } from "expo-router";
import {
    CommonActions,
    type NavigationProp,
    type ParamListBase,
} from "expo-router/react-navigation";

import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { useThemeColors } from "@/hooks/useTheme";

const TABS = "(tabs)";
/** Security and Terms of Service (`src/app/(settings)`): pushed over the page that opened them. */
const SETTINGS = "(settings)";
/** The login sheet (`src/app/loginProviders.tsx`): presented over the page that opened it. */
const LOGIN_SHEET = "loginProviders";
/** An unknown address (`src/app/+not-found.tsx`). */
const NOT_FOUND = "+not-found";

interface RootRoute {
    name: string;
}

/**
 * Whether the root stack keeps the screen beneath the top one. Two cases: the login sheet, which is presented over the
 * page that opened it; and Security or Terms of Service (the account or settings menu's Security, the Support menu's
 * Terms of Service), pushed over the page that opened them, in a tab or outside the tabs, so their back button and the
 * platform's back swipe return there. Everything else keeps just the top screen, as the Slot this replaced did: signing
 * out must not leave the signed-in tabs mounted under the landing page.
 */
export function keepsScreenBeneath(
    routes: readonly RootRoute[],
    index: number
): boolean {
    if (index === 0) return false;
    const top = routes[index]?.name;
    return top === LOGIN_SHEET || top === SETTINGS;
}

/** On each focus: drops the screens beneath the top one unless `keepsScreenBeneath` says to keep them. */
function pruneRootStack(navigation: NavigationProp<ParamListBase>) {
    const state = navigation.getState();
    if (state.routes.length < 2) return;
    if (keepsScreenBeneath(state.routes, state.index)) return;
    const top = state.routes[state.index];
    if (!top) return;
    // same key: the top screen stays mounted; only the ones beneath go
    navigation.dispatch(
        CommonActions.reset({ ...state, index: 0, routes: [top] })
    );
}

/**
 * Native root navigator: a native stack, so Security and Terms of Service get the platform's push and back swipe over
 * the page they were opened from, signed in or out, in a tab or outside the tabs. Every other change between the
 * signed-out pages and the tabs (sign in, sign out, the route guard) stays instant, with no swipe, as it was with the
 * Slot. Web: `RootNavigator.web.tsx`, where browser history is the back.
 */
export function RootNavigator() {
    const colors = useThemeColors();
    // the groups draw their own navigation bars; Page Not Found is a page of its own here
    const notFoundOptions = useNativeStackOptions();
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
            <Stack.Screen name={TABS} />
            <Stack.Screen
                name={SETTINGS}
                options={{ animation: "default", gestureEnabled: true }}
            />
            <Stack.Screen name={NOT_FOUND} options={notFoundOptions} />
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
