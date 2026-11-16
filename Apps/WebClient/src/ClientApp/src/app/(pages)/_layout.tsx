import { Stack } from "expo-router";
import {
    CommonActions,
    type NavigationProp,
    type ParamListBase,
} from "expo-router/react-navigation";

import { PageScroll } from "@/components/site/PageScroll";
import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { useThemeColors } from "@/hooks/useTheme";

/** App.vue's chrome rules (`usePageChrome`): no app bar on the login callback or VPP login. */
const NO_BAR = ["loginCallback", "vppLogin"];

/** These pages replace one another, as the Slot this replaced did: only the newest stays, so there is nothing to go back to. */
function keepTopOnly(navigation: NavigationProp<ParamListBase>) {
    const state = navigation.getState();
    if (state.routes.length < 2) return;
    const top = state.routes[state.index];
    if (!top) return;
    // same key: the top screen stays mounted; only the ones beneath go
    navigation.dispatch(
        CommonActions.reset({ ...state, index: 0, routes: [top] })
    );
}

/**
 * Native: routes outside the tab bar (landing, registration, Terms of Service acceptance, the error pages), each in the
 * page scroll view and under the navigation bar every page has: the logo, and the settings menu (signed out) or the
 * account menu (`useNativeStackOptions`). A stack only for that bar: the change between pages stays instant, with no
 * back. Web: `_layout.web.tsx`.
 */
export default function PagesLayout() {
    const colors = useThemeColors();
    const pageOptions = useNativeStackOptions({ hidden: NO_BAR });
    return (
        <Stack
            screenOptions={(props) => ({
                ...pageOptions(props),
                animation: "none",
                gestureEnabled: false,
                contentStyle: { backgroundColor: colors.background },
            })}
            screenLayout={({ children }) => <PageScroll>{children}</PageScroll>}
            screenListeners={({ navigation }) => ({
                focus: () =>
                    keepTopOnly(navigation as NavigationProp<ParamListBase>),
            })}
        />
    );
}
