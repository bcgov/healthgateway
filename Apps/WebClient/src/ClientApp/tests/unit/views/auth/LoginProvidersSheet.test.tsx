import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Alert, Platform } from "react-native";

import { Path } from "@/constants/path";
import type { IdentityProviderConfiguration } from "@/models/configData";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import {
    idOf,
    inAct,
    isDisabled,
    press,
    render,
    screen,
    settle,
} from "@/test/ui";
import { LoginProvidersSheet } from "@/views/auth/LoginProvidersSheet";
import { LOGIN_FAILED_MESSAGE } from "@/views/auth/useProviderSignIn";

jest.mock("@/views/auth/completeLogin", () => ({
    completeLogin: jest.fn(async () => "/home"),
}));

const bcsc =
    configFixture.webClient.mobile!.authentication!.identityProviders![0];
const keycloak: IdentityProviderConfiguration = {
    id: "KeyCloak",
    name: "KeyCloak",
    icon: "user-secret",
    hint: "",
    disabled: false,
};
const idir: IdentityProviderConfiguration = {
    id: "IDIR",
    name: "BC Government IDIR",
    icon: "user",
    hint: "idir",
    disabled: true,
};

const withMobileProviders = (
    identityProviders: IdentityProviderConfiguration[]
) =>
    useConfigStore.setState({
        status: "loaded",
        config: {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                mobile: {
                    ...configFixture.webClient.mobile,
                    authentication: {
                        ...configFixture.webClient.mobile!.authentication,
                        identityProviders,
                    },
                },
            },
        },
    });

type AlertButton = { text: string; onPress: () => void };
const alertButtons = (): AlertButton[] => {
    const alert = jest.mocked(Alert.alert);
    expect(alert).toHaveBeenCalledTimes(1);
    expect(alert.mock.calls[0][0]).toBe("Unable to log in");
    expect(alert.mock.calls[0][1]).toBe(LOGIN_FAILED_MESSAGE);
    return (alert.mock.calls[0][2] ?? []) as AlertButton[];
};
const tap = (text: string) =>
    alertButtons()
        .find((b) => b.text === text)!
        .onPress();

// the sheet is native only: web keeps the login page's own list
const describeNative = Platform.OS === "web" ? describe.skip : describe;

describeNative("LoginProvidersSheet", () => {
    let auth: ReturnType<typeof fakeAuth>;
    beforeEach(() => {
        auth = fakeAuth();
        services.initialize(configFixture, auth);
        resetRouter(Path.LoginProviders);
        jest.mocked(router.replace).mockClear();
        jest.mocked(router.back).mockClear();
        jest.spyOn(Alert, "alert").mockImplementation(() => undefined);
        withMobileProviders([bcsc, keycloak, idir]);
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("lists the enabled mobile providers with the login page's button ids", async () => {
        await render(<LoginProvidersSheet />);
        expect(idOf(screen.getByTestId("bcscBtn"))).toBe("bcscBtn");
        expect(idOf(screen.getByTestId("KeyCloakBtn"))).toBe("KeyCloakBtn");
        expect(screen.queryByTestId("IDIRBtn")).toBeNull();
        expect(screen.getByText("or")).toBeTruthy();
        expect(auth.signIn).not.toHaveBeenCalled();
    });

    it("logs in with the chosen provider's hint, then goes where the login leads", async () => {
        resetRouter(Path.LoginProviders, { redirect: Path.Registration });
        jest.mocked(router.replace).mockClear();
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("bcscBtn"));
        await settle();
        expect(auth.signIn).toHaveBeenCalledWith(Path.Registration, bcsc.hint);
        expect(router.replace).toHaveBeenCalledWith("/home");
        expect(Alert.alert).not.toHaveBeenCalled();
    });

    it("a provider with no hint logs in without one", async () => {
        jest.mocked(auth.signIn).mockReturnValue(new Promise(() => undefined));
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("KeyCloakBtn"));
        expect(auth.signIn).toHaveBeenCalledWith(Path.Home, undefined);
    });

    it("while a login is under way, no other can start", async () => {
        jest.mocked(auth.signIn).mockReturnValue(new Promise(() => undefined));
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("bcscBtn"));
        expect(isDisabled(screen.getByTestId("KeyCloakBtn"))).toBe(true);
    });

    it("failed: the platform alert, and Try again starts the same provider", async () => {
        jest.mocked(auth.signIn)
            .mockRejectedValueOnce(new Error("boom"))
            .mockReturnValue(new Promise(() => undefined));
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("KeyCloakBtn"));
        await settle();
        await inAct(() => tap("Try again"));
        expect(auth.signIn).toHaveBeenCalledTimes(2);
        expect(auth.signIn).toHaveBeenLastCalledWith(Path.Home, undefined);
    });

    it("failed, Cancel: stays on the sheet to choose again", async () => {
        jest.mocked(auth.signIn).mockRejectedValue(new Error("boom"));
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("bcscBtn"));
        await settle();
        await inAct(() => tap("Cancel"));
        expect(router.replace).not.toHaveBeenCalled();
        expect(router.back).not.toHaveBeenCalled();
        expect(isDisabled(screen.getByTestId("KeyCloakBtn"))).toBe(false);
    });

    it("cancelled: no alert, and the sheet stays to choose again", async () => {
        const cancelled = new Error("cancelled");
        cancelled.name = "AuthCancelledError";
        jest.mocked(auth.signIn).mockRejectedValue(cancelled);
        await render(<LoginProvidersSheet />);
        await press(screen.getByTestId("bcscBtn"));
        await settle();
        expect(Alert.alert).not.toHaveBeenCalled();
        expect(router.replace).not.toHaveBeenCalled();
        expect(isDisabled(screen.getByTestId("bcscBtn"))).toBe(false);
    });

    it("the close button closes the sheet", async () => {
        await render(<LoginProvidersSheet />);
        await press(screen.getByLabelText("Close"));
        expect(router.back).toHaveBeenCalled();
    });
});
