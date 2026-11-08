import { useLocalSearchParams, useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform, View } from "react-native";

import { Loading } from "@/components/ui";
import { Path } from "@/constants/path";
import { usePageTitle } from "@/hooks/usePageTitle";
import { isSessionEnded } from "@/models/errors";
import { toHref } from "@/navigation/href";
import { describeError, logger } from "@/services/logger";
import { useAuthStore } from "@/stores/auth.store";

import { completeLogin } from "./completeLogin";

/** Longer than the token exchange and first data load normally take. */
const ABANDONED_CALLBACK_MS = 30000;

/** Port of LoginCallbackView.vue (web only: keycloak-js has already consumed the code during init). */
export function LoginCallbackView() {
    usePageTitle("Logging In");
    const router = useRouter();
    const params = useLocalSearchParams<{ redirect?: string }>();
    const redirectPath = params.redirect ? String(params.redirect) : Path.Home;

    useEffect(() => {
        // Native: the sign-in that opened the browser is still running and finishes the login. Android also delivers its
        // return here as a link, and signing in again would open the browser a second time. Only a link with no
        // sign-in behind it stays here, so it gives way after a while.
        if (Platform.OS !== "web") {
            const timer = setTimeout(
                () => router.replace(Path.Root),
                ABANDONED_CALLBACK_MS
            );
            return () => clearTimeout(timer);
        }
        void (async () => {
            const auth = useAuthStore.getState();
            try {
                await auth.signIn(redirectPath);
                const target = await completeLogin(redirectPath);
                router.replace(toHref(target));
            } catch (error) {
                if (isSessionEnded(error)) return;
                logger.error(`LoginCallback Error: ${describeError(error)}`);
                auth.clearStorage();
                router.replace({
                    pathname: Path.Login,
                    params: { isRetry: "true" },
                });
            }
        })();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <View className="min-h-[320px]">
            <Loading isLoading text="Logging In..." />
        </View>
    );
}
