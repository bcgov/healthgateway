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
import { useRouteGuard } from "@/hooks/useRouteGuard";
import { services } from "@/services/container";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { useUserStore } from "@/stores/user.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    bcscUserInfo,
    configFixture,
    makeProfile,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { renderHook } from "@/test/ui";

describe("useRouteGuard (beforeEachGuard port)", () => {
    beforeEach(() => {
        jest.spyOn(services.tracking, "trackPageView").mockImplementation(
            () => undefined
        );
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("does nothing until the app is ready", async () => {
        resetRouter(Path.Home);
        await renderHook(() => useRouteGuard(false));
        expect(router.replace).not.toHaveBeenCalled();
    });

    it("sends a visitor from a private page: web to login with the redirect, native to the landing page", async () => {
        resetRouter(Path.Timeline);
        await renderHook(() => useRouteGuard(true));
        if (Platform.OS === "web") {
            expect(router.replace).toHaveBeenCalledWith({
                pathname: Path.Login,
                params: { redirect: Path.Timeline },
            });
        } else {
            expect(router.replace).toHaveBeenCalledWith(Path.Root);
        }
    });

    it("an explicit login request goes to login, once, even from a public page", async () => {
        resetRouter(Path.Root);
        useAuthStore.setState({ loginRequest: { idp: "bcsc" } });
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith({
            pathname: Path.Login,
            params: { idp: "bcsc", redirect: Path.Root },
        });
        expect(useAuthStore.getState().loginRequest).toBeUndefined();
    });

    it("a login request waits until the session has ended", async () => {
        resetRouter(Path.Timeline);
        signInRegistered();
        useAuthStore.setState({ loginRequest: {} });
        await renderHook(() => useRouteGuard(true));
        expect(useAuthStore.getState().loginRequest).toEqual({});
        useAuthStore.setState({ loginRequest: undefined });
    });

    it("leaves a visitor on public pages and tracks the page view", async () => {
        resetRouter(Path.TermsOfService);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).not.toHaveBeenCalled();
        expect(services.tracking.trackPageView).toHaveBeenCalledWith(
            Path.TermsOfService
        );
    });

    it("sends a registered user away from the registration page", async () => {
        signInRegistered();
        resetRouter(Path.Registration);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.Home);
    });

    it("waits while a signed-in user's essential data is still loading", async () => {
        signInRegistered();
        useUserStore.setState({ essentialDataLoaded: false });
        resetRouter(Path.Registration);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).not.toHaveBeenCalled();
    });

    it("sends a not-registered user to registration and an invalid provider to the IDIR page", async () => {
        useAuthStore.setState({ isAuthenticated: true });
        useUserStore.getState().setOidcUserInfo(bcscUserInfo);
        useUserStore.setState({ essentialDataLoaded: true });
        resetRouter(Path.Home);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.Registration);

        updateStores(() =>
            useUserStore
                .getState()
                .setOidcUserInfo({ ...bcscUserInfo, idp: "idir" })
        );
        resetRouter(Path.Home);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.IdirLoggedIn);
    });

    it("sends a pending-deletion user to Profile and keeps them there", async () => {
        signInRegistered({ closedDateTime: "2026-10-01T00:00:00Z" });
        resetRouter(Path.Home);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.Profile);
        resetRouter(Path.Profile);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).not.toHaveBeenCalled();
    });

    it("blocks a route whose feature toggle is off", async () => {
        signInRegistered();
        const t = configFixture.webClient.featureToggleConfiguration;
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...t,
                        services: { ...t.services, enabled: false },
                    },
                },
            },
        });
        resetRouter(Path.Services);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.Unauthorized);
    });

    it("routes unknown paths to not-found and leaves stateless routes alone", async () => {
        resetRouter("/does-not-exist");
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).toHaveBeenCalledWith(Path.NotFound);
        resetRouter(Path.Logout);
        await renderHook(() => useRouteGuard(true));
        expect(router.replace).not.toHaveBeenCalled();
    });

    it("on native sends a registered user from the landing page to Home; web keeps the landing page", async () => {
        signInRegistered(makeProfile());
        resetRouter(Path.Root);
        await renderHook(() => useRouteGuard(true));
        if (Platform.OS === "web")
            expect(router.replace).not.toHaveBeenCalled();
        else expect(router.replace).toHaveBeenCalledWith(Path.Home);
    });
});
