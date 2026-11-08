import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Linking, Platform } from "react-native";

import { ExternalUrl } from "@/constants/externalUrls";
import { Action, Destination, Text } from "@/constants/tracking";
import type { UserProfile } from "@/models/userProfile";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useUserStore } from "@/stores/user.store";
import { setBreakpoint } from "@/test/breakpointMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    isDisabled,
    isOn,
    press,
    render,
    screen,
    toggleSwitch,
    waitFor,
} from "@/test/ui";
import { NotificationsSection } from "@/views/profile/NotificationsSection";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const EMAIL = "BC Cancer Screening Letters email notifications";
const SMS = "BC Cancer Screening Letters text (SMS) notifications";

function withNotifications(
    enabled = true,
    preferences = { email: true, sms: true }
) {
    const t = configFixture.webClient.featureToggleConfiguration;
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...t,
                        profile: {
                            notifications: {
                                enabled,
                                type: [
                                    {
                                        name: "bcCancerScreening",
                                        enabled: true,
                                        preferences,
                                    },
                                ],
                            },
                        },
                    },
                },
            },
        })
    );
}

/** Signed in with: email verified, SMS present but unverified, nothing saved — unless overridden. */
function user(profile: Partial<UserProfile> = {}) {
    signInRegistered(profile);
    withNotifications();
}

