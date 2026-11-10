import { usePathname } from "expo-router";

import { Path } from "@/constants/path";

/** App.vue rules for which chrome a route gets: nothing on the login callback or VPP login, no footer on Registration. */
export function usePageChrome() {
    const path = usePathname().toLowerCase();
    const isVppLogin = path === Path.VppLogin.toLowerCase();
    const hideChrome = path === Path.LoginCallback.toLowerCase() || isVppLogin;
    const hideFooter = hideChrome || path === Path.Registration.toLowerCase();
    return { hideChrome, hideFooter, hideSidebar: isVppLogin };
}
