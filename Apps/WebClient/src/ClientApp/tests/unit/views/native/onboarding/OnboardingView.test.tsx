import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Alert, Platform } from "react-native";

import { onboardingScreens } from "@/constants/onboarding";
import { vault } from "@/services/vault";
import { useAppLockStore } from "@/stores/appLock.store";
import { useEnvironmentStore } from "@/stores/environment.store";
import {
    selectOnboardingPending,
    useOnboardingStore,
} from "@/stores/onboarding.store";
import { updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import { getLocalAuthMethod } from "@/utils/localAuth";
import { OnboardingView } from "@/views/native/onboarding/OnboardingView";

jest.mock("@/services/vault", () => ({
    vault: {
        unlock: jest.fn(),
        lock: jest.fn(),
        destroy: jest.fn(async () => undefined),
    },
}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(),
    setItemAsync: jest.fn(async () => undefined),
    deleteItemAsync: jest.fn(),
}));
jest.mock("@/utils/localAuth", () => ({
    getLocalAuthMethod: jest.fn(),
}));

const authenticate = jest.mocked(vault.unlock);
const method = jest.mocked(getLocalAuthMethod);
const onboarding = () => useOnboardingStore.getState();

/** The pager mounts every slide, so assert the button rather than which slide is on screen. */
const nextLabel = () => screen.getByTestId("onboardingNextBtn");

