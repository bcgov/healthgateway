import type { Href } from "expo-router";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { selectIdentityProviders, useConfigStore } from "@/stores/config.store";

/** The providers a person can choose from: the platform's list without the disabled ones. */
export function enabledIdentityProviders() {
    return selectIdentityProviders(useConfigStore.getState()).filter(
        (p) => !p.disabled
    );
}

/**
 * Where "Log in" goes. Native with more than one provider: the login sheet, to choose one. Otherwise the login page,
 * which starts the only provider by itself, with no page of ours in between; web always has the page.
 */
export function loginHref(redirect?: string): Href {
    const sheet =
        Platform.OS !== "web" && enabledIdentityProviders().length > 1;
    const pathname = sheet ? Path.LoginProviders : Path.Login;
    return redirect ? { pathname, params: { redirect } } : pathname;
}

/**
 * The label for a "Log in" button that opens `loginHref`: "Log in" when there is a choice of providers, or "Log in with
 * <the provider>" when there is only one. Each platform counts its own list (`selectIdentityProviders`).
 */
export function useLoginLabel(): string {
    const providers = useConfigStore(selectIdentityProviders);
    const enabled = providers.filter((p) => !p.disabled);
    return enabled.length === 1 ? `Log in with ${enabled[0].name}` : "Log in";
}
