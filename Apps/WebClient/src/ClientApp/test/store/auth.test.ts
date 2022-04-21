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

    test("Sets oidcAuth authenticated", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "id_token",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setOidcAuth(state, details);

        // assert result
        expect(state.isAuthenticated).toBe(true);
        expect(state.authentication.idToken).toBe(details.idToken);
        expect(state.authentication.accessToken).toBe(details.accessToken);
    });

    test("Sets oidcAuth not authenticated", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setOidcAuth(state, details);

        // assert result
        expect(state.isAuthenticated).toBe(false);
        expect(state.authentication.idToken).toBe(details.idToken);
        expect(state.authentication.accessToken).toBe(details.accessToken);
    });

    test("Unsets oidc data", () => {
        const state = initialState;

        const details: OidcTokenDetails = {
            idToken: "id_token",
            accessToken: "access_token",
            refreshToken: "refresh_token",
            expired: false,
            hdid: "",
        };

        // apply mutation
        mutations.setOidcAuth(state, details);

        // assert result
        expect(state.isAuthenticated).toBe(true);
        expect(state.authentication.idToken).toBe(details.idToken);
        expect(state.authentication.accessToken).toBe(details.accessToken);

        mutations.unsetOidcAuth(state);

        expect(state.isAuthenticated).toBe(false);
        expect(state.authentication.idToken).toBe(undefined);
        expect(state.authentication.accessToken).toBe(undefined);
    });

    test("Sets oidc isChecked", () => {
        const state = initialState;
        // assert result
        expect(state.authentication.isChecked).toBe(false);

        // apply mutation
        mutations.setOidcAuthIsChecked(state);
        expect(state.authentication.isChecked).toBe(true);
    });

    test("Sets oidc error", () => {
        const state = initialState;

        const errorMessage = "some error string";

        // apply mutation
        mutations.setOidcError(state, errorMessage);

        // assert result
        expect(state.error).toBe(errorMessage);

        // apply mutation
        const errorObject = { message: errorMessage, otherProp: false };
        mutations.setOidcError(state, errorObject);

        // assert result
        expect(state.error).toBe(errorObject);
    });
});
