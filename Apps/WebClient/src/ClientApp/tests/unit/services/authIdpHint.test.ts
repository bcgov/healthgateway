import { beforeEach, describe, expect, it, jest } from "@jest/globals";
import * as AuthSession from "expo-auth-session";
import Keycloak from "keycloak-js";

import { NativeAuthService } from "@/services/auth/authService.impl.native";

const mockPromptAsync = jest.fn<() => Promise<unknown>>();
const mockLogin = jest.fn<(options: unknown) => Promise<void>>();

jest.mock("expo-auth-session", () => ({
    makeRedirectUri: jest.fn(() => "healthgateway://loginCallback"),
    fetchDiscoveryAsync: jest.fn(async () => ({})),
    AuthRequest: jest.fn(() => ({ promptAsync: mockPromptAsync })),
    ResponseType: { Code: "code" },
    CodeChallengeMethod: { S256: "S256" },
}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(async () => null),
    setItemAsync: jest.fn(),
    deleteItemAsync: jest.fn(),
}));
jest.mock("expo-web-browser", () => ({ warmUpAsync: jest.fn() }));
jest.mock("keycloak-js", () => ({
    __esModule: true,
    default: jest.fn(() => ({
        login: mockLogin,
        authenticated: false,
    })),
}));

const oidc = {
    authority: "https://sso.test/auth/realms/hg",
    clientId: "hg",
    scope: "openid",
    callbacks: null,
};

describe("kc_idp_hint", () => {
    beforeEach(() => {
        jest.clearAllMocks();
        mockPromptAsync.mockResolvedValue({ type: "cancel" });
    });

    const nativeExtraParams = async (hint?: string) => {
        await new NativeAuthService(oidc as never)
            .signIn("/home", hint)
            .catch(() => undefined);
        const options = jest.mocked(AuthSession.AuthRequest).mock
            .calls[0][0] as { extraParams?: unknown };
        return options.extraParams;
    };

    it("native sends the provider's hint", async () => {
        await expect(nativeExtraParams("bcsc")).resolves.toEqual({
            kc_idp_hint: "bcsc",
        });
    });

    it("native sends none for a provider without one", async () => {
        await expect(nativeExtraParams(undefined)).resolves.toEqual({});
    });

    it("web sends the provider's hint", async () => {
        const { createAuthService } = jest.requireActual<{
            createAuthService: (o: unknown) => {
                signIn: (p: string, h?: string) => Promise<unknown>;
            };
        }>("@/services/auth/authService.impl.web");
        await createAuthService({
            ...oidc,
            callbacks: {
                Logon: "https://web.test/loginCallback",
                Logout: "https://web.test/logoutComplete",
            },
        })
            .signIn("/home", "bcsc")
            .catch(() => undefined);
        expect(Keycloak).toHaveBeenCalled();
        expect(mockLogin).toHaveBeenCalledWith(
            expect.objectContaining({ idpHint: "bcsc" })
        );
    });
});
