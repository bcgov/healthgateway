import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Dimensions, Linking, Platform } from "react-native";

import { APP_STORE_URL, PLAY_STORE_URL } from "@/config/storeListings";
import { EntryType } from "@/constants/entryType";
import { Action, Destination, Origin, Text, Type } from "@/constants/tracking";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
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
    updateStores,
} from "@/test/fixtures";
import {
    attrOf,
    idOf,
    isWithin,
    press,
    render,
    screen,
    textOf,
    unmount,
    waitFor,
} from "@/test/ui";
import { HomeView } from "@/views/private/home/HomeView";

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

const healthRecordsDescription =
    "View your available health records, including dispensed medications, health visits, lab results, immunizations, and more.";

function enableHealthConnect() {
    const base = configFixture.webClient.featureToggleConfiguration;
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...base,
                        services: {
                            enabled: true,
                            services: [
                                ...base.services.services,
                                {
                                    name: "HealthConnectRegistry",
                                    enabled: true,
                                },
                            ],
                        },
                    },
                },
            },
        })
    );
}

const titles = () =>
    screen.getAllByTestId("card-button-title").map((t) => textOf(t));

/** Whether the Health Records card is a compact tile: its description describes the button rather than showing. */
function isCompact() {
    const control = screen.getByLabelText("Health Records");
    return Platform.OS === "web"
        ? attrOf(control, "aria-describedby") !== undefined
        : screen.queryByText(healthRecordsDescription) === null;
}

/** The window's width and text scale: React Native's Jest setup reports a fontScale of 2 unless told otherwise. */
function windowAt(width: number, fontScale = 1) {
    jest.spyOn(Dimensions, "get").mockReturnValue({
        width,
        height: Math.max(width, 900),
        scale: 3,
        fontScale,
    });
    setBreakpoint(width);
}