describe("NotificationsSection (UserProfileNotificationsComponent.vue port)", () => {
    let track: jest.SpiedFunction<typeof services.tracking.trackEvent>;
    beforeEach(() => {
        setBreakpoint(1366);
        track = jest
            .spyOn(services.tracking, "trackEvent")
            .mockImplementation(() => undefined);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("renders nothing when the feature is off", async () => {
        signInRegistered();
        withNotifications(false);
        await render(<NotificationsSection />);
        expect(
            screen.queryByTestId("profile-notification-preferences-label")
        ).toBeNull();
    });

    it("shows the web client's label, headers, ids and LEARN MORE button", async () => {
        user();
        await render(<NotificationsSection />);
        expect(screen.getByText("Notifications")).toBeTruthy();
        expect(
            screen.getByTestId("profile-notification-preferences-header")
        ).toBeTruthy();
        expect(screen.getByText("Notification Type")).toBeTruthy();
        expect(screen.getByText("BC Cancer Screening Letters")).toBeTruthy();
        expect(
            screen.getByTestId(
                "profile-notification-preferences-bc-cancer-screening-label-value"
            )
        ).toBeTruthy();
        expect(
            screen.getByTestId(
                "profile-notification-preferences-bc-cancer-screening-email-value"
            )
        ).toBeTruthy();
        expect(
            screen.getByTestId(
                "profile-notification-preferences-bc-cancer-screening-sms-value"
            )
        ).toBeTruthy();
        expect(screen.getByText("LEARN MORE")).toBeTruthy();
    });

    it("disables a channel whose contact is missing or unverified and asks to verify", async () => {
        user(); // SMS unverified
        await render(<NotificationsSection />);
        expect(isDisabled(screen.getByLabelText(EMAIL))).toBe(false);
        expect(isDisabled(screen.getByLabelText(SMS))).toBe(true);
        expect(
            screen.getByTestId(
                "profile-notification-preferences-verification-message"
            )
        ).toBeTruthy();
    });

    it("shows saved preferences, but never a channel the user cannot use", async () => {
        user({
            isSMSNumberVerified: false,
            notificationSettings: [
                {
                    type: "BcCancerScreening",
                    emailEnabled: true,
                    smsEnabled: true,
                },
            ],
        });
        await render(<NotificationsSection />);
        expect(isOn(screen.getByLabelText(EMAIL))).toBe(true);
        expect(isOn(screen.getByLabelText(SMS))).toBe(false);
    });

    it("saves an unusable channel as off and an untoggled one as null (web client payload rule)", async () => {
        // email changed and unverified, but the saved setting still has email on
        user({
            isEmailVerified: false,
            isSMSNumberVerified: true,
            notificationSettings: [
                {
                    type: "BcCancerScreening",
                    emailEnabled: true,
                    smsEnabled: false,
                },
            ],
        });
        const update = jest.fn(async (_s: unknown) => undefined);
        updateStores(() =>
            useUserStore.setState({
                updateNotificationSettings: update,
            })
        );
        await render(<NotificationsSection />);
        await toggleSwitch(screen.getByLabelText(SMS));
        expect(update).toHaveBeenCalledWith({
            type: "BcCancerScreening",
            emailEnabled: false,
            smsEnabled: true,
        });
        expect(track).toHaveBeenCalledWith(
            expect.objectContaining({ text: Text.ScreeningSmsToggledOn })
        );

        withNotifications(true, { email: false, sms: true });
        await render(<NotificationsSection />);
        await toggleSwitch(screen.getByLabelText(SMS));
        expect(update).toHaveBeenLastCalledWith({
            type: "BcCancerScreening",
            emailEnabled: null,
            smsEnabled: true,
        });
    });

    it("flips immediately, locks the row while saving, and rolls back with a message if the save fails", async () => {
        user();
        let fail: (e: Error) => void = () => undefined;
        const update = jest.fn(
            () => new Promise<void>((_ok, reject) => (fail = reject))
        );
        updateStores(() =>
            useUserStore.setState({
                updateNotificationSettings: update,
            })
        );
        await render(<NotificationsSection />);
        await toggleSwitch(screen.getByLabelText(EMAIL));
        expect(isOn(screen.getByLabelText(EMAIL))).toBe(true);
        expect(isDisabled(screen.getByLabelText(EMAIL))).toBe(true);
        fail(new Error("down"));
        await waitFor(() =>
            expect(
                screen.getByTestId(
                    "profile-notification-preferences-save-error"
                )
            ).toBeTruthy()
        );
        expect(isOn(screen.getByLabelText(EMAIL))).toBe(false);
        expect(isDisabled(screen.getByLabelText(EMAIL))).toBe(false);
        expect(
            screen.getByText(
                "An unexpected error has occurred. Please try again later."
            )
        ).toBeTruthy();
    });

    it("on phones hides the header row and labels each switch instead", async () => {
        setBreakpoint(390);
        user();
        await render(<NotificationsSection />);
        expect(
            screen.queryByTestId("profile-notification-preferences-header")
        ).toBeNull();
        expect(screen.getByText("Email")).toBeTruthy();
        expect(screen.getByText("Text (SMS)")).toBeTruthy();
    });

    it("LEARN MORE opens the information dialog with the web client's copy, link and OK", async () => {
        user();
        const open = jest
            .spyOn(Linking, "openURL")
            .mockResolvedValue(undefined);
        await render(<NotificationsSection />);
        await press(
            screen.getByTestId(
                "profile-notification-preferences-bc-cancer-screening-learn-more"
            )
        );
        expect(track).toHaveBeenCalledWith(
            expect.objectContaining({
                text: Text.ScreeningNotificationsLearnMore,
            })
        );
        expect(screen.getByTestId("information-modal")).toBeTruthy();
        expect(
            screen.getByText("BC Cancer Screening Program Letters")
        ).toBeTruthy();
        expect(
            screen.getByText(
                /Cancer screening program letters are available on Health Gateway\./
            )
        ).toBeTruthy();
        expect(
            screen.getByText(
                /will not change the paper letters that will be mailed to you\./
            )
        ).toBeTruthy();
        const link = screen.getByTestId("information-modal-link-2-1");
        if (Platform.OS === "web") {
            expect((link as unknown as HTMLElement).getAttribute("href")).toBe(
                ExternalUrl.ScreeningContact
            );
            expect(
                (link as unknown as HTMLElement).getAttribute("target")
            ).toBe("_blank");
        }
        await press(link);
        expect(track).toHaveBeenCalledWith(
            expect.objectContaining({
                action: Action.ExternalLink,
                text: Text.ScreeningNotificationsContactLink,
                destination: Destination.ScreeningContact,
                url: ExternalUrl.ScreeningContact,
            })
        );
        if (Platform.OS !== "web")
            expect(open).toHaveBeenCalledWith(ExternalUrl.ScreeningContact);
        await press(screen.getByTestId("generic-message-ok-btn"));
        await waitFor(() =>
            expect(screen.queryByTestId("information-modal")).toBeNull()
        );
    });

    it("the dialog's close button dismisses it too", async () => {
        user();
        await render(<NotificationsSection />);
        await press(
            screen.getByTestId(
                "profile-notification-preferences-bc-cancer-screening-learn-more"
            )
        );
        await press(screen.getByTestId("message-modal-close-button"));
        await waitFor(() =>
            expect(screen.queryByTestId("information-modal")).toBeNull()
        );
    });
});
