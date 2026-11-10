import { usePathname, useRouter } from "expo-router";
import { useEffect } from "react";
import { Platform, View } from "react-native";

import { AppText, HgButton } from "@/components/common";
import { Path } from "@/constants/path";
import { Action, InternalUrl, Text, Type } from "@/constants/tracking";
import { usePageTitle } from "@/hooks/usePageTitle";
import { loginHref } from "@/navigation/login";
import { services } from "@/services/container";
import { logger } from "@/services/logger";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectIsOffline,
    selectWebConfig,
    useConfigStore,
} from "@/stores/config.store";

/** Port of LogoutCompleteView.vue — returns to the landing page after timeouts.logoutRedirect ms. */
export function LogoutCompleteView() {
    usePageTitle("Logged Out");
    const router = useRouter();
    const pathname = usePathname();
    const configured = useConfigStore((s) =>
        Number(selectWebConfig(s)?.timeouts.logoutRedirect)
    );
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const isOffline = useConfigStore(selectIsOffline);
    // Native: the header has no Log in button, so it is here to sign in again quickly (stated deviation)
    const showLogin = Platform.OS !== "web" && !isAuthenticated && !isOffline;
    const timeout =
        Number.isNaN(configured) || configured <= 0 ? 10000 : configured;

    useEffect(() => {
        if (Number.isNaN(configured) || configured <= 0) {
            logger.warn(
                `Logout redirect timeout is invalid: ${configured}, using default value of 10000ms`
            );
        }
        const t = setTimeout(() => {
            if (pathname.toLowerCase() === Path.LogoutComplete.toLowerCase())
                router.push(Path.Root);
        }, timeout);
        return () => clearTimeout(t);
    }, [configured, timeout, pathname, router]);

    return (
        <View className="items-center">
            <AppText
                variant="h1"
                testID="logout-complete-msg"
                className="mt-12 mb-6 text-center"
            >
                You signed out of your account
            </AppText>
            <AppText className="mb-4 text-center">
                This does not sign out of any other accounts.
            </AppText>
            <AppText className="text-center">
                It is recommended that you sign out of all other accounts and
                close all browser windows to protect your health information.
            </AppText>
            {showLogin ? (
                <HgButton
                    testID="logoutCompleteLoginBtn"
                    text="Log in"
                    icon="right-to-bracket"
                    uppercase={false}
                    className="mt-6 self-center"
                    onPress={() => {
                        services.tracking.trackEvent({
                            action: Action.ButtonClick,
                            text: Text.Login,
                            type: Type.Login,
                            url: InternalUrl.Login,
                        });
                        router.push(loginHref());
                    }}
                />
            ) : null}
        </View>
    );
}
