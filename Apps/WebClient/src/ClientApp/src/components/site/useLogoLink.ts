import { usePathname, useRouter } from "expo-router";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { Action, Destination, InternalUrl, Text } from "@/constants/tracking";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";

/**
 * The Health Gateway logo's link (HeaderComponent.vue's router-link), for the web header and the native navigation bars.
 * Signed in → Home (the route guard redirects non-registered states to the right page); signed out → landing. Avoids the
 * web client's gap where a signed-in user who is not yet in the "registered" state (profile still loading, pending
 * deletion) was sent to the marketing page.
 */
export function useLogoLink() {
    const router = useRouter();
    const pathname = usePathname();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const destination = isAuthenticated ? Path.Home : Path.Root;

    /**
     * Web: browser history, like the web client's link. Native: replaces the page. `dismissTo` did nothing on real
     * devices from another tab, and a push would stack another copy, leaving something to swipe back to.
     */
    function follow() {
        services.tracking.trackEvent({
            action: Action.InternalLink,
            text: Text.HealthGatewayLogo,
            destination: Destination.Home,
            url: InternalUrl.Home,
        });
        if (Platform.OS === "web") router.push(destination);
        else if (pathname !== destination) router.replace(destination);
    }

    return { destination, follow };
}
