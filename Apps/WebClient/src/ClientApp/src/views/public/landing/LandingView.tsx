import { useRouter } from "expo-router";
import {
    Image,
    type ImageSourcePropType,
    Platform,
    Pressable,
    View,
} from "react-native";

import { AppText, HgButton } from "@/components/common";
import {
    getAccessLinks,
    getHealthServicesLinks,
} from "@/constants/accessLinks";
import { ExternalUrl } from "@/constants/externalUrls";
import { Path } from "@/constants/path";
import {
    Action,
    Destination,
    InternalUrl,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import { usePageTitle } from "@/hooks/usePageTitle";
import { useColorScheme } from "@/hooks/useTheme";
import type { InfoTile } from "@/models/infoTile";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import {
    selectIsOffline,
    selectWebConfig,
    useConfigStore,
} from "@/stores/config.store";
import { hrefProps } from "@/utils/hrefProps";
import { openExternal } from "@/utils/openExternal";

import { LandingIntro } from "./LandingIntro";
import { TileCarousel } from "./TileCarousel";

/** Port of LandingView.vue. Native keeps only the top row. */
export function LandingView() {
    usePageTitle("Health Gateway");
    const router = useRouter();
    const { lgAndUp } = useBreakpoint();
    const isOffline = useConfigStore(selectIsOffline);
    const offlineMessage = useConfigStore(
        (s) => selectWebConfig(s)?.offlineMode?.message ?? ""
    );
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

    if (isOffline) {
        return (
            <View className="p-4 items-center">
                <AppText variant="h1" className="mb-6 text-center">
                    The site is offline for maintenance
                </AppText>
                <AppText testID="offlineMessage">{offlineMessage}</AppText>
            </View>
        );
    }

    return (
        // LandingView.vue wraps the page in its own v-container inside App.vue's fluid one: 16pt padding
        // (32pt gutters on phones) and Vuetify's container max-widths (md 900, lg 1200, xl 1800, xxl 2400).
        <View className="w-full self-center p-4 md:max-w-[900px] lg:max-w-[1200px] xl:max-w-[1800px] 2xl:max-w-[2400px]">
            <View className="flex-row items-start">
                {/* v-row no-gutters, v-col lg="5": no padding beside the intro */}
                <View className={lgAndUp ? "w-5/12" : "flex-1"}>
                    <LandingIntro />
                    {!isAuthenticated ? (
                        <View className="mb-4">
                            <HgButton
                                testID="btnLogin"
                                nativeID="btnLogin"
                                text="Log in with BC Services Card"
                                uppercase={false}
                                onPress={() => {
                                    services.tracking.trackEvent({
                                        action: Action.ButtonClick,
                                        text: Text.LoginBCSC,
                                        type: Type.Login,
                                        url: InternalUrl.Login,
                                    });
                                    router.push(Path.Login);
                                }}
                            />
                            <View className="flex-row items-center mt-4">
                                <AppText className="mr-2">
                                    Need an account?
                                </AppText>
                                <HgButton
                                    testID="btnStart"
                                    nativeID="btnStart"
                                    text="Register"
                                    variant="link"
                                    uppercase={false}
                                    onPress={() => {
                                        services.tracking.trackEvent({
                                            action: Action.InternalLink,
                                            text: Text.Register,
                                            origin: Origin.Landing,
                                            destination:
                                                Destination.Registration,
                                            type: Type.Landing,
                                            url: InternalUrl.Registration,
                                        });
                                        // Native: /registration is only for the logged in, and the guard would
                                        // send this straight back here. Log in first, then register.
                                        if (Platform.OS === "web")
                                            router.push(Path.Registration);
                                        else
                                            router.push({
                                                pathname: Path.Login,
                                                params: {
                                                    redirect: Path.Registration,
                                                },
                                            });
                                    }}
                                />
                            </View>
                        </View>
                    ) : null}
                </View>
                {lgAndUp ? (
                    <View className="w-7/12 items-center">
                        <View
                            // v-img sizes by width (natural 1729x1033) capped by .device-preview-md max-height: 412px;
                            // the row's height, and so the next section's top, follows it
                            style={{
                                width: "100%",
                                aspectRatio: 1729 / 1033,
                                maxHeight: 412,
                            }}
                        >
                            <Image
                                testID="landing-top-image-id"
                                source={
                                    require("@/assets/images/landing/landing-top.png") as ImageSourcePropType
                                }
                                accessibilityLabel="Health Gateway Preview"
                                resizeMode="contain"
                                style={{ width: "100%", height: "100%" }}
                            />
                        </View>
                    </View>
                ) : null}
            </View>

            {/* Native: the first-launch tutorial introduces the app instead (stated deviation). */}
            {Platform.OS === "web" ? <LandingSections /> : null}
        </View>
    );
}

/**
 * TEMPORARY (owner, 2026-11-01): the web client's tile order, which LandingPage.js expects on the phone carousel. Once
 * the whole Cypress suite has been validated, remove this and return to the navigation order from `getAccessLinks()`.
 */
const cypressOrder = (tiles: InfoTile[]) => {
    const order = [
        "HealthRecords",
        "DependentRecords",
        "RecordsManagement",
        "HealthServices",
    ];
    return [...tiles].sort(
        (a, b) => order.indexOf(a.type) - order.indexOf(b.type)
    );
};

/** The web client's sections under the sign-in row. */
function LandingSections() {
    const dark = useColorScheme() === "dark";
    const { xs, mdAndUp } = useBreakpoint();
    const accessLinks = cypressOrder(getAccessLinks());
    const healthServices = getHealthServicesLinks();

    return (
        <>
            <View className="mt-4 md:mt-8">
                <AppText
                    variant="h1"
                    className="mb-2 md:mb-4"
                    testID="access-links-header"
                >
                    What you can access
                </AppText>
                <AppText className="mb-4">
                    There are many features that help you view, manage and stay
                    informed about your health.
                </AppText>
                <TileCarousel
                    tiles={accessLinks}
                    cardTestID="access-card"
                    mobileCardTestID="mobile-access-card"
                    columns={4}
                    carousel={xs}
                />
            </View>

            {/* mt-4 mt-md-8 in the web client, where it collapses into the tile grid's larger or equal bottom margin
                (mb-6 mb-md-8); flex layout does not collapse margins, so only the carousel (xs) keeps it */}
            <View
                className={`${xs ? "mt-4" : ""} bg-section-background rounded-md p-6 lg:p-8 flex-row items-start`}
            >
                <View className={mdAndUp ? "w-7/12 pr-8" : "flex-1"}>
                    <AppText variant="h1" className="mb-2">
                        Find your health records
                    </AppText>
                    <AppText className="mb-4">
                        Health Gateway helps bring your records together in one
                        place. It connects to many record sources, but not all.
                    </AppText>
                    <Pressable
                        accessibilityRole="link"
                        // an inline <a> in the web client: only the text takes the press, not the column's width
                        className="self-start"
                        // a real anchor on web, so the keyboard can follow it; the press tracks and opens it itself
                        {...hrefProps(ExternalUrl.YourHealthInformation, {
                            newTab: true,
                        })}
                        onPress={(event) => {
                            event.preventDefault();
                            services.tracking.trackEvent({
                                action: Action.ExternalLink,
                                text: Text.FindYourHealthRecords,
                                origin: Origin.Landing,
                                destination: Destination.SupportGuide,
                                type: Type.Landing,
                                url: TrackedExternalUrl.YourHealthInformation,
                            });
                            void openExternal(
                                ExternalUrl.YourHealthInformation
                            );
                        }}
                    >
                        {/* a bare <a> in the web client: no body-large letter-spacing */}
                        <AppText className="text-link underline tracking-normal">
                            Learn more about other health records in B.C.
                        </AppText>
                    </Pressable>
                </View>
                {mdAndUp ? (
                    <View className="w-5/12 items-center justify-center">
                        {/* v-img max-width="300" contain: the box takes the PNG's 303 by 251 shape; react-native-web's Image would take the PNG's own height */}
                        <View
                            style={{
                                width: "100%",
                                maxWidth: 300,
                                aspectRatio: 303 / 251,
                            }}
                        >
                            <Image
                                source={
                                    dark
                                        ? (require("@/assets/images/landing/finding-records-dark.png") as ImageSourcePropType)
                                        : (require("@/assets/images/landing/finding-records.png") as ImageSourcePropType)
                                }
                                accessibilityLabel="Illustration for finding records"
                                resizeMode="contain"
                                style={{ width: "100%", height: "100%" }}
                            />
                        </View>
                    </View>
                ) : null}
            </View>

            <View className="mt-6 md:mt-8">
                <AppText
                    variant="h1"
                    className="mb-2 md:mb-4"
                    testID="health-services-links-header"
                >
                    Explore other trusted B.C. health services
                </AppText>
                <TileCarousel
                    tiles={healthServices}
                    cardTestID="managed-health-card"
                    mobileCardTestID="mobile-health-services-card"
                    columns={3}
                    carousel={xs}
                />
            </View>
        </>
    );
}
