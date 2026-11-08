import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { SessionEndedError } from "@/models/errors";
import { classifyAuthFailure } from "@/services/auth/authFailure";
import {
    MalformedTokenError,
    NativeAuthService,
} from "@/services/auth/authService.impl.native";

const stored = {
    accessToken: "a",
    refreshToken: "r1",
    idToken: "i",
    expiresAt: 0,
};
const mockRefreshAsync = jest.fn<(...args: unknown[]) => Promise<unknown>>();

jest.mock("expo-auth-session", () => ({
    makeRedirectUri: () => "healthgateway://x",
    fetchDiscoveryAsync: async () => ({ userInfoEndpoint: "https://kc/info" }),
    refreshAsync: (...args: unknown[]) => mockRefreshAsync(...args),
}));
jest.mock("expo-web-browser", () => ({}));
jest.mock("expo-secure-store", () => ({
    getItemAsync: jest.fn(async () => JSON.stringify(stored)),
    setItemAsync: jest.fn(async () => undefined),
    deleteItemAsync: jest.fn(async () => undefined),
    WHEN_UNLOCKED_THIS_DEVICE_ONLY: "x",
}));

const start = async () => {
    const service = new NativeAuthService({
        authority: "https://kc",
        clientId: "hg",
        scope: "openid",
    } as never);
    mockRefreshAsync.mockRejectedValueOnce(new TypeError("offline"));
    await service.init({ keepSessionOnFailure: true });
    mockRefreshAsync.mockClear();
    return service;
};
const tokens = { accessToken: "a2", refreshToken: "r2", expiresIn: 300 };

describe("native token refresh", () => {
    beforeEach(() => {
        mockRefreshAsync.mockReset();
    });

    it("two refreshes at once use the refresh token once", async () => {
        const service = await start();
        mockRefreshAsync.mockResolvedValue(tokens);
        await Promise.all([
            service.refreshToken(true),
            service.refreshToken(true),
        ]);
        expect(mockRefreshAsync).toHaveBeenCalledTimes(1);
    });

    it("a later refresh uses the new refresh token", async () => {
        const service = await start();
        mockRefreshAsync.mockResolvedValue(tokens);
        await service.refreshToken(true);
        await service.refreshToken(true);
        expect(mockRefreshAsync).toHaveBeenCalledTimes(2);
        expect(mockRefreshAsync.mock.calls[1][0]).toMatchObject({
            refreshToken: "r2",
        });
    });

    it("an answer arriving after sign-out does not bring the login back", async () => {
        const service = await start();
        let answer: (value: unknown) => void = () => undefined;
        mockRefreshAsync.mockReturnValue(
            new Promise((resolve) => {
                answer = resolve;
            })
        );
        const refresh = service.refreshToken(true);
        await new Promise((resolve) => setImmediate(resolve));
        await service.signOut();
        answer(tokens);
        await refresh;
        expect(service.getOidcTokenDetails()).toBeNull();
    });

    describe("user info answered after signing out", () => {
        const respond = () => {
            let answer: (value: Response) => void = () => undefined;
            jest.spyOn(global, "fetch").mockImplementation((url) =>
                (url as string).includes("info")
                    ? new Promise<Response>((resolve) => {
                          answer = resolve;
                      })
                    : Promise.resolve(new Response("{}"))
            );
            return (body: unknown) =>
                answer(new Response(JSON.stringify(body)));
        };
        afterEach(() => {
            jest.restoreAllMocks();
        });

        it("is not kept for the next login", async () => {
            const service = await start();
            const answer = respond();
            const late = service.getOidcUserInfo();
            await new Promise((resolve) => setImmediate(resolve));
            await service.signOut();
            answer({ hdid: "USER-A" });
            await expect(late).rejects.toBeInstanceOf(SessionEndedError);

            // user B logs in on the same service
            mockRefreshAsync.mockRejectedValueOnce(new TypeError("offline"));
            await service.init({ keepSessionOnFailure: true });
            const answerB = respond();
            const next = service.getOidcUserInfo();
            await new Promise((resolve) => setImmediate(resolve));
            answerB({ hdid: "USER-B" });
            await expect(next).resolves.toEqual({ hdid: "USER-B" });
        });

        it("a token refresh on the way is not a new login", async () => {
            const service = await start();
            const answer = respond();
            const info = service.getOidcUserInfo();
            await new Promise((resolve) => setImmediate(resolve));
            mockRefreshAsync.mockResolvedValue(tokens);
            await service.refreshToken(true);
            answer({ hdid: "USER-A" });
            await expect(info).resolves.toEqual({ hdid: "USER-A" });
        });
    });

    describe("a successful answer holding no usable tokens", () => {
        const broken: [string, unknown][] = [
            ["no access token", { refreshToken: "r2", expiresIn: 300 }],
            ["an empty access token", { accessToken: "", expiresIn: 300 }],
            ["an access token of the wrong type", { accessToken: 7 }],
            [
                "a lifetime of the wrong type",
                { accessToken: "a2", expiresIn: "x" },
            ],
            ["no lifetime left", { accessToken: "a2", expiresIn: 0 }],
        ];

        it.each(broken)("%s keeps the working login", async (_name, answer) => {
            const service = await start();
            mockRefreshAsync.mockResolvedValue(answer);
            await service.refreshToken(true).catch(() => undefined);
            expect(service.getOidcTokenDetails()?.accessToken).toBe("a");
        });

        it.each(broken)("%s at launch is no login", async (_name, answer) => {
            const service = new NativeAuthService({
                authority: "https://kc",
                clientId: "hg",
                scope: "openid",
            } as never);
            mockRefreshAsync.mockResolvedValueOnce(answer);
            await service.init({ keepSessionOnFailure: true });
            expect(service.getRestoreFailure()).toBe("rejected");
        });

        it("counts as unexpected, so repeats end in the usual way", () => {
            expect(classifyAuthFailure(new MalformedTokenError())).toBe(
                "unexpected"
            );
        });
    });

    it.each([
        ["no answer", new TypeError("Network request failed"), "network"],
        [
            "a refusal",
            Object.assign(new Error("x"), { code: "invalid_grant" }),
            "rejected",
        ],
        [
            "a response that makes no sense",
            new SyntaxError("Unexpected token <"),
            "rejected",
        ],
        [
            "bad data",
            new TypeError("Cannot read properties of undefined"),
            "rejected",
        ],
    ])("at launch, %s counts as %s", async (_name, error, expected) => {
        const service = new NativeAuthService({
            authority: "https://kc",
            clientId: "hg",
            scope: "openid",
        } as never);
        mockRefreshAsync.mockRejectedValueOnce(error);
        await service.init({ keepSessionOnFailure: true });
        expect(service.getRestoreFailure()).toBe(expected);
    });
});
