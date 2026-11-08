import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { EntryType } from "@/constants/entryType";
import { services } from "@/services/container";
import { useTimelineStore } from "@/stores/timeline.store";
import { useUserStore } from "@/stores/user.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";
import {
    attrOf,
    idOf,
    press,
    render,
    screen,
    unmount,
    waitFor,
} from "@/test/ui";
import { HomeView } from "@/views/home/HomeView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("HomeView (HomeView.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter("/home");
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        jest.spyOn(
            services.immunization,
            "getPatientImmunizations"
        ).mockResolvedValue(immunizationResultFixture);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("title is #subject 'Home' with Add Quick Link beside it", async () => {
        signInRegistered();
        await render(<HomeView />);
        expect(idOf(screen.getByText("Home"))).toBe("subject");
        expect(screen.getByTestId("add-quick-link-button")).toBeTruthy();
    });

    it("unverified contact: the banner links to the profile (profile-page-link, #profilePageLink)", async () => {
        signInRegistered({ isSMSNumberVerified: false });
        await render(<HomeView />);
        expect(screen.getByTestId("incomplete-profile-banner")).toBeTruthy();
        const link = screen.getByTestId("profile-page-link");
        expect(idOf(link)).toBe("profilePageLink");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe("/profile");
        await press(link);
        expect(router.push).toHaveBeenCalledWith("/profile");
    });

    it("SMS removed: profile-preferences-link (#profilePreferencesLink) and the preference is reset", async () => {
        const setPreference = jest.fn(async (_p: unknown) => undefined);
        signInRegistered({
            isSMSNumberVerified: true,
            preferences: {
                showSmsRemoved: {
                    preference: "showSmsRemoved",
                    value: "true",
                    version: 1,
                    createdDateTime: "2026-01-01T00:00:00Z",
                },
            },
        });
        useUserStore.setState({ setUserPreference: setPreference });
        await render(<HomeView />);
        expect(screen.getByTestId("sms-removed-message")).toBeTruthy();
        const link = screen.getByTestId("profile-preferences-link");
        expect(idOf(link)).toBe("profilePreferencesLink");
        if (Platform.OS === "web")
            expect(attrOf(link, "href")).toBe("/profile");
        expect(setPreference).toHaveBeenCalledWith(
            expect.objectContaining({
                preference: "showSmsRemoved",
                value: "false",
            })
        );
    });

    it("BC Cancer banner only when the previous login predates 2026-05-19", async () => {
        signInRegistered({
            lastLoginDateTimes: [
                "2026-10-29T10:00:00Z",
                "2026-01-01T10:00:00Z",
            ],
        });
        await render(<HomeView />);
        expect(
            screen.getByTestId("bc-cancer-notifications-banner")
        ).toBeTruthy();
        expect(idOf(screen.getByTestId("profile-notifications-link"))).toBe(
            "profileNotificationsLink"
        );
        resetStores();
        signInRegistered();
        await render(<HomeView />);
        expect(
            screen.queryByTestId("bc-cancer-notifications-banner")
        ).toBeNull();
    });

    it("the Immunizations card opens the recommendations dialog with its table", async () => {
        signInRegistered();
        await render(<HomeView />);
        expect(screen.queryByTestId("recommendations-dialog")).toBeNull();
        await press(screen.getByTestId("recommendations-card-button"));
        await waitFor(() =>
            expect(screen.getByTestId("recommendations-dialog")).toBeTruthy()
        );
        await waitFor(() =>
            expect(screen.getAllByTestId("recommendationItem")).toHaveLength(2)
        );
        expect(
            services.immunization.getPatientImmunizations
        ).toHaveBeenCalledWith("HDID1");
        // afterEach signs the user out; a mounted report would then reload for the empty hdid
        await unmount();
    });

    it("a quick link sets the timeline's record-type filter and opens the timeline", async () => {
        signInRegistered({
            preferences: {
                quickLinks: {
                    preference: "quickLinks",
                    value: JSON.stringify([
                        {
                            name: "Immunizations",
                            filter: { modules: ["Immunization"] },
                        },
                    ]),
                    version: 1,
                    createdDateTime: "2026-01-01T00:00:00Z",
                },
            },
        });
        await render(<HomeView />);
        await press(screen.getByTestId("quick-link-card"));
        expect([...useTimelineStore.getState().filter.entryTypes]).toEqual([
            EntryType.Immunization,
        ]);
        expect(router.push).toHaveBeenCalledWith("/timeline");
    });
});
