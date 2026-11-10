import { useAuthStore } from "@/stores/auth.store";
import { selectIsOffline, useConfigStore } from "@/stores/config.store";
import {
    selectIsValidIdentityProvider,
    selectUserIsRegistered,
    useUserStore,
} from "@/stores/user.store";

/** FooterComponent.vue's visibility rule, shared by the footer (web) and the Support menu (native). */
export function useSupportLinksShown(): boolean {
    const isOffline = useConfigStore(selectIsOffline);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    const isRegistered = useUserStore(selectUserIsRegistered);
    return !isOffline && (!isAuthenticated || !isValidIdp || isRegistered);
}
