import { Link, useRouter } from "expo-router";
import { Platform, Pressable, View } from "react-native";

import BcLogo from "@/assets/images/gov/bcid-logo-rev-en.svg";
import { AppText, ExternalLink, HgAlert, HgButton } from "@/components/common";
import { webAttributeRef } from "@/components/common/webClass";
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
import { toHref } from "@/navigation/href";
import { services } from "@/services/container";
import { imageProps } from "@/utils/decorative";
import { openExternal } from "@/utils/openExternal";

function trackNavigationClick(
    action: Action,
    text: string,
    destination: string,
    url: string
) {
    services.tracking.trackEvent({
        action,
        text,
        origin: Origin.VPPLoginPage,
        destination,
        type: Type.VPPLogin,
        url,
    });
}

/** `globalThis.location.assign` on web (this window); the browser on native. */
function leaveTo(url: string) {
    if (Platform.OS === "web") globalThis.location.assign(url);
    else void openExternal(url);
}

/** Port of VppLoginView.vue: the Virtual Patient Portal hand-off page. No header, drawer or footer. */
export function VppLoginView() {
    usePageTitle("Open Health Gateway");
    const router = useRouter();
    const { xs } = useBreakpoint();

    function openAccessMyHealth() {
        trackNavigationClick(
            Action.ExternalLink,
            Text.Cancel,
            Destination.AccessMyHealth,
            TrackedExternalUrl.AccessMyHealth
        );
        leaveTo(TrackedExternalUrl.AccessMyHealth);
    }

    function openHgHome() {
        trackNavigationClick(
            Action.InternalLink,
            Text.ContinueToHealthGateway,
            Destination.HealthGateway,
            InternalUrl.Home
        );
        if (Platform.OS === "web")
            globalThis.location.assign(`${globalThis.location.origin}/login`);
        else router.push(Path.Login);
    }

    return (
        <View className="grow bg-table-header">
            <View className="bg-primary">
                {/* the web client's <router-link>: a real link on web, so the keyboard can follow it */}
                <Link
                    href={toHref(Path.Root)}
                    asChild
                    onPress={(event) => {
                        event.preventDefault();
                        router.push(Path.Root);
                    }}
                >
                    <Pressable
                        nativeID="homeLink"
                        accessibilityRole="link"
                        accessibilityLabel="Return to home page"
                        className="m-3 self-start"
                    >
                        <BcLogo
                            width={152}
                            height={40}
                            {...imageProps("Government of British Columbia")}
                        />
                    </Pressable>
                </Link>
            </View>
            <View className="items-center">
                <View
                    className={`bg-surface rounded shadow w-full max-w-[600px] self-center ${
                        xs ? "m-2 py-6 px-4" : "m-4 py-16 px-16"
                    }`}
                >
                    <AppText
                        variant="title-large"
                        accessibilityRole="header"
                        aria-level={2}
                        className="text-center font-bold mb-4"
                    >
                        Do you want to open Health Gateway?
                    </AppText>
                    <AppText className="text-center mb-4">
                        <ExternalLink
                            testID="click-hgw-link"
                            href={TrackedExternalUrl.HealthGateway}
                            sameWindow
                            onPress={() =>
                                trackNavigationClick(
                                    Action.ExternalLink,
                                    Text.LearnMore,
                                    Destination.HealthGateway,
                                    TrackedExternalUrl.HealthGateway
                                )
                            }
                        >
                            Health Gateway
                        </ExternalLink>{" "}
                        may contain more health records and information.
                    </AppText>
                    <AppText className="text-center mb-4">
                        You are currently signed in to AccessMyHealth. If you
                        wish to continue, you will be signed in to Health
                        Gateway in this window.
                    </AppText>
                    <HgAlert
                        type="warning"
                        testID="signout-close-windows-warning"
                        centerContent
                        text="Please remember to sign out and close both windows when you are done."
                    />
                    <View
                        className={`gap-2 justify-center ${xs ? "" : "flex-row"}`}
                    >
                        <HgButton
                            ref={webAttributeRef(
                                "data-url",
                                TrackedExternalUrl.AccessMyHealth
                            )}
                            testID="cancel-button"
                            variant="secondary"
                            text="Cancel"
                            uppercase={false}
                            labelClassName="text-body-large"
                            block={xs}
                            onPress={openAccessMyHealth}
                        />
                        <HgButton
                            testID="continue-to-hgw-button"
                            text="Continue to Health Gateway"
                            uppercase={false}
                            labelClassName="text-body-large"
                            block={xs}
                            onPress={openHgHome}
                        />
                    </View>
                </View>
            </View>
        </View>
    );
}
