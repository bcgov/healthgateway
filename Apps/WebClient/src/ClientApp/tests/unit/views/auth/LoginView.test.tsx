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
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { idOf, inAct, render, screen, textOf, waitFor } from "@/test/ui";
import { LoginView } from "@/views/auth/LoginView";
import { LOGIN_FAILED_MESSAGE } from "@/views/auth/useProviderSignIn";

const idir = {
    id: "idir",
    name: "IDIR",
    icon: "user-secret",
    hint: "idir",
    disabled: false,
};
/** The list this platform reads: native's is under webClient.mobile.authentication, web's at the top level. */
const withProviders = (providers: typeof configFixture.identityProviders) => {
    const mobile = configFixture.webClient.mobile!;
    useConfigStore.setState({
        config:
            Platform.OS === "web"
                ? { ...configFixture, identityProviders: providers }
                : {
                      ...configFixture,
                      webClient: {
                          ...configFixture.webClient,
                          mobile: {
                              ...mobile,
                              authentication: {
                                  ...mobile.authentication,
                                  identityProviders: providers,
                              },
                          },
                      },
                  },
        status: "loaded",
    });
};

describe("LoginView provider rules (LoginView.vue port)", () => {
    let auth: ReturnType<typeof fakeAuth>;
    beforeEach(() => {
        auth = fakeAuth();
        // signIn never settles, like the web redirect; the view stays on the loading state
        (auth.signIn as jest.Mock).mockImplementation(
            () => new Promise(() => undefined)
        );
        services.initialize(configFixture, auth);
        resetRouter("/login");
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("one provider: starts it straight away, with no page of ours", async () => {
        withProviders([configFixture.identityProviders[0]]);
        jest.mocked(auth.signIn).mockReturnValue(new Promise(() => undefined));
        await render(<LoginView />);
        expect(auth.signIn).toHaveBeenCalledTimes(1);
        // the provider's hint from the configuration, as `kc_idp_hint`
        expect(auth.signIn).toHaveBeenCalledWith(
            "/home",
            configFixture.identityProviders[0].hint
        );
        expect(textOf(screen.getByTestId("loginPicker"))).toContain(
            `Redirecting to ${configFixture.identityProviders[0].name}...`
        );
        expect(screen.queryByTestId("bcscBtn")).toBeNull();
    });

    if (Platform.OS !== "web") {
        it("one provider, cancelled: back to the landing page", async () => {
            withProviders([configFixture.identityProviders[0]]);
            const cancelled = new Error("cancelled");
            cancelled.name = "AuthCancelledError";
            jest.mocked(router.replace).mockClear();
            jest.mocked(auth.signIn).mockRejectedValue(cancelled);
            await render(<LoginView />);
            expect(router.replace).toHaveBeenCalledWith(Path.Root);
        });

        const failedAlert = () => {
            const alert = jest.mocked(Alert.alert);
            expect(alert).toHaveBeenCalledTimes(1);
            const [title, message, buttons] = alert.mock.calls[0];
            expect(title).toBe("Unable to log in");
            expect(message).toBe(LOGIN_FAILED_MESSAGE);
            return (buttons ?? []) as { text: string; onPress: () => void }[];
        };
        const button = (
            buttons: { text: string; onPress: () => void }[],
            text: string
        ) => buttons.find((b) => b.text === text)!;

        it("one provider, failed: the platform alert, not an error on the page", async () => {
            jest.spyOn(Alert, "alert").mockImplementation(() => undefined);
            withProviders([configFixture.identityProviders[0]]);
            jest.mocked(router.replace).mockClear();
            jest.mocked(auth.signIn).mockRejectedValue(new Error("boom"));
            await render(<LoginView />);
            failedAlert();
            expect(screen.queryByText(LOGIN_FAILED_MESSAGE)).toBeNull();
            expect(router.replace).not.toHaveBeenCalled();
        });

        it("one provider, failed, Try again: starts the same provider again", async () => {
            jest.spyOn(Alert, "alert").mockImplementation(() => undefined);
            withProviders([configFixture.identityProviders[0]]);
            jest.mocked(auth.signIn)
                .mockRejectedValueOnce(new Error("boom"))
                .mockReturnValue(new Promise(() => undefined));
            await render(<LoginView />);
            await inAct(() => button(failedAlert(), "Try again").onPress());
            expect(auth.signIn).toHaveBeenCalledTimes(2);
            expect(auth.signIn).toHaveBeenLastCalledWith(
                "/home",
                configFixture.identityProviders[0].hint
            );
        });

        it("one provider, failed, Cancel: back to the landing page", async () => {
            jest.spyOn(Alert, "alert").mockImplementation(() => undefined);
            withProviders([configFixture.identityProviders[0]]);
            jest.mocked(router.replace).mockClear();
            jest.mocked(auth.signIn).mockRejectedValue(new Error("boom"));
            await render(<LoginView />);
            await inAct(() => button(failedAlert(), "Cancel").onPress());
            expect(router.replace).toHaveBeenCalledWith(Path.Root);
        });
    }

    it("uses this platform's list: native the mobile one, web the web client's", async () => {
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        jest.mocked(auth.signIn).mockReturnValue(new Promise(() => undefined));
        await render(<LoginView />);
        const mobileHint =
            configFixture.webClient.mobile!.authentication!
                .identityProviders![0].hint;
        expect(mobileHint).not.toBe(configFixture.identityProviders[0].hint);
        expect(auth.signIn).toHaveBeenCalledWith(
            "/home",
            Platform.OS === "web"
                ? configFixture.identityProviders[0].hint
                : mobileHint
        );
    });

    it("several providers: lists them all and starts none", async () => {
        withProviders([...configFixture.identityProviders, idir]);
        await render(<LoginView />);
        expect(screen.getByTestId("bcscBtn")).toBeTruthy();
        expect(screen.getByTestId("idirBtn")).toBeTruthy();
        expect(screen.getByText("or")).toBeTruthy();
        expect(auth.signIn).not.toHaveBeenCalled();
    });

    it("retry: shows the error and the list, without redirecting", async () => {
        withProviders(configFixture.identityProviders);
        resetRouter("/login", { isRetry: "true" });
        await render(<LoginView />);
        expect(screen.getByText(/unexpected error occurred/)).toBeTruthy();
        expect(screen.getByTestId("bcscBtn")).toBeTruthy();
        expect(auth.signIn).not.toHaveBeenCalled();
    });

    it("an idp deep link starts that provider directly", async () => {
        withProviders([...configFixture.identityProviders, idir]);
        resetRouter("/login", { idp: "IDIR", redirect: "/profile" });
        await render(<LoginView />);
        await waitFor(() =>
            expect(auth.signIn).toHaveBeenCalledWith("/profile", "idir")
        );
    });

    it("hides disabled providers and explains when none are left", async () => {
        withProviders([{ ...idir, disabled: true }]);
        await render(<LoginView />);
        expect(screen.getByText("No login providers configured")).toBeTruthy();
    });

    it("each provider button carries the HTML id the Cypress login helpers click (`#<id>Btn`)", async () => {
        withProviders([...configFixture.identityProviders, idir]);
        await render(<LoginView />);
        expect(idOf(screen.getByTestId("bcscBtn"))).toBe("bcscBtn");
        expect(idOf(screen.getByTestId("idirBtn"))).toBe("idirBtn");
    });
});
