import { afterEach, describe, expect, it, jest } from "@jest/globals";

import { ENVIRONMENTS_URL } from "@/config/environments";
import { fetchReleasedBuild } from "@/services/environments.service";

const answer = (status: number, body: unknown) =>
    jest
        .spyOn(globalThis, "fetch")
        .mockResolvedValue(new Response(JSON.stringify(body), { status }));

describe("fetchReleasedBuild", () => {
    afterEach(() => {
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    it("asks Production's gateway", async () => {
        const fetch = answer(200, { releasedBuild: 452 });
        await fetchReleasedBuild();
        expect(fetch.mock.calls[0]?.[0]).toBe(ENVIRONMENTS_URL);
        expect(ENVIRONMENTS_URL).toBe(
            "https://hg.api.gov.bc.ca/api/mobileservice/environment"
        );
    });

    it.each([
        [452, 452],
        [0, 0],
        [null, null],
    ])("returns %j as %j", async (releasedBuild, expected) => {
        answer(200, { releasedBuild });
        await expect(fetchReleasedBuild()).resolves.toBe(expected);
    });

    it.each([
        ["a negative number", { releasedBuild: -1 }],
        ["a fraction", { releasedBuild: 1.5 }],
        ["a string", { releasedBuild: "452" }],
        ["no field", {}],
        ["not an object", 452],
    ])("throws on %s", async (_name, body) => {
        answer(200, body);
        await expect(fetchReleasedBuild()).rejects.toThrow();
    });

    it.each([429, 500, 404])(
        "throws on a %s (the gateway's rate limit, a server error, no route)",
        async (status) => {
            answer(status, { releasedBuild: 452 });
            await expect(fetchReleasedBuild()).rejects.toThrow();
        }
    );

    it("gives up when the answer is slow", async () => {
        jest.useFakeTimers();
        jest.spyOn(globalThis, "fetch").mockImplementation(
            (_url, init) =>
                new Promise((_resolve, reject) => {
                    init?.signal?.addEventListener("abort", () =>
                        reject(new Error("aborted"))
                    );
                }) as never
        );
        const result = fetchReleasedBuild(3000);
        jest.advanceTimersByTime(3000);
        await expect(result).rejects.toThrow("aborted");
    });
});
