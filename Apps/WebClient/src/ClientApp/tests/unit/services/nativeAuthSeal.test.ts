import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import * as SecureStore from "expo-secure-store";

import { NativeAuthService } from "@/services/auth/authService.impl.native";
import {
    SEALED_STORAGE_KEY,
    STORAGE_KEY,
} from "@/services/auth/storedSession.native";
import { vault } from "@/services/vault";

const mockStore = new Map<string, string>();
const mockRefreshAsync = jest.fn<(...args: unknown[]) => Promise<unknown>>();

jest.mock("expo-auth-session", () => ({
    makeRedirectUri: () => "healthgateway://x",
    fetchDiscoveryAsync: async () => ({ userInfoEndpoint: "https://kc/info" }),
    refreshAsync: (...args: unknown[]) => mockRefreshAsync(...args),
}));
jest.mock("expo-web-browser", () => ({}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(async (key: string) => mockStore.get(key) ?? null),
    setItemAsync: jest.fn(async (key: string, value: string) => {
        mockStore.set(key, value);
    }),
    deleteItemAsync: jest.fn(async (key: string) => {
        mockStore.delete(key);
    }),
    WHEN_UNLOCKED_THIS_DEVICE_ONLY: "x",
}));
// only the vault itself is replaced: what its error codes mean stays the service's own (`isVaultKeyGone` and the like)
jest.mock("@/services/vault", () => ({
    ...jest.requireActual<typeof import("@/services/vault")>(
        "@/services/vault"
    ),
    vault: {
        encrypt: jest.fn(async (text: string) => `sealed:${text}`),
        decrypt: jest.fn(async (text: string) => text.slice("sealed:".length)),
    },
}));

const encrypt = jest.mocked(vault.encrypt);
const decrypt = jest.mocked(vault.decrypt);

const inAnHour = () => Math.floor(Date.now() / 1000) + 3600;
const session = (refreshToken: string, expiresAt = inAnHour()) => ({
    accessToken: `a-${refreshToken}`,
    refreshToken,
    idToken: "i",
    expiresAt,
});
const tokens = { accessToken: "a2", refreshToken: "r2", expiresIn: 300 };

/** What the service stores under the sealed key: `text` encrypted by the (mock) vault, and when its tokens were issued. */
const sealedText = (text: string, issuedAt?: number) =>
    JSON.stringify({ issuedAt, data: `sealed:${text}` });
const sealedValue = (stored: { expiresAt: number; issuedAt?: number }) =>
    sealedText(JSON.stringify(stored), stored.issuedAt);

const sealedCopy = () => {
    const value = mockStore.get(SEALED_STORAGE_KEY);
    if (!value) return undefined;
    const copy = JSON.parse(value) as { issuedAt?: number; data: string };
    return {
        issuedAt: copy.issuedAt,
        ...(JSON.parse(copy.data.slice("sealed:".length)) as {
            refreshToken: string;
        }),
    };
};

const plainCopy = () => {
    const value = mockStore.get(STORAGE_KEY);
    return value ? (JSON.parse(value) as { refreshToken: string }) : undefined;
};

const service = () =>
    new NativeAuthService({
        authority: "https://kc",
        clientId: "hg",
        scope: "openid",
    } as never);

/** A service on a launch with device security on and a sealed login, unsealed. */
const unsealed = async (
    stored: ReturnType<typeof session> & { issuedAt?: number } = session("r1")
) => {
    mockStore.set(SEALED_STORAGE_KEY, sealedValue(stored));
    const auth = service();
    await auth.init({ keepSessionOnFailure: true, sealed: true });
    await auth.unseal();
    return auth;
};

