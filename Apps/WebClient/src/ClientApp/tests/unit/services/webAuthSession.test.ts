import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import type { KeycloakInitOptions } from "keycloak-js";

import { WebAuthService } from "@/services/auth/authService.impl.web";
import { clearStoredSession } from "@/services/auth/storedSession.web";

type InitResult = {
    resolve: (value: boolean) => void;
    reject: (error: unknown) => void;
};

const mockInit = jest.fn<(options: KeycloakInitOptions) => Promise<boolean>>();
const mockLogout = jest.fn<() => Promise<void>>(async () => undefined);
const mockKeycloak: Record<string, unknown> = {};

jest.mock("keycloak-js", () => ({
    __esModule: true,
    default: jest.fn(() => mockKeycloak),
}));

class MemoryStorage implements Storage {
    private items = new Map<string, string>();
    get length() {
        return this.items.size;
    }
    clear() {
        this.items.clear();
    }
    getItem(key: string) {
        return this.items.get(key) ?? null;
    }
    key(index: number) {
        return [...this.items.keys()][index] ?? null;
    }
    removeItem(key: string) {
        this.items.delete(key);
    }
    setItem(key: string, value: string) {
        this.items.set(key, String(value));
    }
    entries() {
        return [...this.items.entries()];
    }
}

const oidc = {
    authority: "https://sso.test/auth/realms/hg",
    clientId: "hg",
    scope: "openid",
    callbacks: null,
};

const HAD_SESSION = "hg.auth.hadSession";
const SSO_REDIRECT = "hg.auth.ssoRedirectAt";
const NOW = 1_800_000_000_000;

let local: MemoryStorage;
let session: MemoryStorage;
let pending: InitResult;

/** Starts init and lets keycloak-js's init settle (or not) under the test's control. */
function startInit(service: WebAuthService) {
    const done = service.init();
    return { done, keycloakInit: pending };
}

/** Lets the settle handlers and init's own await run. */
async function flush() {
    for (let i = 0; i < 5; i++) await Promise.resolve();
}

