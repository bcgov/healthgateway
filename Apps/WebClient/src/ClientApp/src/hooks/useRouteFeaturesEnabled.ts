import { useIsFocused, usePathname } from "expo-router";

import { routeFeaturesEnabled } from "@/navigation/routeMeta";
import { selectFeatureToggles, useConfigStore } from "@/stores/config.store";

/**
 * Whether a page may show its content: false while it is the focused screen of a route whose feature is switched off.
 * The route guard sends the user on from such a route, but it runs in the root layout after the page's own effects
 * have started its data requests; the web client's beforeEach guard decides before the page exists. `PageScroll`
 * holds the content back on this, so no page asks for data it may not show.
 * Only the focused screen is judged, because the path is always the focused route's: a native tab kept mounted in
 * the background, or a screen under a pushed one, keeps what it shows.
 */
export function useRouteFeaturesEnabled(): boolean {
    const pathname = usePathname();
    const focused = useIsFocused();
    const toggles = useConfigStore(selectFeatureToggles);
    return !focused || routeFeaturesEnabled(pathname, toggles);
}
