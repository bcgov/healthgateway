import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { readdirSync, readFileSync, statSync } from "fs";
import { join } from "path";

import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import { SessionEndedError } from "@/models/errors";
import { services } from "@/services/container";
import { HttpClient } from "@/services/http/httpClient";
import { describeError, logger } from "@/services/logger";
import { useAuthStore } from "@/stores/auth.store";
import { useConfigStore } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useErrorStore } from "@/stores/error.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { useUserStore } from "@/stores/user.store";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    jsonResponse,
    resetStores,
} from "@/test/fixtures";

const deferred = <T>() => {
    let resolve: (value: T) => void = () => undefined;
    const promise = new Promise<T>((res) => {
        resolve = res;
    });
    return { promise, resolve };
};
const settle = () => new Promise((resolve) => setImmediate(resolve));

/** Their calls do not belong to a login: storage, configuration, and the login itself. */
const ALLOWED_CATCH_FILES = [
    "appLock.store.ts",
    "config.store.ts",
    "connectivity.store.ts",
    "environment.store.ts",
    "onboarding.store.ts",
    "theme.store.ts",
    "communication.service.ts",
    "config.service.ts",
];

describe("answers from a login that has ended", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });
    afterEach(() => {
        jest.restoreAllMocks();
        resetStores();
    });

    it("the HTTP client cancels running calls, which end as cancelled", async () => {
        const http = new HttpClient();
        const answer = deferred<Response>();
        let signal: AbortSignal | undefined;
        jest.spyOn(global, "fetch").mockImplementation((_url, init) => {
            signal = (init as RequestInit).signal ?? undefined;
            return answer.promise;
        });
        const call = http.get("https://x");
        http.endSession();
        expect(signal?.aborted).toBe(true);
        answer.resolve(jsonResponse({ secret: true }));
        await expect(call).rejects.toBeInstanceOf(SessionEndedError);
    });

    it("a cancelled call that fails ends as cancelled, not as an outage", async () => {
        const http = new HttpClient();
        const answer = deferred<Response>();
        jest.spyOn(global, "fetch").mockReturnValue(
            answer.promise.then(() => {
                throw new TypeError("Aborted");
            })
        );
        const call = http.get("https://x");
        http.endSession();
        answer.resolve(jsonResponse({}));
        await expect(call).rejects.toBeInstanceOf(SessionEndedError);
        expect(useConnectivityStore.getState().serviceUnavailable).toBe(false);
    });

    it("a cancelled call answered with an error ends as cancelled", async () => {
        const http = new HttpClient();
        const answer = deferred<Response>();
        jest.spyOn(global, "fetch").mockReturnValue(answer.promise);
        const call = http.get("https://x");
        http.endSession();
        answer.resolve(jsonResponse({ detail: "boom" }, 500));
        await expect(call).rejects.toBeInstanceOf(SessionEndedError);
    });

    it("what waits on a cancelled call can clean up, and the next login starts afresh", async () => {
        const answer = deferred<Response>();
        const fetchMock = jest
            .spyOn(global, "fetch")
            .mockReturnValue(answer.promise);
        const consoleError = jest
            .spyOn(console, "error")
            .mockImplementation(() => undefined);
        logger.setLevel("error");
        let spinnerCleared = false;
        const call = useImmunizationStore
            .getState()
            .retrieveImmunizations("USER-A")
            .catch((error: unknown) => {
                logger.error(describeError(error));
                return error;
            })
            .finally(() => {
                spinnerCleared = true;
            });
        await useAuthStore.getState().signOut();
        answer.resolve(jsonResponse({ detail: "boom" }, 500));
        await expect(call).resolves.toBeInstanceOf(SessionEndedError);
        expect(spinnerCleared).toBe(true);
        expect(consoleError).not.toHaveBeenCalled();
        expect(useErrorStore.getState().errors).toHaveLength(0);
        logger.setLevel("silent");

        fetchMock.mockClear();
        fetchMock.mockReturnValue(new Promise(() => undefined));
        void useImmunizationStore
            .getState()
            .retrieveImmunizations("USER-A")
            .catch(() => undefined);
        await settle();
        expect(fetchMock).toHaveBeenCalledTimes(1);
    });

    it("a cancelled call is no error to report, even handed straight to the error store", () => {
        useErrorStore
            .getState()
            .handleResultError(
                new SessionEndedError(),
                ErrorType.Retrieve,
                ErrorSourceType.User
            );
        expect(useErrorStore.getState().errors).toHaveLength(0);
    });

    it("every catch that could report a failure lets a cancelled call through", () => {
        const files = (dir: string): string[] =>
            readdirSync(dir).flatMap((name) => {
                const path = join(dir, name);
                if (statSync(path).isDirectory()) return files(path);
                return /\.(store|service)\.ts$|createRequestResultStore\.ts$/.test(
                    name
                )
                    ? [path]
                    : [];
            });
        const bad: string[] = [];
        for (const file of [
            ...files(join(process.cwd(), "src/stores")),
            ...files(join(process.cwd(), "src/services")),
        ]) {
            const text = readFileSync(file, "utf8");
            for (const match of text.matchAll(
                /catch \((\w+)(?:: unknown)?\) \{\n([^\n]*)\n/g
            )) {
                const guarded =
                    /rethrowIfSessionEnded|isSessionEnded|ended\(\)|stale\(\)|sessionGeneration\(\)/.test(
                        match[2]
                    );
                if (
                    !guarded &&
                    !ALLOWED_CATCH_FILES.some((f) => file.endsWith(f))
                )
                    bad.push(`${file}: ${match[2].trim().slice(0, 50)}`);
            }
        }
        expect(bad).toEqual([]);
    });

    it("public calls are not cancelled", async () => {
        const http = new HttpClient();
        const answer = deferred<Response>();
        jest.spyOn(global, "fetch").mockReturnValue(answer.promise);
        const call = http.getPublic("https://x");
        http.endSession();
        answer.resolve(jsonResponse({ ok: true }));
        await expect(call).resolves.toEqual({ ok: true });
    });

    const signedOutDuring = async (
        start: () => Promise<unknown>,
        response: Response
    ) => {
        const answer = deferred<Response>();
        jest.spyOn(global, "fetch").mockReturnValue(answer.promise);
        void start().catch(() => undefined);
        await useAuthStore.getState().signOut();
        answer.resolve(response);
        await settle();
    };

    it("user A's records never reach the store after signing out", async () => {
        await signedOutDuring(
            () =>
                useImmunizationStore.getState().retrieveImmunizations("USER-A"),
            jsonResponse({
                resourcePayload: immunizationResultFixture,
                resultStatus: 1,
            })
        );
        expect(
            useImmunizationStore.getState().states["USER-A"]
        ).toBeUndefined();
    });

    it("a late failure writes no error state and no banner", async () => {
        await signedOutDuring(
            () =>
                useImmunizationStore.getState().retrieveImmunizations("USER-A"),
            jsonResponse({ detail: "boom" }, 500)
        );
        expect(
            useImmunizationStore.getState().states["USER-A"]
        ).toBeUndefined();
        expect(useErrorStore.getState().errors).toHaveLength(0);
    });

    it("a late file answer is not stored", async () => {
        await signedOutDuring(
            () =>
                usePatientDataStore
                    .getState()
                    .retrievePatientDataFile("USER-A", "FILE"),
            jsonResponse({ data: "secret" })
        );
        expect(usePatientDataStore.getState().files["FILE"]).toBeUndefined();
    });

    it("a late answer to a profile change does not reach user B", async () => {
        useUserStore.setState({
            user: Object.assign(useUserStore.getState().user, {
                hdid: "USER-A",
            }),
        });
        const answer = deferred<Response>();
        jest.spyOn(global, "fetch").mockReturnValue(answer.promise);
        void useUserStore
            .getState()
            .retrieveProfile()
            .catch(() => undefined);
        useAuthStore.getState().clearStorage();
        useUserStore.setState({
            user: Object.assign(useUserStore.getState().user, {
                hdid: "USER-B",
            }),
        });
        answer.resolve(
            jsonResponse({
                resourcePayload: { hdid: "USER-A", email: "a@a.ca" },
                resultStatus: 1,
            })
        );
        await settle();
        expect(useUserStore.getState().user.hdid).toBe("USER-B");
        expect(useUserStore.getState().user.email).not.toBe("a@a.ca");
    });

    it("late user info does not sign the old login back in", async () => {
        const info = deferred<{ hdid: string }>();
        const auth = fakeAuth({
            idToken: "i",
            accessToken: "a",
            expired: false,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 300,
            hdid: "USER-A",
        });
        auth.getOidcUserInfo = () => info.promise as never;
        services.initialize(configFixture, auth);
        const status = useAuthStore.getState().checkStatus();
        await useAuthStore.getState().signOut();
        info.resolve({ hdid: "USER-A" });
        await expect(status).resolves.toBe(false);
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useUserStore.getState().user.hdid).toBe("");
    });

    it("signing out during a login ends that login as cancelled", async () => {
        const info = deferred<{ hdid: string }>();
        const auth = fakeAuth({
            idToken: "i",
            accessToken: "a",
            expired: false,
            refreshTokenTime: Math.floor(Date.now() / 1000) + 300,
            hdid: "USER-A",
        });
        auth.getOidcUserInfo = () => info.promise as never;
        services.initialize(configFixture, auth);
        const login = useAuthStore.getState().signIn("/");
        await settle();
        await useAuthStore.getState().signOut();
        info.resolve({ hdid: "USER-A" });
        await expect(login).rejects.toBeInstanceOf(SessionEndedError);
        expect(useAuthStore.getState().isAuthenticated).toBe(false);
        expect(useUserStore.getState().user.hdid).toBe("");
    });

    it("user A's patient and profile do not reach user B", async () => {
        const patient = deferred<never>();
        jest.spyOn(services.patient, "getPatient").mockReturnValueOnce(
            patient.promise
        );
        const user = useUserStore.getState();
        const stale = user.retrieveEssentialData();
        useAuthStore.getState().clearStorage();
        patient.resolve({ firstName: "A" } as never);
        await stale;
        const state = useUserStore.getState();
        expect(state.patient).toBeUndefined();
        expect(state.essentialDataLoaded).toBe(false);
    });

    it("a new login does not join the old login's essential data call", async () => {
        const get = jest
            .spyOn(services.patient, "getPatient")
            .mockReturnValue(new Promise(() => undefined));
        void useUserStore.getState().retrieveEssentialData();
        useAuthStore.getState().clearStorage();
        void useUserStore.getState().retrieveEssentialData();
        expect(get).toHaveBeenCalledTimes(2);
    });

    it("a real failure right after signing out is still reported", () => {
        const error = jest
            .spyOn(console, "error")
            .mockImplementation(() => undefined);
        logger.setLevel("error");
        services.http.endSession();
        logger.error("real");
        useErrorStore
            .getState()
            .addError(ErrorType.Retrieve, ErrorSourceType.User);
        expect(error).toHaveBeenCalledTimes(1);
        expect(useErrorStore.getState().errors).toHaveLength(1);
        logger.setLevel("silent");
    });
});
