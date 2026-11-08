import { Platform, View } from "react-native";

import { AppText } from "@/components/ui";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import {
    selectEnvironment,
    useEnvironmentStore,
} from "@/stores/environment.store";

const textClass = "text-on-accent text-[12px] leading-4 tracking-normal";

/** The web client's production hosts: any other host, or a development build, shows the banner. */
const PRODUCTION_HOSTS = [
    "HEALTHGATEWAY",
    "WWW.HEALTHGATEWAY",
    "CLASSIC.HEALTHGATEWAY",
    "SECURE.HEALTHGATEWAY",
    "LEGACY.HEALTHGATEWAY",
];

export const isProduction = (host: string) =>
    !__DEV__ && PRODUCTION_HOSTS.some((name) => host.startsWith(name));

/** Native (stated deviation): the environment's name, and nothing on Production. */
function NativeEnvironmentBanner() {
    const { banner } = useEnvironmentStore(selectEnvironment);
    if (!banner) return null;
    return (
        <View className="bg-accent items-center py-0.5 px-2 min-h-[6px]">
            <AppText
                testID="environmentBanner"
                variant="body-medium"
                numberOfLines={1}
                className={`font-bold ${textClass}`}
            >
                {banner}
            </AppText>
        </View>
    );
}

/** Port of DevBannerComponent.vue: shown for every non-production environment. */
function WebDevBanner() {
    const { isMobile } = useBreakpoint();
    const host = globalThis.location.hostname.toLocaleUpperCase();
    if (isProduction(host)) return null;
    return (
        <View className="bg-accent items-center py-0.5 px-2 min-h-[6px]">
            <AppText
                variant="body-medium"
                numberOfLines={1}
                className={textClass}
            >
                {!isMobile ? "Non-production environment: " : ""}
                <AppText
                    variant="body-medium"
                    className={`font-bold ${textClass}`}
                >
                    {host}
                </AppText>
            </AppText>
        </View>
    );
}

export function DevBanner() {
    return Platform.OS === "web" ? (
        <WebDevBanner />
    ) : (
        <NativeEnvironmentBanner />
    );
}
