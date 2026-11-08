import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { services } from "@/services/container";
import { refreshTokenIfDue, useAuthStore } from "@/stores/auth.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useUserStore } from "@/stores/user.store";
import {
    bcscUserInfo,
    configFixture,
    fakeAuth,
    resetStores,
    tokenDetails,
} from "@/test/fixtures";

describe("auth store", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });
    afterEach(() => {
        jest.useRealTimers();
        resetStores();
    });

    it("checkStatus with a live session loads user info and marks the user authenticated", async () => {
        const auth = fakeAuth(tokenDetails);
        services.initialize(configFixture, auth);
        await expect(useAuthStore.getState().checkStatus()).resolves.toBe(true);
        expect(useAuthStore.getState().isAuthenticated).toBe(true);
        expect(useAuthStore.getState().tokenDetails).toBe(tokenDetails);
        expect(useUserStore.getState().user.hdid).toBe(bcscUserInfo.hdid);
        expect(auth.getOidcUserInfo).toHaveBeenCalled();
    });

    it("checkStatus with no session clears a stale authenticated flag", async () => {
        services.initialize(configFixture, fakeAuth(undefined));
        useAuthStore.setState({ isAuthenticated: true });
        await expect(useAuthStore.getState().checkStatus()).resolves.toBe(
            false
        );
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
    });

    it("schedules a token refresh for the refresh time", async () => {
        const auth = fakeAuth(tokenDetails);
        services.initialize(configFixture, auth);
        useAuthStore.getState().handleSuccessfulAuthentication({
            ...tokenDetails,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 60,
        });
        expect(auth.refreshToken).not.toHaveBeenCalled();
        await jest.advanceTimersByTimeAsync(61000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(1);
    });

    it("signOut clears tokens, user data and the bearer header even if the end-session call fails", async () => {
        const auth = fakeAuth(tokenDetails);
        (auth.signOut as jest.Mock).mockImplementation(async () => {
            throw new Error("no network");
        });
        services.initialize(configFixture, auth);
        await useAuthStore.getState().checkStatus();
        await useAuthStore.getState().signOut();
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useAuthStore.getState().tokenDetails).toBeUndefined();
        expect(useUserStore.getState().user.hdid).toBe("");
    });

    it("signIn loads user info and authenticates with the returned tokens", async () => {
        const auth = fakeAuth(tokenDetails);
        services.initialize(configFixture, auth);
        await useAuthStore.getState().signIn("/home", "bcsc");
        expect(auth.signIn).toHaveBeenCalledWith("/home", "bcsc");
        expect(useAuthStore.getState().isAuthenticated).toBe(true);
        expect(useUserStore.getState().oidcUserInfo?.hdid).toBe("HDID1");
    });

    it("a refresh that renews the token re-applies it; a refused refresh signs out", async () => {
        const auth = fakeAuth(tokenDetails);
        (auth.refreshToken as jest.Mock<() => Promise<boolean>>)
            .mockResolvedValueOnce(true)
            .mockRejectedValueOnce(
                Object.assign(new Error("expired"), { code: "invalid_grant" })
            );
        services.initialize(configFixture, auth);
        // whole-second clock: each refresh is due exactly 1 s after the previous one
        jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
        const soon = () => ({
            ...tokenDetails,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 1,
        });
        (
            auth.getOidcTokenDetails as jest.Mock<() => typeof tokenDetails>
        ).mockImplementation(soon);
        useAuthStore.getState().handleSuccessfulAuthentication(soon());
        await jest.advanceTimersByTimeAsync(1000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(1);
        expect(useAuthStore.getState().isAuthenticated).toBe(true);
        await jest.advanceTimersByTimeAsync(1000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(2);
        expect(auth.signOut).toHaveBeenCalled();
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
    });

    it("a refresh with no answer keeps the login on native and tries again; web signs out", async () => {
        const auth = fakeAuth(tokenDetails);
        (auth.refreshToken as jest.Mock<() => Promise<boolean>>)
            .mockRejectedValueOnce(new TypeError("Network request failed"))
            .mockResolvedValue(true);
        services.initialize(configFixture, auth);
        jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
        const soon = () => ({
            ...tokenDetails,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 1,
        });
        (
            auth.getOidcTokenDetails as jest.Mock<() => typeof tokenDetails>
        ).mockImplementation(soon);
        useAuthStore.getState().handleSuccessfulAuthentication(soon());
        await jest.advanceTimersByTimeAsync(1000);
        if (Platform.OS === "web") {
            expect(auth.signOut).toHaveBeenCalled();
            return;
        }
        expect(auth.signOut).not.toHaveBeenCalled();
        expect(useAuthStore.getState().isAuthenticated).toBe(true);
        expect(useConnectivityStore.getState().serviceUnavailable).toBe(true);
        await jest.advanceTimersByTimeAsync(30000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(2);
        useConnectivityStore.setState({ serviceUnavailable: false });
    });

    if (Platform.OS !== "web") {
        const failRefresh = (error: Error) => {
            const auth = fakeAuth(tokenDetails);
            (
                auth.refreshToken as jest.Mock<() => Promise<boolean>>
            ).mockRejectedValue(error);
            services.initialize(configFixture, auth);
            jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
            useAuthStore.getState().handleSuccessfulAuthentication({
                ...tokenDetails,
                refreshTokenTime: Math.floor(Date.now() / 1000) + 1,
            });
            return auth;
        };

        it("a failure that is not a lost connection or a refusal stops retrying after three", async () => {
            const auth = failRefresh(new SyntaxError("Unexpected token <"));
            await jest.advanceTimersByTimeAsync(1000);
            expect(useConnectivityStore.getState().serviceUnavailable).toBe(
                false
            );
            expect(auth.signOut).not.toHaveBeenCalled();
            await jest.advanceTimersByTimeAsync(60000);
            expect(auth.refreshToken).toHaveBeenCalledTimes(3);
            expect(auth.signOut).toHaveBeenCalled();
            expect(useAuthStore.getState().isAuthenticated).toBe(false);
        });

        it("a TypeError that is not a lost connection stops retrying too", async () => {
            const auth = failRefresh(
                new TypeError("Cannot read properties of undefined")
            );
            await jest.advanceTimersByTimeAsync(61000);
            expect(auth.refreshToken).toHaveBeenCalledTimes(3);
            expect(auth.signOut).toHaveBeenCalled();
        });

        it("a lost connection keeps retrying for as long as it lasts", async () => {
            const auth = failRefresh(new TypeError("Network request failed"));
            await jest.advanceTimersByTimeAsync(1000 + 30000 * 5);
            expect(auth.refreshToken).toHaveBeenCalledTimes(6);
            expect(auth.signOut).not.toHaveBeenCalled();
            useConnectivityStore.setState({ serviceUnavailable: false });
        });
    }

    it("a token already due for refresh still gets one, soon", async () => {
        const auth = fakeAuth(tokenDetails);
        services.initialize(configFixture, auth);
        jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
        useAuthStore.getState().handleSuccessfulAuthentication({
            ...tokenDetails,
            refreshTokenTime: Math.floor(Date.now() / 1000) - 10,
        });
        await jest.advanceTimersByTimeAsync(1000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(1);
    });

    it("a refresh that was not needed yet does not end the refreshing", async () => {
        const auth = fakeAuth(tokenDetails);
        (auth.refreshToken as jest.Mock<() => Promise<boolean>>)
            .mockResolvedValueOnce(false)
            .mockResolvedValue(true);
        jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
        const soon = () => ({
            ...tokenDetails,
            refreshToken: "r",
            expired: false,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 1,
        });
        (
            auth.getOidcTokenDetails as jest.Mock<() => typeof tokenDetails>
        ).mockImplementation(soon);
        services.initialize(configFixture, auth);
        useAuthStore.getState().handleSuccessfulAuthentication(soon());
        await jest.advanceTimersByTimeAsync(1000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(1);
        await jest.advanceTimersByTimeAsync(1000);
        expect(auth.refreshToken).toHaveBeenCalledTimes(2);
    });

    describe("refreshTokenIfDue", () => {
        const signedIn = (seconds: number) => {
            const auth = fakeAuth(tokenDetails);
            jest.setSystemTime(new Date("2026-10-29T00:00:00.000Z"));
            const details = {
                ...tokenDetails,
                refreshTokenTime: Math.floor(Date.now() / 1000) + seconds,
            };
            (
                auth.getOidcTokenDetails as jest.Mock<() => typeof tokenDetails>
            ).mockReturnValue(details);
            services.initialize(configFixture, auth);
            useAuthStore.getState().handleSuccessfulAuthentication(details);
            return auth;
        };

        it("refreshes once the token is due", async () => {
            const auth = signedIn(100);
            jest.setSystemTime(new Date("2026-10-29T00:10:00.000Z"));
            await refreshTokenIfDue();
            expect(auth.refreshToken).toHaveBeenCalledTimes(1);
        });

        it("leaves a token that is not due yet", async () => {
            const auth = signedIn(100);
            await refreshTokenIfDue();
            expect(auth.refreshToken).not.toHaveBeenCalled();
        });

        it("does nothing while the login is on hold", async () => {
            const auth = signedIn(100);
            jest.setSystemTime(new Date("2026-10-29T00:10:00.000Z"));
            useConnectivityStore.setState({ isOnline: false });
            await refreshTokenIfDue();
            expect(auth.refreshToken).not.toHaveBeenCalled();
            useConnectivityStore.setState({ isOnline: true });
        });
    });

    it("clearStorage drops the session and the auth service state", () => {
        const auth = fakeAuth(tokenDetails);
        services.initialize(configFixture, auth);
        useAuthStore.setState({ isAuthenticated: true, tokenDetails });
        useAuthStore.getState().clearStorage();
        expect(auth.clearState).toHaveBeenCalled();
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
    });
});
