import { DateTime } from "luxon";
import { Platform } from "react-native";

import { AppText, HgButton, Icon, LiveRegion } from "@/components/common";
import { useThemeColors } from "@/hooks/useTheme";
import { useAuthStore } from "@/stores/auth.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { beginRelogin } from "@/stores/localSession";

/** Native (stated deviation: web needs a connection to show anything at all): why the records may not be current. */
export function SessionBanner() {
    const colors = useThemeColors();
    const isOnline = useConnectivityStore((s) => s.isOnline);
    const serviceUnavailable = useConnectivityStore(
        (s) => s.serviceUnavailable
    );
    const sessionStatus = useConnectivityStore((s) => s.sessionStatus);
    const savedAt = useConnectivityStore((s) => s.savedAt);

    if (Platform.OS === "web") return null;

    const saved = savedAt
        ? ` Showing records saved ${DateTime.fromMillis(savedAt).toFormat("yyyy-MMM-dd, h:mm a")}.`
        : "";

    if (!isOnline) {
        return (
            <LiveRegion
                politeness="assertive"
                testID="offlineBanner"
                className="flex-row items-center bg-section-background border-b border-border-divider px-4 py-2"
            >
                <Icon name="wifi" size={14} color={colors["bc-grey"]} />
                <AppText variant="body-medium" className="flex-1 ml-3">
                    {`You are offline.${saved} Changes can't be saved until you reconnect.`}
                </AppText>
            </LiveRegion>
        );
    }

    if (serviceUnavailable || sessionStatus === "offline") {
        return (
            <LiveRegion
                politeness="assertive"
                testID="serviceUnavailableBanner"
                className="flex-row items-center bg-section-background border-b border-border-divider px-4 py-2"
            >
                <Icon
                    name="triangle-exclamation"
                    size={14}
                    color={colors["bc-grey"]}
                />
                <AppText variant="body-medium" className="flex-1 ml-3">
                    {`Health Gateway is currently unavailable.${saved} Changes can't be saved until it's back.`}
                </AppText>
            </LiveRegion>
        );
    }

    if (sessionStatus === "expired") {
        return (
            <LiveRegion
                politeness="assertive"
                testID="sessionExpiredBanner"
                className="flex-row items-center bg-section-background border-b border-border-divider pl-4 pr-1 py-1"
            >
                <Icon name="lock" size={14} color={colors["bc-grey"]} />
                <AppText variant="body-medium" className="flex-1 mx-3">
                    Your login has expired. Log in again to see your latest
                    records.
                </AppText>
                <HgButton
                    testID="sessionExpiredLoginBtn"
                    text="Log in"
                    variant="link"
                    uppercase={false}
                    onPress={() => {
                        // starts the BC Services Card login directly, instead of asking again which one
                        useAuthStore.setState({
                            loginRequest: { idp: "bcsc" },
                        });
                        beginRelogin();
                    }}
                />
            </LiveRegion>
        );
    }

    return null;
}
