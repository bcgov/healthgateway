import type { NativeStackNavigationOptions } from "expo-router/native-stack";

import { useThemeColors } from "@/hooks/useTheme";
import {
    outerBack,
    routeBeneath,
    type StackNavigation,
} from "@/navigation/stackBack";
import { fonts } from "@/theme";

import { useNativeHeaderItems } from "./nativeHeaderItems";

interface Options {
    /**
     * The routes a stack starts on: the logo in place of a back button, and no title. Omitted: every route is one (the
     * pages outside the tabs, which replace one another).
     */
    roots?: readonly string[];
    /** The titles of the pages pushed on the stack, by route name. */
    titles?: Readonly<Record<string, string>>;
    /** The back button's label to a page the stack starts on (they have no title): the stack's name. */
    backTitle?: string;
    /** Routes shown with no navigation bar at all. */
    hidden?: readonly string[];
    /**
     * Routes whose bar has only the back button and title: no account menu (nor Refresh), which would offer the page
     * itself again (Profile, Security).
     */
    withoutMenu?: readonly string[];
    /**
     * The bar of a screen that covers the app (`OverAppScreen`: the intro, the lock screen, Update required): its logo
     * and settings menu leave out the app's pages, which are behind it (`useNativeHeaderItems`).
     */
    overApp?: boolean;
}

/**
 * Native: the navigation bar every stack shows (stated deviation: the web client's app bar is one header over every
 * page). Under the shell's environment and session banners: the logo on a stack's first page, otherwise the back button
 * and the page's title; on the right, the page's Refresh (when it has pull to refresh) and the account menu (signed in)
 * or the settings menu (signed out), except on the pages that menu itself opens. It stays put while the page scrolls.
 * The back button is named for the page it returns to; a pushed page first in its own stack (signed out, Security or
 * Terms of Service over the page that opened them) gets one drawn by the bar (`outerBack`). Returns `screenOptions`
 * for the stack.
 */
export function useNativeStackOptions({
    roots,
    titles = {},
    backTitle,
    hidden = [],
    withoutMenu = [],
    overApp = false,
}: Options = {}) {
    const colors = useThemeColors();
    const { logo, menu, back } = useNativeHeaderItems({ overApp });
    const base: NativeStackNavigationOptions = {
        headerShown: true,
        headerStyle: { backgroundColor: colors["app-bar"] },
        headerTintColor: colors.primary,
        headerTitleStyle: { fontFamily: fonts.bold, color: colors["bc-black"] },
        headerBackTitleStyle: { fontFamily: fonts.regular },
        headerShadowVisible: true,
        // Android: the shell already holds the bar below the status bar (and the environment banner); left alone, the
        // bar would leave the status bar's height again above itself
        unstable_nativeProps: {
            headerConfig: { disableTopInsetApplication: true },
        },
    };

    return ({
        route,
        navigation,
    }: {
        route: { name: string; key?: string };
        navigation?: StackNavigation;
    }): NativeStackNavigationOptions => {
        if (hidden.includes(route.name)) return { headerShown: false };
        const right = withoutMenu.includes(route.name) ? {} : menu;
        if (roots === undefined || roots.includes(route.name))
            return {
                ...base,
                ...right,
                ...logo,
                title: "",
                headerBackVisible: false,
            };
        const outer = outerBack(route, navigation);
        const beneath = routeBeneath(route, navigation);
        return {
            ...base,
            ...right,
            ...(outer ? back(outer) : {}),
            title: titles[route.name] ?? "",
            headerBackTitle:
                (beneath === undefined ? undefined : titles[beneath]) ??
                backTitle,
        };
    };
}
