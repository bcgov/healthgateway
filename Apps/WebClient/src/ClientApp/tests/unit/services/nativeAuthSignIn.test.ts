import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { SessionEndedError } from "@/models/errors";
import { NativeAuthService } from "@/services/auth/authService.impl.native";

const mockStored = new Map<string, string>();
const mockSetItem = jest.fn<(key: string, value: string) => Promise<void>>();
const mockDeleteItem = jest.fn<(key: string) => Promise<void>>();
const mockPrompt = jest.fn<() => Promise<unknown>>();
const mockExchange = jest.fn<() => Promise<unknown>>();

jest.mock("expo-auth-session", () => ({
    makeRedirectUri: () => "healthgateway://x",
    fetchDiscoveryAsync: async () => ({
        endSessionEndpoint: "https://kc/logout",
    }),
    exchangeCodeAsync: () => mockExchange(),
    refreshAsync: jest.fn(),
    AuthRequest: class {
        codeVerifier = "v";
        promptAsync = () => mockPrompt();
    },
    ResponseType: { Code: "code" },
    CodeChallengeMethod: { S256: "S256" },
}));
jest.mock("expo-web-browser", () => ({}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: async (key: string) => mockStored.get(key) ?? null,
    setItemAsync: (key: string, value: string) => mockSetItem(key, value),
    deleteItemAsync: (key: string) => mockDeleteItem(key),
    WHEN_UNLOCKED_THIS_DEVICE_ONLY: "x",
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
const newService = () =>
    new NativeAuthService({
        authority: "https://kc",
        clientId: "hg",
        scope: "openid",
    } as never);
const tokens = {
    accessToken: "a-old",
    refreshToken: "r-old",
    idToken: "i",
    expiresIn: 300,
};
const success = { type: "success", params: { code: "c" } };

describe("native sign-in that outlives its login", () => {
    let logout: jest.SpiedFunction<typeof fetch>;

    beforeEach(() => {
        mockStored.clear();
        mockPrompt.mockReset();
        mockExchange.mockReset();
        mockSetItem.mockReset();
        mockDeleteItem.mockReset();
        mockSetItem.mockImplementation(async (key, value) => {
            mockStored.set(key, value);
        });
        mockDeleteItem.mockImplementation(async (key) => {
            mockStored.delete(key);
        });
        mockPrompt.mockResolvedValue(success);
        mockExchange.mockResolvedValue(tokens);
        logout = jest
            .spyOn(global, "fetch")
            .mockImplementation(async () => new Response("{}"));
    });
    afterEach(() => {
        jest.restoreAllMocks();
    });

    /** What a next launch would find. */
    const nextLaunch = async () => {
        const service = newService();
        await service.init();
        return service.getOidcTokenDetails();
    };

    it("a sign-in left alone is kept, in memory and for the next launch", async () => {
        const service = newService();
        const details = await service.signIn("/");
        expect(details.accessToken).toBe("a-old");
        expect((await nextLaunch())?.accessToken).toBe("a-old");
    });

    it.each([
        ["signing out", (s: NativeAuthService) => s.signOut()],
        [
            "a reset or clearing the session",
            async (s: NativeAuthService) => s.clearState(),
        ],
    ])("%s during the code exchange keeps nothing", async (_name, end) => {
        const service = newService();
        const exchange = deferred<unknown>();
        mockExchange.mockReturnValue(exchange.promise);
        const login = service.signIn("/");
        await settle();
        await end(service);
        exchange.resolve(tokens);
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        await settle();
        expect(service.getOidcTokenDetails()).toBeNull();
        expect(mockStored.size).toBe(0);
        await expect(nextLaunch()).resolves.toBeNull();
    });

    it("the discarded login is ended at Keycloak", async () => {
        const service = newService();
        const exchange = deferred<unknown>();
        mockExchange.mockReturnValue(exchange.promise);
        const login = service.signIn("/");
        await settle();
        await service.signOut();
        exchange.resolve(tokens);
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        await settle();
        const [url, init] = logout.mock.calls[0];
        expect(url).toBe("https://kc/logout");
        expect((init as RequestInit).body).toContain("refresh_token=r-old");
    });

    it("signing out while the sheet is open never exchanges the code", async () => {
        const service = newService();
        const prompt = deferred<unknown>();
        mockPrompt.mockReturnValue(prompt.promise);
        const login = service.signIn("/");
        await settle();
        await service.signOut();
        prompt.resolve(success);
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        expect(mockExchange).not.toHaveBeenCalled();
        expect(mockStored.size).toBe(0);
    });

    it("signing out during a slow write leaves nothing stored", async () => {
        const service = newService();
        const write = deferred<void>();
        mockSetItem.mockImplementationOnce(async (key, value) => {
            await write.promise;
            mockStored.set(key, value);
        });
        const login = service.signIn("/");
        await settle();
        expect(mockSetItem).toHaveBeenCalled();
        const signOut = service.signOut();
        await settle();
        // the delete waits for the write, so it cannot be overtaken
        expect(mockDeleteItem).not.toHaveBeenCalled();
        write.resolve();
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        await signOut;
        expect(service.getOidcTokenDetails()).toBeNull();
        expect(mockStored.size).toBe(0);
        await expect(nextLaunch()).resolves.toBeNull();
    });

    it("a write still waiting its turn is dropped once signed out", async () => {
        const service = newService();
        const earlier = deferred<void>();
        mockDeleteItem.mockImplementationOnce(async (key) => {
            await earlier.promise;
            mockStored.delete(key);
        });
        service.clearState();
        await settle();
        const login = service.signIn("/");
        await settle();
        service.clearState();
        earlier.resolve();
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        await settle();
        expect(mockSetItem).not.toHaveBeenCalled();
        expect(mockStored.size).toBe(0);
    });

    it("a slow delete from signing out does not remove the next login", async () => {
        const service = newService();
        const slow = deferred<void>();
        mockDeleteItem.mockImplementationOnce(async (key) => {
            await slow.promise;
            mockStored.delete(key);
        });
        const signOut = service.signOut();
        await settle();
        const login = service.signIn("/");
        await settle();
        slow.resolve();
        await signOut;
        await expect(login).resolves.toMatchObject({ accessToken: "a-old" });
        expect((await nextLaunch())?.accessToken).toBe("a-old");
    });

    it("signing out still ends the login in memory when the stored copy cannot be deleted", async () => {
        const service = newService();
        await service.signIn("/");
        mockDeleteItem.mockRejectedValueOnce(new Error("keychain"));
        await expect(service.signOut()).resolves.toBeUndefined();
        expect(service.getOidcTokenDetails()).toBeNull();
    });

    it("a first sign-in answered without usable tokens stores nothing", async () => {
        const service = newService();
        mockExchange.mockResolvedValue({ idToken: "i", expiresIn: 300 });
        await expect(service.signIn("/")).rejects.toThrow();
        expect(service.getOidcTokenDetails()).toBeNull();
        expect(mockStored.size).toBe(0);
    });
});
