import "@/global.css";
// Metro picks fonts.web.ts on web (loads fonts.web.css) and fonts.ts (empty) on native.
import "@/fonts";

import { useFonts } from "expo-font";
import { usePathname } from "expo-router";
import * as SplashScreen from "expo-splash-screen";
import { StatusBar } from "expo-status-bar";
import { useCallback, useEffect, useState } from "react";
import { Appearance, Platform, View } from "react-native";
import { GestureHandlerRootView } from "react-native-gesture-handler";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { AppShell } from "@/components/site/AppShell";
import { LoadingOverlay, LoadingScope } from "@/components/site/LoadingOverlay";
import { Announcer } from "@/components/ui/Announcer";
import { NewTabHint } from "@/components/ui/ExternalLink";
import { AppErrorType } from "@/constants/errorType";
import { Path } from "@/constants/path";
import { useAppLockLifecycle } from "@/hooks/useAppLock";
import { useRouteGuard } from "@/hooks/useRouteGuard";
import {
    useColorScheme,
    useThemeVars,
    useWebThemeRoot,
} from "@/hooks/useTheme";
import { isTooManyRequestsError } from "@/models/errors";
import { RootNavigator } from "@/navigation/RootNavigator";
import { logger } from "@/services/logger";
import { bootstrap } from "@/stores/bootstrap";
import { selectUpdateRequired, useConfigStore } from "@/stores/config.store";
import { useEnvironmentStore } from "@/stores/environment.store";
import { useLoadingStore } from "@/stores/loading.store";
import {
    selectOnboardingPending,
    useOnboardingStore,
} from "@/stores/onboarding.store";
import { useThemeStore } from "@/stores/theme.store";
import { fontAssets } from "@/theme";
import { registerDevStores } from "@/utils/devStores";
import { LockGate, LockScreen } from "@/views/appLock/LockScreen";
import { AppErrorView } from "@/views/errors/AppErrorView";
import { UpdateRequiredView } from "@/views/errors/UpdateRequiredView";
import { OnboardingView } from "@/views/onboarding/OnboardingView";
import { SplashView } from "@/views/splash/SplashView";

registerDevStores();

SplashScreen.preventAutoHideAsync().catch(() => undefined);
SplashScreen.setOptions({ fade: true, duration: 300 });

type BootState =
    | { status: "booting" }
    | { status: "ready" }
    | { status: "error"; type: AppErrorType };

export default function RootLayout() {
    const pathname = usePathname();
    // Native: load the TTFs before first render. Web: fonts.web.css declares woff2 faces with font-display: swap,
    // so nothing is gated on them (6 MB of TTFs were otherwise downloaded before the first paint).
    const [fontsLoaded] = useFonts(Platform.OS === "web" ? {} : fontAssets);
    const [boot, setBoot] = useState<BootState>({ status: "booting" });
    const [attempt, setAttempt] = useState(0);

    useEffect(() => {
        let cancelled = false;
        bootstrap(pathname)
            .then(
                (newest) => newest && !cancelled && setBoot({ status: "ready" })
            )
            .catch((error) => {
                logger.error(
                    "An error occurred while initializing the app",
                    error
                );
                if (!cancelled) {
                    setBoot({
                        status: "error",
                        type: isTooManyRequestsError(error)
                            ? AppErrorType.TooManyRequests
                            : AppErrorType.General,
                    });
                }
            });
        return () => {
            cancelled = true;
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [attempt]);

    const ready = fontsLoaded && boot.status !== "booting";
    useEffect(() => {
        if (ready) SplashScreen.hide();
    }, [ready]);

    const retry = useCallback(() => {
        setBoot({ status: "booting" });
        setAttempt((a) => a + 1);
    }, []);
    // Environment Selector: bootstraps again behind the loading overlay. The routes stay mounted; remounting restarts at Home.
    const environmentRestarts = useEnvironmentStore((s) => s.restarts);
    useEffect(() => {
        if (environmentRestarts === 0) return;
        const { show, hide } = useLoadingStore.getState();
        show("environment");
        bootstrap(Path.Root)
            .then((newest) => newest && setBoot({ status: "ready" }))
            .catch((error: unknown) => {
                logger.error("Could not switch environment", error);
                setBoot({ status: "error", type: AppErrorType.General });
            })
            .finally(() => hide("environment"));
    }, [environmentRestarts]);
    const onboardingPending = useOnboardingStore(selectOnboardingPending);
    const updateRequired = useConfigStore(selectUpdateRequired);
    useRouteGuard(boot.status === "ready" && !updateRequired);
    useAppLockLifecycle();
    const scheme = useColorScheme();
    const themeVars = useThemeVars();
    useWebThemeRoot(scheme);
    // Native system chrome (tab bar, alerts, keyboard) follows the in-app appearance choice, not only the OS.
    const preference = useThemeStore((s) => s.preference);
    useEffect(() => {
        if (Platform.OS === "web") return;
        Appearance.setColorScheme(
            preference === "system" ? "unspecified" : preference
        );
    }, [preference]);

    return (
        // Native gestures anywhere in the app: the dependent tab swipes and the drawer's swipe to close
        <GestureHandlerRootView style={{ flex: 1 }}>
            <SafeAreaProvider>
                {/* Splash is always dark navy; afterwards the bar follows the palette. */}
                <StatusBar
                    style={ready && scheme === "light" ? "dark" : "light"}
                />
                <View style={themeVars} className="flex-1">
                    <LoadingScope>
                        {!ready ? (
                            // Web: plain white while config/fonts load (a few hundred ms), no splash flash.
                            // Native: a React twin of the launch screen covers the hand-off from the native splash.
                            Platform.OS === "web" ? (
                                <View className="flex-1 bg-background" />
                            ) : (
                                <SplashView />
                            )
                        ) : updateRequired ? (
                            <UpdateRequiredView />
                        ) : boot.status === "error" ? (
                            <AppErrorView type={boot.type} onRetry={retry} />
                        ) : (
                            <LockGate covered={onboardingPending}>
                                <AppShell>
                                    <RootNavigator />
                                </AppShell>
                            </LockGate>
                        )}
                    </LoadingScope>
                    <LoadingOverlay />
                    <Announcer />
                    <NewTabHint />
                    {/* Over the loading overlay too. Overlays, so the routes stay mounted: remounting them restarts at Home. */}
                    {ready && boot.status === "ready" && !updateRequired ? (
                        onboardingPending ? (
                            <View className="absolute inset-0 z-50 bg-background">
                                <OnboardingView />
                            </View>
                        ) : (
                            <LockScreen />
                        )
                    ) : null}
                </View>
            </SafeAreaProvider>
        </GestureHandlerRootView>
    );
}
