import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Alert, type AlertButton, Platform } from "react-native";

import { vault } from "@/services/vault";
import { useAppLockStore } from "@/stores/appLock.store";
import { useEnvironmentStore } from "@/stores/environment.store";
import { switchEnvironment } from "@/stores/switchEnvironment";
import { updateStores } from "@/test/fixtures";
import { isChecked, press, render, screen } from "@/test/ui";
import { getLocalAuthMethod } from "@/utils/localAuth";
import { resetApp } from "@/utils/resetApp";
import { SecurityView } from "@/views/native/security/SecurityView";

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
    deleteItemAsync: jest.fn(async () => undefined),
}));
jest.mock("@/utils/localAuth", () => ({
    getLocalAuthMethod: jest.fn(),
}));
jest.mock("@/utils/resetApp", () => ({
    ...jest.requireActual<typeof import("@/utils/resetApp")>(
        "@/utils/resetApp"
    ),
    resetApp: jest.fn(async () => undefined),
}));
jest.mock("@/stores/switchEnvironment", () => ({
    switchEnvironment: jest.fn(async () => undefined),
}));

const authenticate = jest.mocked(vault.unlock);
const method = jest.mocked(getLocalAuthMethod);
const reset = jest.mocked(resetApp);
const switchTo = jest.mocked(switchEnvironment);

