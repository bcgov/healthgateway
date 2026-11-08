import { useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform, View } from "react-native";

import { Loading } from "@/components/ui";
import { Path } from "@/constants/path";
import { describeError, logger } from "@/services/logger";
import { useAuthStore } from "@/stores/auth.store";

/** Port of LogoutView.vue. On web keycloak redirects to /logoutComplete; native navigates itself. */
export function LogoutView() {
    const router = useRouter();
    useEffect(() => {
        void useAuthStore
            .getState()
            .signOut()
            .catch((error: unknown) =>
                logger.warn(`Sign out failed: ${describeError(error)}`)
            )
            .finally(() => {
                if (Platform.OS !== "web") router.replace(Path.LogoutComplete);
            });
    }, [router]);
    return (
        <View className="min-h-[320px]">
            <Loading isLoading text="Signing Out..." />
        </View>
    );
}
