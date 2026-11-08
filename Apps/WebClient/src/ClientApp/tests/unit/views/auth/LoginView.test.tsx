import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { idOf, render, screen, textOf, waitFor } from "@/test/ui";
import { LoginView } from "@/views/auth/LoginView";

const idir = {
    id: "idir",
    name: "IDIR",
    icon: "user-secret",
    hint: "idir",
    disabled: false,
};
const withProviders = (providers: typeof configFixture.identityProviders) =>
    useConfigStore.setState({
        config: { ...configFixture, identityProviders: providers },
        status: "loaded",
    });

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

        it("one provider, failed: stays to show the error and offer it again", async () => {
            withProviders([configFixture.identityProviders[0]]);
            jest.mocked(router.replace).mockClear();
            jest.mocked(auth.signIn).mockRejectedValue(new Error("boom"));
            await render(<LoginView />);
            expect(router.replace).not.toHaveBeenCalledWith(Path.Root);
            expect(
                screen.getByText(
                    "An unexpected error occurred while processing the request, please try again."
                )
            ).toBeTruthy();
        });
    }

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
