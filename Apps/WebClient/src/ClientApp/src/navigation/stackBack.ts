import { Path } from "@/constants/path";

import { menuItems, menuRouteName } from "./menu";

/** A route as a navigation state holds it; a partial state (a deep link not yet opened) may lack the key. */
export interface StackRoute {
    key?: string;
    name: string;
    state?: StackState;
}

interface StackState {
    type?: string;
    index?: number;
    routes: readonly StackRoute[];
}

/** The parts of a screen's `navigation` prop the back button needs. */
export interface StackNavigation {
    getState(): StackState;
    getParent(): StackNavigation | undefined;
    goBack(): void;
}

/**
 * The pages outside the tabs that the back button names, by route name: the places one goes to. On iOS and Android the
 * landing page is where one logs in, so it is named for that. Any other page, a status rather than a place (Logged
 * Out, Page Not Found, Unauthorized): "Back".
 */
export const outsidePageTitles: Readonly<Record<string, string>> = {
    index: "Log In",
    login: "Log In",
};

/** The titles of the pages pushed on the Home tab's stack, by route name. */
export const homeStackTitles: Readonly<Record<string, string>> = {
    otherRecordSources: "Other record sources",
    profile: "Profile",
};

/** The titles of the pages pushed on the Dependents tab's stack, by route name. */
export const dependentsStackTitles: Readonly<Record<string, string>> = {
    "[id]/index": "Health Records",
    "[id]/timeline": "Health Records",
};

/** The pages pushed on each tab's stack, by the tab's route name. */
const tabStackTitles: Readonly<
    Record<string, Readonly<Record<string, string>>>
> = {
    [menuRouteName(Path.Home)]: homeStackTitles,
    [menuRouteName(Path.Dependents)]: dependentsStackTitles,
};

/** Each tab's name, by its route name: what the back button says to a tab's first page, which shows the logo. */
const tabTitle = (name: string) =>
    menuItems.find((item) => menuRouteName(item.path) === name)?.title;

/** The back button's label when the page beneath has no name. */
export const DEFAULT_BACK_TITLE = "Back";

/** A back button to a page outside the route's own stack. */
export interface OuterBack {
    label: string;
    goBack: () => void;
}

const topIndex = (state: StackState) => state.index ?? state.routes.length - 1;

/** The page a navigator shows: the deepest focused route under `route`. */
function shownRoute(route: StackRoute): StackRoute {
    let shown = route;
    while (shown.state && shown.state.routes.length > 0)
        shown = shown.state.routes[topIndex(shown.state)] ?? shown;
    return shown;
}

/**
 * What the back button calls the page `route` shows. The tabs: the tab's name on its first page, or the title of the
 * page pushed on its stack (Profile). Outside the tabs: `outsidePageTitles`. Otherwise "Back".
 */
export function shownTitle(route: StackRoute): string {
    if (route.name === "(tabs)" && route.state) {
        const tab = route.state.routes[topIndex(route.state)];
        const stack = tab?.state;
        const page = stack ? stack.routes[topIndex(stack)] : undefined;
        if (tab && page && stack && topIndex(stack) > 0)
            return (
                tabStackTitles[tab.name]?.[page.name] ??
                tabTitle(tab.name) ??
                DEFAULT_BACK_TITLE
            );
        return (tab && tabTitle(tab.name)) ?? DEFAULT_BACK_TITLE;
    }
    return outsidePageTitles[shownRoute(route).name] ?? DEFAULT_BACK_TITLE;
}

function indexInStack(
    route: { key?: string },
    navigation: StackNavigation
): number {
    if (route.key === undefined) return -1;
    return navigation
        .getState()
        .routes.findIndex((r) => r.key !== undefined && r.key === route.key);
}

/** The route beneath `route` in its own stack, whose back button the platform draws; none when it is the first. */
export function routeBeneath(
    route: { key?: string },
    navigation: StackNavigation | undefined
): string | undefined {
    if (!navigation) return undefined;
    const index = indexInStack(route, navigation);
    return index > 0
        ? navigation.getState().routes[index - 1]?.name
        : undefined;
}

/**
 * The way back from a page that is the first in its own stack while a stack around it has a page beneath: Security or
 * Terms of Service, pushed over whatever page opened them, in a tab or outside the tabs (`RootNavigator`). The platform
 * draws no back button there (its stack has nothing beneath), so the bar draws one that goes back in that outer stack,
 * named for the page it returns to (`shownTitle`). None for a page with a page beneath in its own stack, or with nothing
 * to go back to.
 */
export function outerBack(
    route: { key?: string },
    navigation: StackNavigation | undefined
): OuterBack | undefined {
    if (!navigation || indexInStack(route, navigation) !== 0) return undefined;
    for (let outer = navigation.getParent(); outer; outer = outer.getParent()) {
        const state = outer.getState();
        // the tabs between them are not a way back
        if (state.type !== "stack") continue;
        const index = topIndex(state);
        const beneath = index > 0 ? state.routes[index - 1] : undefined;
        if (!beneath) continue;
        const target = outer;
        return {
            label: shownTitle(beneath),
            goBack: () => target.goBack(),
        };
    }
    return undefined;
}
