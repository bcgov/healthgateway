import { describe, expect, it } from "@jest/globals";

import { decodeJwtPayload } from "@/services/auth/jwt";

const b64url = (o: unknown) =>
    Buffer.from(JSON.stringify(o))
        .toString("base64")
        .replace(/\+/g, "-")
        .replace(/\//g, "_")
        .replace(/=+$/, "");
const token = (payload: unknown) => `header.${b64url(payload)}.sig`;

describe("decodeJwtPayload", () => {
    it("decodes base64url claims including non-ASCII names", () => {
        expect(
            decodeJwtPayload(token({ hdid: "H1", given_name: "Zoë" }))
        ).toEqual({ hdid: "H1", given_name: "Zoë" });
    });

    it("returns undefined for missing, malformed or non-JSON tokens", () => {
        expect(decodeJwtPayload(undefined)).toBeUndefined();
        expect(decodeJwtPayload("nodots")).toBeUndefined();
        expect(decodeJwtPayload("a.%%%.c")).toBeUndefined();
    });

    it("works without atob (Hermes builds that lack it)", () => {
        const saved = global.atob;
        // @ts-expect-error simulate a runtime without atob
        delete global.atob;
        try {
            expect(decodeJwtPayload(token({ sub: "abc", n: 1 }))).toEqual({
                sub: "abc",
                n: 1,
            });
            expect(
                decodeJwtPayload(token({ given_name: "Zoë", family: "Đặng" }))
            ).toEqual({ given_name: "Zoë", family: "Đặng" });
        } finally {
            global.atob = saved;
        }
    });
});
