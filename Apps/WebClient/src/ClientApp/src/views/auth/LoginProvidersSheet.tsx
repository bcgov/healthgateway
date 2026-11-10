import { useLocalSearchParams, useRouter } from "expo-router";
import { Platform, Pressable, View } from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";

import { AppText, HgButton, Icon } from "@/components/common";
import { Path } from "@/constants/path";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useThemeColors } from "@/hooks/useTheme";
import type { IdentityProviderConfiguration } from "@/models/configData";
import { selectIdentityProviders, useConfigStore } from "@/stores/config.store";

import { alertLoginFailed, useProviderSignIn } from "./useProviderSignIn";

/**
 * Native: the login picker as a sheet over the page that opened it (presented as a form sheet by `RootNavigator`), for
 * when there is more than one identity provider; with one, "Log in" starts it directly (`loginHref`). The providers'
 * buttons keep the login page's ids (`#<id>Btn`), so the Maestro and Cypress flows find them either way. A failed login
 * is the platform alert; a cancelled one, or Cancel on the alert, leaves the sheet up to choose again.
 */
export function LoginProvidersSheet() {
    usePageTitle("Log In");
    const router = useRouter();
    const insets = useSafeAreaInsets();
    const colors = useThemeColors();
    const params = useLocalSearchParams<{ redirect?: string }>();
    const redirectPath = params.redirect ? String(params.redirect) : Path.Home;
    const providers = useConfigStore(selectIdentityProviders);
    const enabled = providers.filter((p) => !p.disabled);
    const { start, pending } = useProviderSignIn(redirectPath);

    async function choose(provider: IdentityProviderConfiguration) {
        const outcome = await start(provider);
        if (outcome === "failed")
            alertLoginFailed(
                () => void choose(provider),
                () => undefined
            );
    }

    return (
        <View
            testID="loginSheet"
            className="bg-surface px-5 pt-2"
            // iOS: fitToContents already leaves room for the home indicator
            style={{
                paddingBottom: Platform.OS === "ios" ? 16 : insets.bottom + 16,
            }}
        >
            <View className="flex-row items-center justify-between h-14">
                <View className="w-11" />
                <AppText
                    variant="h3"
                    role="heading"
                    aria-level={2}
                    className="text-bc-black"
                >
                    Log In
                </AppText>
                <Pressable
                    testID="loginSheetCloseBtn"
                    accessibilityRole="button"
                    accessibilityLabel="Close"
                    onPress={() => router.back()}
                    className="w-11 h-11 items-center justify-center"
                >
                    <Icon name="xmark" size={20} color={colors["bc-black"]} />
                </Pressable>
            </View>
            <View className="pt-2">
                {enabled.map((p, i) => (
                    <View key={p.id}>
                        <HgButton
                            testID={`${p.id}Btn`}
                            nativeID={`${p.id}Btn`}
                            text={p.name}
                            icon={p.icon}
                            block
                            className="justify-start"
                            loading={pending === p.id}
                            disabled={pending !== undefined}
                            onPress={() => void choose(p)}
                        />
                        {i < enabled.length - 1 ? (
                            <AppText className="text-center my-1">or</AppText>
                        ) : null}
                    </View>
                ))}
            </View>
        </View>
    );
}
