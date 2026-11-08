import { describe, expect, it } from "@jest/globals";

import { classifyAuthFailure } from "@/services/auth/authFailure";

const coded = (code: string) => Object.assign(new Error("x"), { code });

describe("classifyAuthFailure", () => {
    it.each([
        ["a refused refresh token", coded("invalid_grant"), "refused"],
        [
            "no answer, React Native",
            new TypeError("Network request failed"),
            "connection",
        ],
        [
            "no answer, a browser",
            new TypeError("Failed to fetch"),
            "connection",
        ],
        ["no answer, Safari", new TypeError("Load failed"), "connection"],
        ["another OAuth error", coded("invalid_client"), "unexpected"],
        ["a server error from Keycloak", coded("server_error"), "unexpected"],
        [
            "an HTML error page instead of JSON",
            new SyntaxError("Unexpected token < in JSON"),
            "unexpected",
        ],
        [
            "a TypeError from bad data, not the network",
            new TypeError("Cannot read properties of undefined"),
            "unexpected",
        ],
        ["a thrown string", "boom", "unexpected"],
        ["nothing", undefined, "unexpected"],
    ])("%s", (_name, error, expected) => {
        expect(classifyAuthFailure(error)).toBe(expected);
    });
});
