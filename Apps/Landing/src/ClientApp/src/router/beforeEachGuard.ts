import { NavigationGuard, RouteLocationNormalizedLoaded } from "vue-router";

import { useConfigStore } from "@/stores/config";

export const beforeEachGuard: NavigationGuard = async (
    to: RouteLocationNormalizedLoaded,
    _from: RouteLocationNormalizedLoaded
) => {
    const configStore = useConfigStore();
    const webClientConfig = configStore.config.webClient;

    const meta: { redirectPath?: string; classicRedirectPath?: string } =
        to.meta;

    if (meta?.redirectPath && webClientConfig.betaUrl) {
        let url = webClientConfig.betaUrl + meta.redirectPath;
        if (to.params.inviteKey) {
            url += "/" + to.params.inviteKey;
        }

        window.location.assign(url);
        return false;
    }

    if (meta?.classicRedirectPath && webClientConfig.classicUrl) {
        const url = webClientConfig.classicUrl + meta.classicRedirectPath;
        window.location.assign(url);
        return false;
    }
};
