import { useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform, View } from "react-native";

import { Loading } from "@/components/site/Loading";
import { Path } from "@/constants/path";
import { describeError, logger } from "@/services/logger";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";

/** Port of LogoutView.vue. On web keycloak redirects to /logoutComplete; native navigates itself. */
export function LogoutView() {
    const router = useRouter();
    // Native, device security: under the lock nothing is signed out. A link that opens the app here
    // (healthgateway://logout) would delete a sealed login without the device's prompt, and without telling Keycloak,
    // whose refresh token is still sealed. Once the prompt has passed, the sign-out goes ahead.
    const locked = useAppLockStore((s) => s.locked);
    useEffect(() => {
        if (locked) return;
        void useAuthStore
            .getState()
            .signOut()
            .catch((error: unknown) =>
                logger.warn(`Sign out failed: ${describeError(error)}`)
            )
            .finally(() => {
                if (Platform.OS !== "web") router.replace(Path.LogoutComplete);
            });
    }, [router, locked]);
    return (
        <View className="min-h-[320px]">
            <Loading isLoading text="Signing Out..." />
        </View>
    );
}
