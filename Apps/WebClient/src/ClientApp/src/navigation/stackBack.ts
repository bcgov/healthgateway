/** A route as a navigation state holds it; a partial state (a deep link not yet opened) may lack the key. */
interface StackRoute {
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
 * The pages outside the tabs that the back button names, by route name: the places one goes to, by their own title
 * (`usePageTitle`), the landing page being the signed-out Home. Any other page, a status rather than a place (Logged
 * Out, Page Not Found, Unauthorized): "Back".
 */
export const outsidePageTitles: Readonly<Record<string, string>> = {
    index: "Home",
    login: "Log In",
};

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
 * The way back from a page that is the first in its own stack while a stack around it has a page beneath: signed out,
 * the tabs group is pushed over the page outside the tabs that opened Security or Terms of Service (`RootNavigator`).
 * The platform draws no back button there (its stack has nothing beneath), so the bar draws one that goes back in that
 * outer stack, named for the page it returns to. None for a page with a page beneath in its own stack, or with nothing
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
            label:
                outsidePageTitles[shownRoute(beneath).name] ??
                DEFAULT_BACK_TITLE,
            goBack: () => target.goBack(),
        };
    }
    return undefined;
}
