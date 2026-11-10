import { useLocalSearchParams, useRouter } from "expo-router";
import { useEffect, useRef, useState } from "react";
import { Platform, View } from "react-native";

import { AppText, HgAlert, HgButton } from "@/components/common";
import { Loading } from "@/components/site/Loading";
import { Path } from "@/constants/path";
import { usePageTitle } from "@/hooks/usePageTitle";
import type { IdentityProviderConfiguration } from "@/models/configData";
import { toHref } from "@/navigation/href";
import { useAuthStore } from "@/stores/auth.store";
import { selectIdentityProviders, useConfigStore } from "@/stores/config.store";

import { completeLogin } from "./completeLogin";
import {
    alertLoginFailed,
    LOGIN_FAILED_MESSAGE,
    useProviderSignIn,
} from "./useProviderSignIn";

/**
 * Port of LoginView.vue. Identity providers come from /configuration on every platform. Native opens this page only to
 * start one provider (the only one, or one named by a deep link): with more to choose from, "Log in" opens the login
 * sheet instead (`loginHref`), and this page's own list is left for anything that still lands here.
 */
export function LoginView() {
    usePageTitle("Log In");
    const router = useRouter();
    const params = useLocalSearchParams<{
        redirect?: string;
        isRetry?: string;
        idp?: string;
    }>();
    const providers = useConfigStore(selectIdentityProviders);
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

    const redirectPath = params.redirect ? String(params.redirect) : Path.Home;
    const enabled = providers.filter((p) => !p.disabled);
    const hasMultiple = enabled.length > 1;
    const isRetry = params.isRetry === "true";
    // A single provider starts straight away, with no page of ours in between (as the web client does).
    // A deep link such as healthgateway://login?idp=BCSC (or /login?idp=bcsc on web) starts that provider directly.
    const requested = params.idp
        ? enabled.find(
              (p) =>
                  p.id.toLowerCase() === String(params.idp).toLowerCase() ||
                  p.hint.toLowerCase() === String(params.idp).toLowerCase()
          )
        : undefined;
    const autoRedirect =
        !isRetry && (requested !== undefined || enabled.length === 1);

    const [isLoading, setIsLoading] = useState(isAuthenticated || autoRedirect);
    const [showError, setShowError] = useState(isRetry);
    const [showPicker, setShowPicker] = useState(isRetry);
    const autoStarted = useRef(false);
    const { start } = useProviderSignIn(redirectPath);

    async function signInAndRedirect(provider: IdentityProviderConfiguration) {
        setIsLoading(true);
        setShowError(false);
        const outcome = await start(provider);
        if (outcome === "done") return;
        if (outcome === "ended") {
            setIsLoading(false);
            return;
        }
        if (Platform.OS !== "web") {
            // Native: a failure is the platform alert, and a cancel goes back to where the person started. A page that
            // was only passed through (one provider, or a deep link to one) has nothing to come back to.
            const leave = () => {
                if (autoRedirect) router.replace(Path.Root);
                else setIsLoading(false);
            };
            if (outcome === "failed")
                alertLoginFailed(() => void signInAndRedirect(provider), leave);
            else leave();
            return;
        }
        // nothing here to come back to: this page was only ever passed through
        if (outcome === "cancelled" && enabled.length === 1) {
            router.replace(Path.Root);
            return;
        }
        setShowError(outcome === "failed");
        // after a cancel or failure always offer the provider list so the person can try again
        setShowPicker(true);
        setIsLoading(false);
    }

    useEffect(() => {
        if (autoStarted.current) return;
        autoStarted.current = true;
        if (isAuthenticated) {
            void completeLogin(redirectPath).then((t) =>
                router.replace(toHref(t))
            );
        } else if (autoRedirect) {
            void Promise.resolve().then(() =>
                signInAndRedirect(requested ?? enabled[0])
            );
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    return (
        <View className="items-center">
            <Loading isLoading={isLoading} />
            {showError ? (
                <View className="w-full max-w-[520px]">
                    <HgAlert
                        type="error"
                        live="assertive"
                        title="Error"
                        text={LOGIN_FAILED_MESSAGE}
                        closable
                        onClose={() => setShowError(false)}
                    />
                </View>
            ) : null}
            {enabled.length > 0 ? (
                <View
                    testID="loginPicker"
                    className="w-full max-w-[400px] bg-surface rounded-md shadow-lg border border-border-light"
                >
                    <AppText
                        variant="h3"
                        role="heading"
                        aria-level={2}
                        className="text-center pt-5 text-bc-black"
                    >
                        Log In
                    </AppText>
                    {/* the "Redirecting…" text only while an automatic redirect is under way */}
                    {hasMultiple || showError || showPicker || !autoRedirect ? (
                        <View className="px-10 py-8">
                            {enabled.map((p, i) => (
                                <View key={p.id}>
                                    <HgButton
                                        testID={`${p.id}Btn`}
                                        nativeID={`${p.id}Btn`}
                                        text={p.name}
                                        icon={p.icon}
                                        block
                                        className="justify-start"
                                        onPress={() =>
                                            void signInAndRedirect(p)
                                        }
                                    />
                                    {i < enabled.length - 1 ? (
                                        <AppText className="text-center my-1">
                                            or
                                        </AppText>
                                    ) : null}
                                </View>
                            ))}
                        </View>
                    ) : (
                        <AppText className="p-5 text-center">
                            Redirecting to{" "}
                            <AppText className="font-bold">
                                {enabled[0].name}
                            </AppText>
                            ...
                        </AppText>
                    )}
                </View>
            ) : (
                <AppText>No login providers configured</AppText>
            )}
        </View>
    );
}