describe("web auth session (tokens in memory only)", () => {
    beforeEach(() => {
        jest.useFakeTimers({ now: NOW });
        local = new MemoryStorage();
        session = new MemoryStorage();
        Object.defineProperty(globalThis, "localStorage", {
            value: local,
            configurable: true,
        });
        Object.defineProperty(globalThis, "sessionStorage", {
            value: session,
            configurable: true,
        });
        Object.defineProperty(globalThis, "location", {
            value: { origin: "https://app.test" },
            configurable: true,
        });
        for (const key of Object.keys(mockKeycloak)) delete mockKeycloak[key];
        mockInit.mockReset();
        mockInit.mockImplementation(
            () =>
                new Promise<boolean>((resolve, reject) => {
                    pending = { resolve, reject };
                })
        );
        Object.assign(mockKeycloak, {
            init: mockInit,
            logout: mockLogout,
            authenticated: false,
            clearToken: jest.fn(() =>
                (mockKeycloak.onAuthLogout as () => void)()
            ),
        });
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    it("never hands keycloak-js stored tokens, and removes those an earlier build left", async () => {
        session.setItem(
            "hg.oidc.web.tokens",
            JSON.stringify({ token: "a", refreshToken: "r" })
        );
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        keycloakInit.resolve(false);
        await done;

        const options = mockInit.mock.calls[0][0];
        expect(options).not.toHaveProperty("token");
        expect(options).not.toHaveProperty("refreshToken");
        expect(options).not.toHaveProperty("idToken");
        expect(session.getItem("hg.oidc.web.tokens")).toBeNull();
    });

    it("stays on the page for a browser that was not signed in", async () => {
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        expect(mockInit.mock.calls[0][0].silentCheckSsoFallback).toBe(false);
        expect(session.getItem(SSO_REDIRECT)).toBeNull();
        keycloakInit.resolve(false);
        await done;
        expect(local.getItem(HAD_SESSION)).toBeNull();
    });

    it("lets keycloak-js redirect for a browser that was signed in, and marks the tab before it can leave", async () => {
        local.setItem(HAD_SESSION, "true");
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        expect(mockInit.mock.calls[0][0].silentCheckSsoFallback).toBe(true);
        expect(session.getItem(SSO_REDIRECT)).toBe(String(NOW));

        mockKeycloak.authenticated = true;
        keycloakInit.resolve(true);
        await done;
        expect(session.getItem(SSO_REDIRECT)).toBeNull();
        expect(local.getItem(HAD_SESSION)).toBe("true");
    });

    it("keeps the mark while the page is leaving for Keycloak", async () => {
        local.setItem(HAD_SESSION, "true");
        const { done } = startInit(new WebAuthService(oidc));
        // keycloak-js navigated away: its init never settles
        jest.advanceTimersByTime(5000);
        await done;
        expect(session.getItem(SSO_REDIRECT)).toBe(String(NOW));
    });

    it("does not redirect again when the tab redirected moments ago", async () => {
        local.setItem(HAD_SESSION, "true");
        session.setItem(SSO_REDIRECT, String(NOW - 2000));
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        expect(mockInit.mock.calls[0][0].silentCheckSsoFallback).toBe(false);

        // came back without its callback: Keycloak's answer is lost, so this load is signed out and the hint goes
        keycloakInit.resolve(false);
        await done;
        expect(session.getItem(SSO_REDIRECT)).toBeNull();
        expect(local.getItem(HAD_SESSION)).toBeNull();
    });

    it("redirects again once an old mark has expired", () => {
        local.setItem(HAD_SESSION, "true");
        session.setItem(SSO_REDIRECT, String(NOW - 60_000));
        startInit(new WebAuthService(oidc));
        expect(mockInit.mock.calls[0][0].silentCheckSsoFallback).toBe(true);
    });

    it("forgets the session when Keycloak answers that there is none", async () => {
        local.setItem(HAD_SESSION, "true");
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        keycloakInit.resolve(false); // prompt=none came back with login_required
        await done;
        expect(local.getItem(HAD_SESSION)).toBeNull();
    });

    it("keeps the hint when Keycloak could not be asked", async () => {
        local.setItem(HAD_SESSION, "true");
        const { done, keycloakInit } = startInit(new WebAuthService(oidc));
        keycloakInit.reject(new Error("network"));
        await done;
        await flush();
        expect(local.getItem(HAD_SESSION)).toBe("true");
        expect(session.getItem(SSO_REDIRECT)).toBeNull();
    });

    it("never writes a token to browser storage", async () => {
        const service = new WebAuthService(oidc);
        const { done, keycloakInit } = startInit(service);
        Object.assign(mockKeycloak, {
            authenticated: true,
            token: "access-token",
            refreshToken: "refresh-token",
            idToken: "id-token",
        });
        (mockKeycloak.onAuthSuccess as () => void)();
        (mockKeycloak.onAuthRefreshSuccess as () => void)();
        keycloakInit.resolve(true);
        await done;

        const stored = JSON.stringify([
            ...local.entries(),
            ...session.entries(),
        ]);
        expect(stored).not.toMatch(/access-token|refresh-token|id-token/);
        expect(local.getItem(HAD_SESSION)).toBe("true");
    });

    it("forgets the session on sign-out, on a cleared login and on Reset App", async () => {
        const service = new WebAuthService(oidc);
        local.setItem(HAD_SESSION, "true");
        await service.signOut();
        expect(local.getItem(HAD_SESSION)).toBeNull();

        local.setItem(HAD_SESSION, "true");
        service.clearState();
        expect(local.getItem(HAD_SESSION)).toBeNull();

        local.setItem(HAD_SESSION, "true");
        session.setItem(SSO_REDIRECT, String(NOW));
        session.setItem("hg.oidc.web.tokens", "{}");
        await clearStoredSession();
        expect([...local.entries(), ...session.entries()]).toEqual([]);
    });
});
