import {
    useNavigation,
    useNavigationState,
    useRoute,
} from "expo-router/react-navigation";
import {
    createContext,
    type ReactNode,
    useContext,
    useLayoutEffect,
} from "react";

import type { OuterBack } from "@/navigation/stackBack";

import { useNativeHeaderItems } from "./nativeHeaderItems";

/** Where Back leads from the first page of this stack: worked out by the layout, drawn by each page (`SettingsPage`). */
export const WayBackContext = createContext<OuterBack | undefined>(undefined);

/**
 * Each page's back button, set once the navigation state has settled. Read during the first render, a navigator's state
 * can still be the one before the push (`getState()` lagged on the Download tab), so this reads it through hooks, which
 * render again when it changes. The first page draws the way back out of this stack; one above another (Security over
 * Terms of Service) keeps the platform's own, named for the page beneath.
 */
export function SettingsPage({
    children,
    titles,
}: {
    children?: ReactNode;
    /** The stack's page titles by route name: the platform back button's label to the page beneath. */
    titles: Readonly<Record<string, string>>;
}) {
    const navigation = useNavigation();
    const route = useRoute();
    const position = useNavigationState((state) =>
        state.routes.findIndex((r) => r.key === route.key)
    );
    const beneathName = useNavigationState((state) =>
        position > 0 ? state.routes[position - 1]?.name : undefined
    );
    const wayBack = useContext(WayBackContext);
    const { back } = useNativeHeaderItems();

    useLayoutEffect(() => {
        if (position === 0 && wayBack) navigation.setOptions(back(wayBack));
        else if (position > 0)
            navigation.setOptions({
                headerBackTitle: beneathName ? titles[beneathName] : undefined,
            });
    }, [back, beneathName, navigation, position, titles, wayBack]);

    return children;
}
