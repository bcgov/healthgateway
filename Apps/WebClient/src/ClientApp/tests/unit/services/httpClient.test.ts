import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { HttpError } from "@/models/errors";
import { HttpClient } from "@/services/http/httpClient";
import { jsonResponse } from "@/test/fixtures";

type FetchMock = jest.Mock<typeof fetch>;
const fetchMock = () => global.fetch as unknown as FetchMock;
const lastInit = () => fetchMock().mock.calls[0][1] as RequestInit;

describe("HttpClient", () => {
    beforeEach(() => {
        global.fetch = jest.fn<typeof fetch>();
    });
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("GETs JSON with an Accept header and no body", async () => {
        fetchMock().mockResolvedValue(jsonResponse({ ok: 1 }));
        const client = new HttpClient();
        await expect(
            client.get<{ ok: number }>("https://api.test/x")
        ).resolves.toEqual({ ok: 1 });
        expect(fetchMock().mock.calls[0][0]).toBe("https://api.test/x");
        expect(lastInit().method).toBe("GET");
        expect(lastInit().body).toBeUndefined();
        expect((lastInit().headers as Record<string, string>).Accept).toBe(
            "application/json"
        );
    });

    it("serialises objects and passes pre-serialised strings through, with a JSON content type", async () => {
        fetchMock().mockResolvedValue(jsonResponse(undefined, 204));
        const client = new HttpClient();
        await client.post("https://api.test/x", { a: 1 });
        expect(lastInit().body).toBe('{"a":1}');
        expect(
            (lastInit().headers as Record<string, string>)["Content-Type"]
        ).toBe("application/json; charset=utf-8");
        await client.put(
            "https://api.test/y",
            JSON.stringify("dr@example.test"),
            {
                "Content-Type": "application/json; charset=utf-8",
            }
        );
        expect((fetchMock().mock.calls[1][1] as RequestInit).body).toBe(
            '"dr@example.test"'
        );
    });

    it("adds and removes the bearer token", async () => {
        fetchMock().mockResolvedValue(jsonResponse(undefined, 204));
        const client = new HttpClient();
        client.setAuthorizationHeader("tok");
        await client.delete("https://api.test/x");
        expect(
            (lastInit().headers as Record<string, string>).Authorization
        ).toBe("Bearer tok");
        client.unsetAuthorizationHeader();
        await client.get("https://api.test/x");
        expect(
            (fetchMock().mock.calls[1][1] as RequestInit).headers
        ).not.toHaveProperty("Authorization");
    });

    it("returns undefined for 204 and empty bodies", async () => {
        fetchMock().mockResolvedValueOnce(jsonResponse(undefined, 204));
        fetchMock().mockResolvedValueOnce(new Response("", { status: 200 }));
        const client = new HttpClient();
        await expect(client.get("https://api.test/x")).resolves.toBeUndefined();
        await expect(client.get("https://api.test/x")).resolves.toBeUndefined();
    });

    it("translates a problem-details error into HttpError with status and traceId", async () => {
        fetchMock().mockResolvedValue(
            jsonResponse({ detail: "Too many", traceId: "trace-1" }, 429)
        );
        const client = new HttpClient();
        const error = await client
            .get("https://api.test/x")
            .catch((e: unknown) => e);
        expect(error).toBeInstanceOf(HttpError);
        expect((error as HttpError).message).toBe("Too many");
        expect((error as HttpError).statusCode).toBe(429);
        expect((error as HttpError).traceId).toBe("trace-1");
    });

    it("falls back to the status text when the error body is not JSON", async () => {
        fetchMock().mockResolvedValue(
            new Response("<html>", { status: 502, statusText: "Bad Gateway" })
        );
        const client = new HttpClient();
        const error = (await client
            .get("https://api.test/x")
            .catch((e: unknown) => e)) as HttpError;
        expect(error.message).toBe("Bad Gateway");
        expect(error.statusCode).toBe(502);
    });

    it("wraps a network failure in HttpError without a status", async () => {
        fetchMock().mockRejectedValue(new TypeError("Network request failed"));
        const client = new HttpClient();
        const error = (await client
            .get("https://api.test/x")
            .catch((e: unknown) => e)) as HttpError;
        expect(error).toBeInstanceOf(HttpError);
        expect(error.statusCode).toBeUndefined();
        expect(error.message).toBe("Network request failed");
    });
});
