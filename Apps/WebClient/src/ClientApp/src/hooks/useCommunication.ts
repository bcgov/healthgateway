import { usePathname } from "expo-router";
import { useEffect, useRef } from "react";
import { AppState } from "react-native";

import { CommunicationType } from "@/models/communication";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import {
    COMMUNICATIONS_CACHE_SERVICE,
    useCommunicationStore,
} from "@/stores/communication.store";
import {
    selectCacheMs,
    selectIsOffline,
    useConfigStore,
} from "@/stores/config.store";
import {
    selectIsValidIdentityProvider,
    selectUserIsRegistered,
    useUserStore,
} from "@/stores/user.store";

/** Which communication the banner shows (CommunicationComponent.vue): InApp once registered, Banner otherwise. */
export function useCommunicationType(): CommunicationType {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isOffline = useConfigStore(selectIsOffline);
    const isRegistered = useUserStore(selectUserIsRegistered);
    const isValidIdp = useUserStore(selectIsValidIdentityProvider);
    return isAuthenticated && isRegistered && isValidIdp && !isOffline
        ? CommunicationType.InApp
        : CommunicationType.Banner;
}

/**
 * Keeps the banner's communication current, for the whole app: mounted once, in the shell. On each page change, when
 * the type changes, and when the app comes back to the foreground, it asks the store, which only calls the API once
 * what it holds is older than the configured cache time (`webClient.cache` / `webClient.mobile.cache`, service
 * "Communications").
 */
export function useCommunicationRetrieval(): void {
    const pathname = usePathname();
    const type = useCommunicationType();
    const maxAgeMs = useConfigStore((s) =>
        selectCacheMs(s, COMMUNICATIONS_CACHE_SERVICE)
    );
    // what the foreground listener asks for: the latest, without subscribing again on every change
    const latest = useRef({ type, maxAgeMs });

    useEffect(() => {
        latest.current = { type, maxAgeMs };
        if (!services.isInitialized) return;
        void useCommunicationStore.getState().retrieve(type, maxAgeMs);
    }, [type, pathname, maxAgeMs]);

    // back from the background (web: the tab shown again), with no page change to prompt a check
    useEffect(() => {
        const subscription = AppState.addEventListener("change", (state) => {
            if (state !== "active" || !services.isInitialized) return;
            const { type: current, maxAgeMs: maxAge } = latest.current;
            void useCommunicationStore.getState().retrieve(current, maxAge);
        });
        return () => subscription.remove();
    }, []);
}
