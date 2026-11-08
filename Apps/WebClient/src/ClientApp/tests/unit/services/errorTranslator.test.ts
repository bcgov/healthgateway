import { describe, expect, it } from "@jest/globals";

import { ServiceCode } from "@/constants/errorType";
import {
    HttpError,
    isTooManyRequestsError,
    ResultError,
} from "@/models/errors";
import { toResultError } from "@/services/errorTranslator";

describe("toResultError", () => {
    it("passes a ResultError through unchanged", () => {
        const e = new ResultError("ClientApp-CI-HGU", "x", 400, "t");
        expect(toResultError(e, ServiceCode.Patient)).toBe(e);
    });

    it("maps HttpError to the service's client error code with status and traceId", () => {
        const r = toResultError(
            new HttpError("Nope", 409, "trace"),
            ServiceCode.HealthGatewayUser
        );
        expect(r).toBeInstanceOf(ResultError);
        expect(r.errorCode).toBe("ClientApp-CI-HGU");
        expect(r.message).toBe("Nope");
        expect(r.statusCode).toBe(409);
        expect(r.traceId).toBe("trace");
    });

    it("gives an empty HttpError message the generic text", () => {
        expect(
            toResultError(new HttpError(""), ServiceCode.Patient).message
        ).toBe("An unexpected error occurred.");
    });

    it("wraps plain errors and non-errors", () => {
        expect(
            toResultError(new Error("boom"), ServiceCode.Patient).errorCode
        ).toBe("ClientApp-CI-PAT");
        expect(toResultError("??", ServiceCode.Patient).message).toBe(
            "An unexpected error occurred."
        );
    });

    it("recognises 429 on both error types", () => {
        expect(isTooManyRequestsError(new HttpError("x", 429))).toBe(true);
        expect(
            isTooManyRequestsError(
                new ResultError("ClientApp-CI-HGU", "x", 429)
            )
        ).toBe(true);
        expect(
            isTooManyRequestsError(
                new ResultError("ClientApp-CI-HGU", "x", 500)
            )
        ).toBe(false);
        expect(isTooManyRequestsError(new Error("x"))).toBe(false);
    });
});
