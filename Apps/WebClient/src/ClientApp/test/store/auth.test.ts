import "@/plugins/inversify.config";

import { OidcTokenDetails } from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { state as initialState } from "@/store/modules/auth/auth";
import { mutations } from "@/store/modules/auth/mutations";

describe("Auth mutations", () => {
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    logger.initialize("info");

    test("Sets authenticated", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "id_token",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            refreshTokenTime: 0,
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setAuthenticated(state, details);

        // assert result
        expect(state.tokenDetails?.idToken).toBe(details.idToken);
        expect(state.tokenDetails?.accessToken).toBe(details.accessToken);
        expect(state.tokenDetails?.refreshToken).toBe(details.refreshToken);
        expect(state.tokenDetails?.refreshTokenTime).toBe(
            details.refreshTokenTime
        );
        expect(state.tokenDetails?.expired).toBe(details.expired);
        expect(state.tokenDetails?.hdid).toBe(details.hdid);
    });

    test("Sets authenticated (invalid)", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            refreshTokenTime: 0,
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setAuthenticated(state, details);

        // assert result
        expect(state.tokenDetails?.idToken).toBe(details.idToken);
        expect(state.tokenDetails?.accessToken).toBe(details.accessToken);
        expect(state.tokenDetails?.refreshToken).toBe(details.refreshToken);
        expect(state.tokenDetails?.refreshTokenTime).toBe(
            details.refreshTokenTime
        );
        expect(state.tokenDetails?.expired).toBe(details.expired);
        expect(state.tokenDetails?.hdid).toBe(details.hdid);
    });

    test("Sets unauthenticated", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "id_token",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            refreshTokenTime: 0,
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setAuthenticated(state, details);

        // assert result
        expect(state.tokenDetails?.idToken).toBe(details.idToken);
        expect(state.tokenDetails?.accessToken).toBe(details.accessToken);
        expect(state.tokenDetails?.refreshToken).toBe(details.refreshToken);
        expect(state.tokenDetails?.refreshTokenTime).toBe(
            details.refreshTokenTime
        );
        expect(state.tokenDetails?.expired).toBe(details.expired);
        expect(state.tokenDetails?.hdid).toBe(details.hdid);

        mutations.setUnauthenticated(state);

        expect(state.tokenDetails).toBe(undefined);
    });

    test("Sets error", () => {
        const state = initialState;

        const errorMessage = "some error string";

        // apply mutation
        mutations.setError(state, errorMessage);

        // assert result
        expect(state.error).toBe(errorMessage);

        // apply mutation
        const errorObject = { message: errorMessage, otherProp: false };
        mutations.setError(state, errorObject);

        // assert result
        expect(state.error).toBe(errorObject);
    });
});