describe("SecurityView", () => {
    beforeEach(() => {
        authenticate.mockReset();
        authenticate.mockResolvedValue("unlocked");
        method.mockResolvedValue("faceId");
        reset.mockClear();
        switchTo.mockClear();
        useAppLockStore.setState({ enabled: false, authenticating: false });
        useEnvironmentStore.setState({ selected: "dev", effective: "dev" });
    });

    afterEach(async () => {
        jest.restoreAllMocks();
        updateStores(() =>
            useAppLockStore.setState({ enabled: false, authenticating: false })
        );
        updateStores(() =>
            useEnvironmentStore.setState({
                selected: undefined,
                effective: "local",
            })
        );
    });

    if (Platform.OS === "web") {
        it("is not on web, which has no device security", async () => {
            await render(<SecurityView />);
            expect(screen.queryByTestId("deviceSecuritySwitch")).toBeNull();
        });
        return;
    }

    const open = async () => {
        await render(<SecurityView />);
    };
    const toggle = async (value: boolean) => {
        const { act, fireEvent } =
            await import("@testing-library/react-native");
        await act(async () => {
            await fireEvent(
                screen.getByTestId("deviceSecuritySwitch"),
                "valueChange",
                value
            );
        });
    };
    /** Spies on the alert and returns a way to press one of its buttons. */
    const spyAlert = () => {
        const alert = jest.spyOn(Alert, "alert").mockReturnValue();
        return {
            alert,
            choose: async (text: string) => {
                const buttons = alert.mock.calls[0][2] as AlertButton[];
                const { act } = await import("@testing-library/react-native");
                await act(async () => {
                    buttons.find((b) => b.text === text)?.onPress?.();
                });
            },
        };
    };

    it("has Device Security, Cache, Environment and Reset App", async () => {
        await open();
        expect(screen.getByTestId("deviceSecuritySwitch")).toBeTruthy();
        expect(screen.getByTestId("cacheUnavailable")).toBeTruthy();
        expect(screen.getByTestId("environmentSection")).toBeTruthy();
        expect(screen.getByTestId("resetAppBtn")).toBeTruthy();
    });

    describe("Environment", () => {
        const checked = (id: string) =>
            isChecked(screen.getByTestId(`environment-${id}`));

        it("lists each environment with its host, the current one checked", async () => {
            await open();
            // Jest runs as a development build, so Local is listed too
            for (const id of ["local", "dev", "test", "prod"])
                expect(screen.getByTestId(`environment-${id}`)).toBeTruthy();
            expect(
                screen.getByText("test.healthgateway.gov.bc.ca")
            ).toBeTruthy();
            expect(checked("dev")).toBe(true);
            expect(checked("test")).toBe(false);
        });

        it("asks before switching, and Cancel stays put", async () => {
            const { alert, choose } = spyAlert();
            await open();
            await press(screen.getByTestId("environment-test"));
            expect(alert.mock.calls[0][0]).toBe("Switch to Test?");
            await choose("Cancel");
            expect(switchTo).not.toHaveBeenCalled();
        });

        it("switches once confirmed, as the selector did", async () => {
            const { choose } = spyAlert();
            await open();
            await press(screen.getByTestId("environment-prod"));
            await choose("Switch");
            expect(switchTo).toHaveBeenCalledTimes(1);
            expect(switchTo.mock.calls[0][0]).toBe("prod");
        });

        describe("only what this build offers (environmentSelector.md)", () => {
            afterEach(() => {
                updateStores(() =>
                    useEnvironmentStore.setState({
                        offered: ["local", "dev", "test", "prod"],
                    })
                );
            });

            it("a released build (Production only): no section", async () => {
                useEnvironmentStore.setState({
                    effective: "prod",
                    offered: ["prod"],
                });
                await open();
                expect(screen.queryByTestId("environmentSection")).toBeNull();
                expect(screen.getByTestId("resetAppBtn")).toBeTruthy();
            });

            it("an unreleased build: Development, Test and Production", async () => {
                useEnvironmentStore.setState({
                    effective: "test",
                    offered: ["dev", "test", "prod"],
                });
                await open();
                expect(screen.queryByTestId("environment-local")).toBeNull();
                for (const id of ["dev", "test", "prod"])
                    expect(
                        screen.getByTestId(`environment-${id}`)
                    ).toBeTruthy();
                expect(checked("test")).toBe(true);
            });
        });

        it("does nothing on the current environment", async () => {
            const { alert } = spyAlert();
            await open();
            await press(screen.getByTestId("environment-dev"));
            expect(alert).not.toHaveBeenCalled();
            expect(switchTo).not.toHaveBeenCalled();
        });
    });

    it("turns on through the prompt, without a warning", async () => {
        const { alert } = spyAlert();
        await open();
        await toggle(true);
        expect(alert).not.toHaveBeenCalled();
        expect(authenticate).toHaveBeenCalledTimes(1);
        expect(useAppLockStore.getState().enabled).toBe(true);
    });

    it("warns about saved data before turning off, and Cancel keeps it on", async () => {
        useAppLockStore.setState({ enabled: true });
        const { alert, choose } = spyAlert();
        await open();
        await toggle(false);
        expect(alert.mock.calls[0][1]).toBe(
            "Any locally saved data will be removed. Are you sure you want to continue?"
        );
        await choose("Cancel");
        expect(authenticate).not.toHaveBeenCalled();
        expect(useAppLockStore.getState().enabled).toBe(true);
    });

    it("turns off once confirmed and the prompt passes", async () => {
        useAppLockStore.setState({ enabled: true });
        const { choose } = spyAlert();
        await open();
        await toggle(false);
        await choose("Turn Off");
        expect(authenticate).toHaveBeenCalledTimes(1);
        expect(useAppLockStore.getState().enabled).toBe(false);
    });

    it("stays on when confirmed but the prompt fails", async () => {
        useAppLockStore.setState({ enabled: true });
        authenticate.mockResolvedValue("cancelled");
        const { choose } = spyAlert();
        await open();
        await toggle(false);
        await choose("Turn Off");
        expect(useAppLockStore.getState().enabled).toBe(true);
    });

    it("cannot be turned on with no device security", async () => {
        method.mockResolvedValue("none");
        await open();
        expect(
            screen.getByText(
                "Turn on a passcode or biometrics in your device settings to use this."
            )
        ).toBeTruthy();
    });

    it("confirms Reset App, and Cancel does nothing", async () => {
        const { alert, choose } = spyAlert();
        await open();
        await press(screen.getByTestId("resetAppBtn"));
        expect(alert.mock.calls[0][0]).toBe("Reset App?");
        await choose("Cancel");
        expect(reset).not.toHaveBeenCalled();
    });

    it("Reset resets once confirmed", async () => {
        const { choose } = spyAlert();
        await open();
        await press(screen.getByTestId("resetAppBtn"));
        await choose("Reset");
        expect(reset).toHaveBeenCalledTimes(1);
    });
});