describe("native session sealed by the vault", () => {
    beforeEach(() => {
        mockStore.clear();
        mockRefreshAsync.mockReset();
        encrypt.mockClear();
        decrypt.mockClear();
    });

    describe("at launch with device security on", () => {
        it("keeps a sealed login sealed: no token in memory, no refresh, no prompt", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1", 0)));
            const auth = service();
            await auth.init({ keepSessionOnFailure: true, sealed: true });

            expect(auth.isSealed()).toBe(true);
            expect(auth.getOidcTokenDetails()).toBeNull();
            expect(mockRefreshAsync).not.toHaveBeenCalled();
            expect(decrypt).not.toHaveBeenCalled();
        });

        it("deletes an unsealed copy left beside the sealed one", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            mockStore.set(STORAGE_KEY, JSON.stringify(session("r0")));
            await service().init({ sealed: true });
            expect(mockStore.has(STORAGE_KEY)).toBe(false);
        });

        it("an unsealed copy that could not be removed at the start goes once the login is unsealed", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            mockStore.set(STORAGE_KEY, JSON.stringify(session("old")));
            jest.mocked(SecureStore.deleteItemAsync).mockRejectedValueOnce(
                new Error("keychain")
            );
            const auth = service();
            await auth.init({ sealed: true });
            expect(plainCopy()?.refreshToken).toBe("old");

            // or the tokens would stay outside the vault for as long as the login lasts
            expect(await auth.unseal()).toBe("unsealed");
            expect(plainCopy()).toBeUndefined();
            expect(sealedCopy()?.refreshToken).toBe("r1");
        });

        it("or, failing that, at the next save", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1", 0)));
            mockStore.set(STORAGE_KEY, JSON.stringify(session("old")));
            jest.mocked(SecureStore.deleteItemAsync)
                .mockRejectedValueOnce(new Error("keychain"))
                .mockRejectedValueOnce(new Error("keychain"));
            const auth = service();
            await auth.init({ sealed: true });
            expect(await auth.unseal()).toBe("unsealed");
            expect(plainCopy()?.refreshToken).toBe("old");

            mockRefreshAsync.mockResolvedValueOnce(tokens);
            await auth.refreshToken(true);
            expect(plainCopy()).toBeUndefined();
            expect(sealedCopy()?.refreshToken).toBe("r2");
        });

        it("keeps the sealed copy when the unsealed one beside it is older", async () => {
            const now = Math.floor(Date.now() / 1000);
            mockStore.set(
                SEALED_STORAGE_KEY,
                sealedValue({ ...session("r2"), issuedAt: now })
            );
            mockStore.set(
                STORAGE_KEY,
                JSON.stringify({ ...session("r1"), issuedAt: now - 600 })
            );
            const auth = service();
            await auth.init({ sealed: true });

            expect(auth.isSealed()).toBe(true);
            expect(plainCopy()).toBeUndefined();
            expect(await auth.unseal()).toBe("unsealed");
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r2");
        });

        it("seals a newer unsealed copy over the sealed one, so its refresh token is not lost", async () => {
            // sealing a refresh's tokens failed, and so did removing the sealed copy of the ones before
            const auth = await unsealed({
                ...session("r1", 0),
                issuedAt: Math.floor(Date.now() / 1000) - 600,
            });
            encrypt.mockRejectedValueOnce(new Error("no vault"));
            jest.mocked(SecureStore.deleteItemAsync).mockRejectedValueOnce(
                new Error("keychain")
            );
            mockRefreshAsync.mockResolvedValueOnce(tokens);
            await auth.refreshToken(true);
            expect(sealedCopy()?.refreshToken).toBe("r1");
            expect(plainCopy()?.refreshToken).toBe("r2");

            // the app closes before another save clears it up
            const next = service();
            await next.init({ keepSessionOnFailure: true, sealed: true });
            expect(sealedCopy()?.refreshToken).toBe("r2");
            expect(plainCopy()).toBeUndefined();
            expect(next.isSealed()).toBe(true);
            expect(next.getOidcTokenDetails()).toBeNull();
            expect(mockRefreshAsync).toHaveBeenCalledTimes(1);

            expect(await next.unseal()).toBe("unsealed");
            expect(next.getOidcTokenDetails()?.refreshToken).toBe("r2");
        });

        it("stores when the tokens were issued in the clear beside the sealed copy", async () => {
            const auth = await unsealed(session("r1", 0));
            mockRefreshAsync.mockResolvedValueOnce({
                ...tokens,
                issuedAt: 1234,
            });
            await auth.refreshToken(true);
            const stored = JSON.parse(
                mockStore.get(SEALED_STORAGE_KEY) ?? "{}"
            ) as { issuedAt?: number };
            expect(stored.issuedAt).toBe(1234);
            expect(sealedCopy()?.refreshToken).toBe("r2");
        });

        it("seals a login kept unsealed by an earlier build, before anything uses it", async () => {
            mockStore.set(STORAGE_KEY, JSON.stringify(session("r1", 0)));
            const auth = service();
            await auth.init({ keepSessionOnFailure: true, sealed: true });

            expect(sealedCopy()?.refreshToken).toBe("r1");
            expect(mockStore.has(STORAGE_KEY)).toBe(false);
            expect(auth.isSealed()).toBe(true);
            expect(auth.getOidcTokenDetails()).toBeNull();
            expect(mockRefreshAsync).not.toHaveBeenCalled();
        });

        it("with device security off, a sealed copy is not read", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            const auth = service();
            await auth.init();
            expect(auth.isSealed()).toBe(false);
            expect(auth.getOidcTokenDetails()).toBeNull();
            expect(decrypt).not.toHaveBeenCalled();
        });
    });

    describe("unsealing", () => {
        it("puts the login back in memory", async () => {
            const auth = await unsealed();
            expect(decrypt).toHaveBeenCalledTimes(1);
            expect(auth.isSealed()).toBe(false);
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r1");
        });

        it("refreshes an expired login when asked to restore it", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1", 0)));
            const auth = service();
            await auth.init({ keepSessionOnFailure: true, sealed: true });
            mockRefreshAsync.mockResolvedValue(tokens);

            expect(await auth.unseal({ restore: true })).toBe("unsealed");
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r2");
            // the new tokens are sealed too
            expect(sealedCopy()?.refreshToken).toBe("r2");
            expect(mockStore.has(STORAGE_KEY)).toBe(false);
        });

        it("keeps a login that could not refresh for lack of a connection", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1", 0)));
            const auth = service();
            await auth.init({ keepSessionOnFailure: true, sealed: true });
            mockRefreshAsync.mockRejectedValue(
                new TypeError("Network request failed")
            );

            expect(await auth.unseal({ restore: true })).toBe("unsealed");
            expect(auth.getRestoreFailure()).toBe("network");
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r1");
        });

        it("stays sealed when the prompt is turned down", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            const auth = service();
            await auth.init({ sealed: true });
            decrypt.mockRejectedValueOnce(
                Object.assign(new Error("no"), { code: "ERR_VAULT_CANCELLED" })
            );

            expect(await auth.unseal()).toBe("cancelled");
            expect(auth.isSealed()).toBe(true);
            expect(mockStore.has(SEALED_STORAGE_KEY)).toBe(true);
        });

        it("deletes a login nothing can read any more", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            const auth = service();
            await auth.init({ sealed: true });
            decrypt.mockRejectedValueOnce(
                Object.assign(new Error("gone"), {
                    code: "ERR_VAULT_INVALIDATED",
                })
            );

            expect(await auth.unseal()).toBe("unreadable");
            expect(auth.isSealed()).toBe(false);
            expect(auth.getOidcTokenDetails()).toBeNull();
            expect(mockStore.size).toBe(0);
        });

        it.each(["ERR_VAULT_UNAVAILABLE", "ERR_VAULT_CRYPTO"])(
            "keeps a login it could not read this time (%s), for the next try",
            async (code) => {
                mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
                const auth = service();
                await auth.init({ sealed: true });
                decrypt.mockRejectedValueOnce(
                    Object.assign(new Error("not now"), { code })
                );

                expect(await auth.unseal()).toBe("unavailable");
                expect(auth.isSealed()).toBe(true);
                expect(auth.getOidcTokenDetails()).toBeNull();
                expect(sealedCopy()?.refreshToken).toBe("r1");

                expect(await auth.unseal()).toBe("unsealed");
                expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r1");
            }
        );

        it("keeps a login whose stored copy could not be read this time", async () => {
            mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
            const auth = service();
            await auth.init({ sealed: true });
            jest.mocked(SecureStore.getItemAsync).mockRejectedValueOnce(
                new Error("keychain")
            );

            expect(await auth.unseal()).toBe("unavailable");
            expect(auth.isSealed()).toBe(true);
            expect(sealedCopy()?.refreshToken).toBe("r1");
        });

        it.each(["not json", JSON.stringify({ refreshToken: "r1" }), "null"])(
            "deletes a copy that does not hold a session (%s)",
            async (damaged) => {
                mockStore.set(SEALED_STORAGE_KEY, sealedText(damaged));
                const auth = service();
                await auth.init({ sealed: true });

                expect(await auth.unseal()).toBe("unreadable");
                expect(auth.getOidcTokenDetails()).toBeNull();
                expect(mockStore.size).toBe(0);
            }
        );
    });

    it("deletes a sealed copy stored in another shape, without asking the vault", async () => {
        mockStore.set(SEALED_STORAGE_KEY, "sealed:x");
        const auth = service();
        await auth.init({ sealed: true });

        expect(await auth.unseal()).toBe("unreadable");
        expect(decrypt).not.toHaveBeenCalled();
        expect(mockStore.size).toBe(0);
    });

    it("deletes a login whose stored copy the vault calls damaged", async () => {
        mockStore.set(SEALED_STORAGE_KEY, sealedText("x"));
        const auth = service();
        await auth.init({ sealed: true });
        decrypt.mockRejectedValueOnce(
            Object.assign(new Error("damaged"), { code: "ERR_VAULT_CORRUPT" })
        );

        // or every unlock would try the same bytes again, and the lock would never lift
        expect(await auth.unseal()).toBe("unreadable");
        expect(auth.isSealed()).toBe(false);
        expect(mockStore.size).toBe(0);
    });

    it("start-up stops when the store cannot say whether a sealed login is there", async () => {
        mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("r1")));
        jest.mocked(SecureStore.getItemAsync).mockRejectedValueOnce(
            new Error("keychain")
        );
        const auth = service();

        // taken as "none", the login would look signed out, and the next login would replace it
        await expect(auth.init({ sealed: true })).rejects.toThrow("keychain");
        expect(sealedCopy()?.refreshToken).toBe("r1");

        // the next try finds it
        const retry = service();
        await retry.init({ sealed: true });
        expect(retry.isSealed()).toBe(true);
    });

    it("a lock still waiting on a refresh leaves the next login alone", async () => {
        const auth = await unsealed(session("r1", 0));
        let finish: (token: unknown) => void = () => undefined;
        mockRefreshAsync.mockReturnValue(
            new Promise((resolve) => {
                finish = resolve;
            })
        );
        const refreshing = auth.refreshToken(true).catch(() => undefined);
        const sealing = auth.seal();

        // signed out from the lock screen, and logged in again, before the refresh answers
        jest.spyOn(global, "fetch").mockResolvedValue(new Response("{}"));
        await auth.signOut();
        await (
            auth as unknown as {
                storeTokenResponse: (token: unknown) => Promise<void>;
            }
        ).storeTokenResponse({ ...tokens, refreshToken: "new", idToken: "i" });

        finish(tokens);
        await refreshing;
        await sealing;
        expect(auth.isSealed()).toBe(false);
        expect(auth.getOidcTokenDetails()?.refreshToken).toBe("new");
        jest.restoreAllMocks();
    });

    it("a new login replaces a sealed one this start did not know of", async () => {
        // the lock setting could not be read at this start, so the sealed login was not looked for
        mockStore.set(SEALED_STORAGE_KEY, sealedValue(session("old")));
        const auth = service();
        await auth.init();
        expect(auth.getOidcTokenDetails()).toBeNull();

        await (
            auth as unknown as {
                storeTokenResponse: (token: unknown) => Promise<void>;
            }
        ).storeTokenResponse({ ...tokens, idToken: "i" });

        expect(plainCopy()?.refreshToken).toBe("r2");
        // or the next start, with device security on, would open the old login and delete this one
        expect(sealedCopy()).toBeUndefined();
    });

    describe("sealing as the lock goes on", () => {
        it("takes the tokens out of memory", async () => {
            const auth = await unsealed();
            await auth.seal();
            expect(auth.isSealed()).toBe(true);
            expect(auth.getOidcTokenDetails()).toBeNull();
            expect(sealedCopy()?.refreshToken).toBe("r1");
        });

        it("waits for a refresh on its way and seals its new tokens", async () => {
            const auth = await unsealed();
            let answer: (value: unknown) => void = () => undefined;
            mockRefreshAsync.mockReturnValue(
                new Promise((resolve) => {
                    answer = resolve;
                })
            );
            const refresh = auth.refreshToken(true);
            await new Promise((resolve) => setImmediate(resolve));
            const seal = auth.seal();
            answer(tokens);
            await Promise.all([refresh, seal]);

            expect(auth.getOidcTokenDetails()).toBeNull();
            // a rotated refresh token is not lost: the sealed copy has the new one
            expect(sealedCopy()?.refreshToken).toBe("r2");
            await auth.unseal();
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r2");
        });

        it("keeps the tokens when they could only be stored unsealed", async () => {
            const auth = await unsealed();
            encrypt.mockRejectedValueOnce(new Error("no vault"));
            mockRefreshAsync.mockResolvedValue(tokens);
            await auth.refreshToken(true);

            // the stale sealed copy goes, the new one is stored unsealed and sealed at the next launch
            expect(mockStore.has(SEALED_STORAGE_KEY)).toBe(false);
            expect(mockStore.has(STORAGE_KEY)).toBe(true);
            await auth.seal();
            expect(auth.isSealed()).toBe(false);
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r2");
        });

        it("does nothing with device security off", async () => {
            mockStore.set(STORAGE_KEY, JSON.stringify(session("r1")));
            const auth = service();
            await auth.init();
            await auth.seal();
            expect(auth.isSealed()).toBe(false);
            expect(auth.getOidcTokenDetails()?.refreshToken).toBe("r1");
        });
    });

    describe("turning device security on and off", () => {
        it("moves the stored login to the sealed copy and back", async () => {
            mockStore.set(STORAGE_KEY, JSON.stringify(session("r1")));
            const auth = service();
            await auth.init();

            await auth.setSealed(true);
            expect(sealedCopy()?.refreshToken).toBe("r1");
            expect(mockStore.has(STORAGE_KEY)).toBe(false);

            await auth.setSealed(false);
            expect(mockStore.has(SEALED_STORAGE_KEY)).toBe(false);
            expect(plainCopy()?.refreshToken).toBe("r1");
        });

        it("rejects, and stays off, when the login could not be sealed", async () => {
            mockStore.set(STORAGE_KEY, JSON.stringify(session("r1")));
            const auth = service();
            await auth.init();
            encrypt.mockRejectedValueOnce(new Error("no vault"));

            await expect(auth.setSealed(true)).rejects.toThrow("no vault");
            mockRefreshAsync.mockResolvedValue(tokens);
            await auth.refreshToken(true);
            expect(mockStore.has(SEALED_STORAGE_KEY)).toBe(false);
            expect(plainCopy()?.refreshToken).toBe("r2");
        });

        it("turning it off leaves no unsealed copy when the sealed one could not be removed", async () => {
            const auth = await unsealed();
            jest.mocked(SecureStore.deleteItemAsync).mockRejectedValueOnce(
                new Error("keychain")
            );

            await expect(auth.setSealed(false)).rejects.toThrow("keychain");
            // still on: the move is undone, or the tokens would sit on disk without the key
            expect(plainCopy()).toBeUndefined();
            expect(sealedCopy()?.refreshToken).toBe("r1");
            await auth.seal();
            expect(auth.isSealed()).toBe(true);
        });

        it("a move that could not be undone is cleared up by the next save", async () => {
            const auth = await unsealed(session("r1", 0));
            jest.mocked(SecureStore.deleteItemAsync)
                .mockRejectedValueOnce(new Error("keychain"))
                .mockRejectedValueOnce(new Error("keychain"));

            await expect(auth.setSealed(false)).rejects.toThrow("keychain");
            expect(plainCopy()?.refreshToken).toBe("r1");
            // the tokens stay in memory while it isn't known that only the sealed copy is stored
            await auth.seal();
            expect(auth.isSealed()).toBe(false);

            mockRefreshAsync.mockResolvedValue(tokens);
            await auth.refreshToken(true);
            expect(plainCopy()).toBeUndefined();
            expect(sealedCopy()?.refreshToken).toBe("r2");
        });

        it("a save of new tokens stands when the other copy could not be removed, and the next save clears it", async () => {
            // sealing failed once, so this login is stored unsealed with device security on
            const auth = await unsealed(session("r1", 0));
            encrypt.mockRejectedValueOnce(new Error("no vault"));
            mockRefreshAsync.mockResolvedValueOnce(tokens);
            await auth.refreshToken(true);
            expect(plainCopy()?.refreshToken).toBe("r2");
            expect(sealedCopy()).toBeUndefined();

            // the next refresh seals it, but the unsealed copy can't be removed
            jest.mocked(SecureStore.deleteItemAsync).mockRejectedValueOnce(
                new Error("keychain")
            );
            mockRefreshAsync.mockResolvedValueOnce({
                ...tokens,
                refreshToken: "r3",
            });
            (
                auth as unknown as { session: { expiresAt: number } }
            ).session.expiresAt = 0;
            await auth.refreshToken(true);
            // the new tokens are not undone, or the old ones would be all that is on disk
            expect(sealedCopy()?.refreshToken).toBe("r3");
            expect(plainCopy()?.refreshToken).toBe("r2");

            mockRefreshAsync.mockResolvedValueOnce({
                ...tokens,
                refreshToken: "r4",
            });
            await auth.refreshToken(true);
            expect(sealedCopy()?.refreshToken).toBe("r4");
            expect(plainCopy()).toBeUndefined();
        });

        it("signed out, only the next login is stored to match", async () => {
            const auth = service();
            await auth.init();
            await auth.setSealed(true);
            expect(mockStore.size).toBe(0);
        });
    });

    it("signing out deletes both copies", async () => {
        const auth = await unsealed();
        mockStore.set(STORAGE_KEY, "left over");
        jest.spyOn(global, "fetch").mockResolvedValue(new Response("{}"));
        await auth.signOut();
        expect(mockStore.size).toBe(0);
        jest.restoreAllMocks();
    });
});
