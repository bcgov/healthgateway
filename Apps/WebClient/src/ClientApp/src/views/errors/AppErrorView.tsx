import { Platform, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import HgLogo from "@/assets/images/gov/hg-logo-rev.svg";
import HgLogoDark from "@/assets/images/gov/hg-logo-rev-dark.svg";
import { AppText, HgAlert, HgButton } from "@/components/common";
import { DevBanner } from "@/components/site/DevBanner";
import { useEnvironmentSelector } from "@/components/site/useEnvironmentSelector";
import { AppErrorType } from "@/constants/errorType";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useColorScheme } from "@/hooks/useTheme";
import { useEnvironmentStore } from "@/stores/environment.store";
import { decorativeProps } from "@/utils/decorative";

interface Props {
    type: AppErrorType;
    onRetry: () => void;
}

/**
 * Port of AppErrorView.vue, shown when the app cannot start. Native also gets a "Try again" button, because an app
 * has no browser refresh; web keeps the web client's page exactly.
 */
export function AppErrorView({ type, onRetry }: Props) {
    usePageTitle("Application Error");
    const isTooManyRequests = type === AppErrorType.TooManyRequests;
    // The menu is out of reach here: without this, a chosen environment that is down would strand the app on every launch
    const selector = useEnvironmentSelector();
    const switched = useEnvironmentStore((s) => s.selected !== undefined);
    // a native dev build always: its environment may not be running, or may not serve what this build needs
    const offerEnvironments = switched || (__DEV__ && Platform.OS !== "web");
    // the logo's height and the same light bar as the rest of the app, as in the web client's v-app-bar color="background"
    const Logo = useColorScheme() === "dark" ? HgLogoDark : HgLogo;
    return (
        <SafeAreaView
            className="flex-1 bg-background"
            edges={["top", "left", "right"]}
        >
            {/* native: which environment failed (none on Production); web keeps the web client's page */}
            {Platform.OS !== "web" ? <DevBanner /> : null}
            <View
                testID="appErrorBar"
                className="flex-row items-center bg-app-bar border-b-2 border-border-light px-4 h-16"
            >
                <Logo
                    width={153}
                    height={48}
                    // decorative: the text beside it names the page
                    {...decorativeProps()}
                />
                <AppText className="flex-1 text-[24px] leading-8 ml-4">
                    Application Error!
                </AppText>
            </View>
            <View className="flex-1 items-center justify-center p-4">
                <View className="w-full max-w-[640px]">
                    {isTooManyRequests ? (
                        <HgAlert
                            type="warning"
                            testID="app-warning"
                            centerContent
                            text="We are unable to complete all actions because the site is too busy. Please try again later."
                        />
                    ) : (
                        <HgAlert
                            type="error"
                            testID="app-error"
                            centerContent
                            text="Unable to load application. Please try refreshing the page or come back later."
                        />
                    )}
                    {Platform.OS !== "web" ? (
                        <HgButton
                            className="self-center"
                            text="Try again"
                            onPress={onRetry}
                        />
                    ) : null}
                    {offerEnvironments ? (
                        <>
                            <HgButton
                                testID="errorChangeEnvironmentBtn"
                                className="self-center mt-2"
                                text="Change Environment"
                                variant="secondary"
                                onPress={selector.open}
                            />
                            {selector.sheet}
                        </>
                    ) : null}
                </View>
            </View>
        </SafeAreaView>
    );
}