describe("HomeView quick-access grid by width", () => {
    beforeEach(() => {
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

    it("full cards with their descriptions on a wide screen; Health Connect shows its logo, not a title", async () => {
        windowAt(1366);
        signInRegistered();
        enableHealthConnect();
        await render(<HomeView />);
        expect(isCompact()).toBe(false);
        expect(screen.getByText(healthRecordsDescription)).toBeTruthy();
        expect(screen.getByTestId("health-connect-registry-card")).toBeTruthy();
        expect(titles()).not.toContain("Health Connect Registry");
    });

    it("compact tiles on a phone, titled, with the menu kept on removable cards", async () => {
        windowAt(390);
        signInRegistered();
        enableHealthConnect();
        await render(<HomeView />);
        expect(isCompact()).toBe(true);
        expect(titles()).toEqual(
            expect.arrayContaining([
                "Health Records",
                "Immunization Record",
                "Immunizations",
                "Health Connect Registry",
                "Organ Donor Registration",
            ])
        );
        // every card but Health Records can be removed
        expect(screen.getAllByTestId("card-menu-button")).toHaveLength(4);
        expect(screen.getByTestId("add-quick-link-button")).toBeTruthy();
        expect(screen.getByTestId("other-record-sources-card")).toBeTruthy();
        await press(screen.getByTestId("health-records-card"));
        expect(router.push).toHaveBeenCalledWith("/timeline");
    });

    it("full cards on a phone with large text", async () => {
        windowAt(390, 1.5);
        signInRegistered();
        await render(<HomeView />);
        expect(isCompact()).toBe(false);
        expect(screen.getByText(healthRecordsDescription)).toBeTruthy();
    });
});

const describeWeb = Platform.OS === "web" ? describe : describe.skip;

const iPhoneSafari =
    "Mozilla/5.0 (iPhone; CPU iPhone OS 26_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.1 Mobile/15E148 Safari/604.1";
const iPadSafari =
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/26.1 Safari/605.1.15";
const androidChrome =
    "Mozilla/5.0 (Linux; Android 16; Pixel 9) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Mobile Safari/537.36";
const macChrome =
    "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36";

/** The browser's user agent and touch points (jsdom has no `maxTouchPoints`, so it is defined for the test). */
function browserOn(userAgent: string, maxTouchPoints = 0) {
    jest.spyOn(navigator, "userAgent", "get").mockReturnValue(userAgent);
    Object.defineProperty(navigator, "maxTouchPoints", {
        value: maxTouchPoints,
        configurable: true,
    });
}

describeWeb("HomeView Get the App card (web on a phone or tablet)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter("/home");
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        jest.spyOn(Linking, "openURL").mockResolvedValue(undefined);
    });
    afterEach(() => {
        delete (navigator as { maxTouchPoints?: number }).maxTouchPoints;
        resetStores();
        jest.restoreAllMocks();
    });

    it("on an iPhone it opens the App Store and tracks the tile", async () => {
        browserOn(iPhoneSafari, 5);
        signInRegistered();
        await render(<HomeView />);
        const card = screen.getByTestId("get-the-app-card");
        expect(screen.getByText("Get the App")).toBeTruthy();
        expect(
            screen.getByText(
                "Download the Health Gateway app from the App Store."
            )
        ).toBeTruthy();
        expect(
            document.querySelector(
                '[data-testid="get-the-app-card"] [data-icon="apple"]'
            )
        ).not.toBeNull();
        await press(card);
        expect(Linking.openURL).toHaveBeenCalledWith(APP_STORE_URL);
        expect(services.tracking.trackEvent).toHaveBeenCalledWith({
            action: Action.ExternalLink,
            text: Text.GetTheApp,
            origin: Origin.Home,
            destination: Destination.AppStore,
            type: Type.HomeTile,
            url: APP_STORE_URL,
        });
    });

    it("on an iPad asking for the desktop site it still opens the App Store", async () => {
        browserOn(iPadSafari, 5);
        signInRegistered();
        await render(<HomeView />);
        await press(screen.getByTestId("get-the-app-card"));
        expect(Linking.openURL).toHaveBeenCalledWith(APP_STORE_URL);
    });

    it("on Android it opens Google Play and tracks the tile", async () => {
        browserOn(androidChrome, 5);
        signInRegistered();
        await render(<HomeView />);
        expect(
            screen.getByText(
                "Download the Health Gateway app from Google Play."
            )
        ).toBeTruthy();
        expect(
            document.querySelector(
                '[data-testid="get-the-app-card"] [data-icon="google-play"]'
            )
        ).not.toBeNull();
        await press(screen.getByTestId("get-the-app-card"));
        expect(Linking.openURL).toHaveBeenCalledWith(PLAY_STORE_URL);
        expect(services.tracking.trackEvent).toHaveBeenCalledWith(
            expect.objectContaining({
                text: Text.GetTheApp,
                destination: Destination.GooglePlay,
                url: PLAY_STORE_URL,
            })
        );
    });

    it("cannot be removed", async () => {
        browserOn(androidChrome, 5);
        signInRegistered();
        await render(<HomeView />);
        const card = screen.getByTestId("get-the-app-card");
        expect(
            screen
                .getAllByTestId("card-menu-button")
                .some((menu) => isWithin(menu, card))
        ).toBe(false);
    });

    it("comes first, ahead of Health Records", async () => {
        browserOn(iPhoneSafari, 5);
        signInRegistered();
        await render(<HomeView />);
        expect(titles().slice(0, 2)).toEqual(["Get the App", "Health Records"]);
    });

    it("not when the showGetTheAppLink feature toggle is off", async () => {
        browserOn(iPhoneSafari, 5);
        signInRegistered();
        const base = configFixture.webClient.featureToggleConfiguration;
        updateStores(() =>
            useConfigStore.setState({
                config: {
                    ...configFixture,
                    webClient: {
                        ...configFixture.webClient,
                        featureToggleConfiguration: {
                            ...base,
                            homepage: {
                                ...base.homepage,
                                showGetTheAppLink: false,
                            },
                        },
                    },
                },
            })
        );
        await render(<HomeView />);
        expect(screen.queryByTestId("get-the-app-card")).toBeNull();
        expect(titles()[0]).toBe("Health Records");
    });

    it("not on a desktop browser", async () => {
        browserOn(macChrome, 0);
        signInRegistered();
        await render(<HomeView />);
        expect(screen.queryByTestId("get-the-app-card")).toBeNull();
    });
});
