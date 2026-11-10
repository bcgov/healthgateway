import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import AsyncStorage from "@react-native-async-storage/async-storage";

import { services } from "@/services/container";
import { useAppLockStore } from "@/stores/appLock.store";
import { useAuthStore } from "@/stores/auth.store";
import { bootstrap } from "@/stores/bootstrap";
import {
    MobileConfigurationError,
    useConfigStore,
} from "@/stores/config.store";
import { useNotificationStore } from "@/stores/notification.store";
import { useUserStore } from "@/stores/user.store";
import { configFixture, resetStores } from "@/test/fixtures";

const mockCreateAuth = jest.fn<(config: unknown) => unknown>();
jest.mock("@/services/auth", () => ({
    createAuthService: (config: unknown) => mockCreateAuth(config),
}));

const deferred = <T>() => {
    let resolve: (value: T) => void = () => undefined;
    const promise = new Promise<T>((res) => {
        resolve = res;
    });
    return { promise, resolve };
};
const settle = async () => {
    for (let i = 0; i < 5; i += 1)
        await new Promise((resolve) => setImmediate(resolve));
};
const details = {
    idToken: "i",
    accessToken: "a",
    expired: false,
    refreshTokenTime: Math.floor(Date.now() / 1000) + 300,
    hdid: "OLD",
};
const fakeService = (init: Promise<void>, info: Promise<unknown>) => ({
    init: jest.fn(() => init),
    getOidcTokenDetails: jest.fn(() => details),
    getOidcUserInfo: jest.fn(() => info),
    getRestoreFailure: () => undefined,
    signOut: jest.fn(async () => undefined),
    clearState: jest.fn(),
    refreshToken: jest.fn(async () => false),
});

describe("start-up replaced by a newer one", () => {
    beforeEach(() => {
        jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
            configFixture
        );
        useAppLockStore.setState({ enabled: false });
    });
    afterEach(() => {
        jest.restoreAllMocks();
        mockCreateAuth.mockReset();
        resetStores();
    });

    const newest = () => {
        mockCreateAuth.mockReturnValueOnce(
            fakeService(Promise.resolve(), Promise.resolve({}))
        );
        const service = fakeService(Promise.resolve(), Promise.resolve({}));
        service.getOidcTokenDetails = jest.fn(() => null) as never;
        mockCreateAuth.mockReset();
        mockCreateAuth.mockReturnValue(service);
        return bootstrap("/");
    };

    it("an old start-up waiting on the login cannot sign out or sign in", async () => {
        const init = deferred<void>();
        const old = fakeService(init.promise, Promise.resolve({ hdid: "OLD" }));
        mockCreateAuth.mockReturnValueOnce(old);
        const first = bootstrap("/");
        await settle();
        expect(old.init).toHaveBeenCalled();

        const second = newest();
        await expect(second).resolves.toBe(true);
        const signOut = jest.spyOn(useAuthStore.getState(), "signOut");

        init.resolve();
        await expect(first).resolves.toBe(false);
        expect(signOut).not.toHaveBeenCalled();
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useUserStore.getState().user.hdid).toBe("");
    });

    it("an old start-up does not go on to fetch notifications", async () => {
        const essential = deferred<void>();
        jest.spyOn(
            useUserStore.getState(),
            "retrieveEssentialData"
        ).mockReturnValue(essential.promise);
        useUserStore.setState({
            user: Object.assign(useUserStore.getState().user, { hdid: "OLD" }),
        });
        await AsyncStorage.setItem("hg.session.lastActive", String(Date.now()));
        const notifications = jest
            .spyOn(useNotificationStore.getState(), "retrieve")
            .mockResolvedValue(undefined as never);
        const old = fakeService(
            Promise.resolve(),
            Promise.resolve({ hdid: "OLD" })
        );
        mockCreateAuth.mockReturnValueOnce(old);
        const first = bootstrap("/");
        await settle();
        await expect(newest()).resolves.toBe(true);
        essential.resolve();
        await expect(first).resolves.toBe(false);
        expect(notifications).not.toHaveBeenCalled();
    });

    it("an old start-up waiting on user info cannot apply it", async () => {
        await AsyncStorage.setItem("hg.session.lastActive", String(Date.now()));
        const info = deferred<unknown>();
        const old = fakeService(Promise.resolve(), info.promise);
        mockCreateAuth.mockReturnValueOnce(old);
        const first = bootstrap("/");
        await settle();
        expect(old.getOidcUserInfo).toHaveBeenCalled();

        await expect(newest()).resolves.toBe(true);
        info.resolve({ hdid: "OLD" });
        await expect(first).resolves.toBe(false);
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useUserStore.getState().user.hdid).toBe("");
    });

    it("a failure in an old start-up is not reported", async () => {
        let fail: (error: Error) => void = () => undefined;
        (
            services.config.getConfiguration as jest.Mock<() => Promise<never>>
        ).mockReturnValueOnce(
            new Promise((_, reject) => {
                fail = reject;
            })
        );
        const first = bootstrap("/");
        await settle();
        await expect(newest()).resolves.toBe(true);
        fail(new Error("old environment is down"));
        await expect(first).resolves.toBe(false);
        expect(useConfigStore.getState().status).toBe("loaded");
    });
});

describe("native sign-in settings", () => {
    beforeEach(() => {
        useAppLockStore.setState({ enabled: false });
    });
    afterEach(() => {
        jest.restoreAllMocks();
        mockCreateAuth.mockReset();
        resetStores();
    });

    it("signs in with the mobile client at the mobile endpoint, not the web client's", async () => {
        jest.spyOn(services.config, "getConfiguration").mockResolvedValue(
            configFixture
        );
        mockCreateAuth.mockReturnValue(
            fakeService(Promise.resolve(), Promise.resolve({}))
        );
        await expect(bootstrap("/")).resolves.toBe(true);
        expect(mockCreateAuth).toHaveBeenCalledWith({
            authority: "https://sso.test/auth/realms/hg-mobile",
            clientId: "hg-mobile-ios",
            scope: configFixture.openIdConnect.scope,
            callbacks: null,
        });
    });

    it("stops start-up when the server leaves out a setting", async () => {
        jest.spyOn(services.config, "getConfiguration").mockResolvedValue({
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                mobile: { authentication: { endpoint: "https://sso.test/x" } },
            },
        });
        await expect(bootstrap("/")).rejects.toBeInstanceOf(
            MobileConfigurationError
        );
        expect(mockCreateAuth).not.toHaveBeenCalled();
    });
});