describe("OnboardingView", () => {
    beforeEach(() => {
        authenticate.mockReset();
        authenticate.mockResolvedValue("unlocked");
        method.mockResolvedValue("faceId");
        useAppLockStore.setState({ enabled: false, authenticating: false });
    });

    afterEach(() => {
        jest.restoreAllMocks();
    });

    describe("environment banner", () => {
        afterEach(() => {
            updateStores(() =>
                useEnvironmentStore.setState({
                    selected: undefined,
                    effective: "local",
                })
            );
        });

        it.each([
            ["tutorial", onboardingScreens],
            ["security step", []],
        ])("shows the environment on the %s", async (_name, screens) => {
            if (Platform.OS === "web") return; // the intro is native only
            useOnboardingStore.setState({ screens, securityOffered: false });
            useEnvironmentStore.setState({
                selected: "test",
                effective: "test",
            });
            await render(<OnboardingView />);
            expect(screen.getByTestId("environmentBanner")).toBeTruthy();
            expect(screen.getByText("Test")).toBeTruthy();
        });

        it.each([
            ["tutorial", onboardingScreens],
            ["security step", []],
        ])(
            "has the logo bar and the gear on the %s, like the app's header",
            async (_name, screens) => {
                if (Platform.OS === "web") return;
                useOnboardingStore.setState({
                    screens,
                    securityOffered: false,
                });
                await render(<OnboardingView />);
                expect(screen.getByTestId("brandHeader")).toBeTruthy();
                expect(screen.getByTestId("settingsMenuBtn")).toBeTruthy();
            }
        );

        it("the gear leaves out Security, which the intro's own step sets", async () => {
            if (Platform.OS === "web") return;
            useOnboardingStore.setState({
                screens: onboardingScreens,
                securityOffered: false,
            });
            await render(<OnboardingView />);
            await press(screen.getByTestId("settingsMenuBtn"));
            expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
            expect(screen.queryByTestId("menu-btn-security-link")).toBeNull();
        });

        it("shows none on Production", async () => {
            if (Platform.OS === "web") return;
            useOnboardingStore.setState({
                screens: onboardingScreens,
                securityOffered: false,
            });
            useEnvironmentStore.setState({
                selected: "prod",
                effective: "prod",
            });
            await render(<OnboardingView />);
            expect(screen.queryByTestId("environmentBanner")).toBeNull();
        });
    });

    describe("tutorial", () => {
        beforeEach(() => {
            useOnboardingStore.setState({
                screens: onboardingScreens,
                securityOffered: false,
            });
        });

        it("shows the landing page tiles' text and an icon for every screen", async () => {
            await render(<OnboardingView />);
            for (const { title, description } of onboardingScreens) {
                expect(screen.getByText(title)).toBeTruthy();
                expect(screen.getByText(description)).toBeTruthy();
            }
            expect(screen.getAllByTestId("onboardingBubble")).toHaveLength(
                onboardingScreens.length
            );
            expect(screen.queryByTestId("onboardingNewLabel")).toBeNull();
        });

        it("walks with Next to Get started, then moves to the security step", async () => {
            await render(<OnboardingView />);
            for (let i = 1; i < onboardingScreens.length; i++) {
                expect(screen.getByText("Next")).toBeTruthy();
                expect(screen.getByTestId("onboardingSkipBtn")).toBeTruthy();
                await press(nextLabel());
            }
            expect(screen.getByText("Get started")).toBeTruthy();
            expect(screen.queryByTestId("onboardingSkipBtn")).toBeNull();

            await press(nextLabel());
            expect(onboarding().screens).toEqual([]);
            expect(
                screen.getByText("Protect your personal information")
            ).toBeTruthy();
        });

        it("Skip intro goes straight to the security step", async () => {
            await render(<OnboardingView />);
            await press(screen.getByText("Skip intro"));
            expect(onboarding().screens).toEqual([]);
            expect(
                screen.getByText("Protect your personal information")
            ).toBeTruthy();
        });

        it("shows a single new screen with NEW and OK, without dots or skip", async () => {
            useOnboardingStore.setState({
                screens: [onboardingScreens[3]],
                securityOffered: true,
            });
            await render(<OnboardingView />);
            expect(screen.getByTestId("onboardingNewLabel")).toBeTruthy();
            expect(screen.getByText("OK")).toBeTruthy();
            expect(screen.queryByTestId("onboardingDots")).toBeNull();
            expect(screen.queryByTestId("onboardingSkipBtn")).toBeNull();

            await press(nextLabel());
            expect(selectOnboardingPending(onboarding())).toBe(false);
        });
    });

    describe("security step", () => {
        beforeEach(() => {
            useOnboardingStore.setState({
                screens: [],
                securityOffered: false,
            });
        });

        it.each([
            ["faceId", "Use Face ID"],
            ["touchId", "Use Touch ID"],
            ["biometrics", "Use biometrics"],
            ["passcode", "Use passcode"],
        ] as const)("offers %s as %s", async (value, label) => {
            method.mockResolvedValue(value);
            await render(<OnboardingView />);
            expect(screen.getByText(label)).toBeTruthy();
        });

        it("explains why device security is needed", async () => {
            await render(<OnboardingView />);
            expect(
                screen.getByText(
                    "Device security must be enabled to support a quicker sign-on and to save a copy of your data on this device for when you are disconnected."
                )
            ).toBeTruthy();
        });

        it("turns the lock on when the prompt passes", async () => {
            await render(<OnboardingView />);
            await press(screen.getByTestId("localAuthBtn"));
            expect(useAppLockStore.getState().enabled).toBe(true);
            expect(selectOnboardingPending(onboarding())).toBe(false);
        });

        it("stays on the step when the prompt fails", async () => {
            authenticate.mockResolvedValue("cancelled");
            await render(<OnboardingView />);
            await press(screen.getByTestId("localAuthBtn"));
            expect(useAppLockStore.getState().enabled).toBe(false);
            expect(selectOnboardingPending(onboarding())).toBe(true);
        });

        it("Not now moves on with the lock off", async () => {
            await render(<OnboardingView />);
            await press(screen.getByText("Not now"));
            expect(authenticate).not.toHaveBeenCalled();
            expect(useAppLockStore.getState().enabled).toBe(false);
            expect(selectOnboardingPending(onboarding())).toBe(false);
        });

        it("sends a device with nothing set up to Settings instead of the prompt", async () => {
            method.mockResolvedValue("none");
            const alert = jest.spyOn(Alert, "alert").mockReturnValue();
            await render(<OnboardingView />);
            await press(screen.getByText("Set up authentication"));
            expect(alert).toHaveBeenCalledTimes(1);
            expect(authenticate).not.toHaveBeenCalled();
        });

        it("opens the security tips and comes back", async () => {
            await render(<OnboardingView />);
            await press(screen.getByTestId("localAuthLearnMoreBtn"));
            expect(screen.getByText("Keeping your data secure")).toBeTruthy();
            expect(
                screen.getByText("Do not share or turn off your passcode")
            ).toBeTruthy();

            await press(screen.getByTestId("securityTipsBackBtn"));
            expect(
                screen.getByText("Protect your personal information")
            ).toBeTruthy();
        });
    });
});
