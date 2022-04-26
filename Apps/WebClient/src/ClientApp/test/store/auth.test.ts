import "@/plugins/inversify.config";

import { User as OidcUser, UserSettings } from "oidc-client";

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
        const settings: UserSettings = {
            id_token: "id_token",
            session_state: "",
            access_token: "access_token",
            refresh_token: "refresh_token",
            token_type: "",
            scope: "test_scope_a test_scope_b",
            profile: {
                name: "User Name",
                iss: "",
                sub: "",
                aud: "",
                exp: 0,
                iat: 0,
            },
            expires_at: 0,
            state: undefined,
        };

        const user: OidcUser = new OidcUser(settings);

        // apply mutation
        mutations.setOidcAuth(state, user);

        // assert result
        expect(state.isAuthenticated).toBe(true);
        expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
        expect(state.authentication.idToken).toBe(settings.id_token);
        expect(state.authentication.accessToken).toBe(settings.access_token);
    });

    test("Sets oidcAuth not authenticated", () => {
        const state = initialState;
        const settings: UserSettings = {
            id_token: "",
            session_state: "",
            access_token: "access_token",
            refresh_token: "refresh_token",
            token_type: "",
            scope: "test_scope",
            profile: {
                name: "User Name",
                iss: "",
                sub: "",
                aud: "",
                exp: 0,
                iat: 0,
            },
            expires_at: 0,
            state: undefined,
        };
        const user: OidcUser = new OidcUser(settings);

        // apply mutation
        mutations.setOidcAuth(state, user);

        // assert result
        expect(state.isAuthenticated).toBe(false);
        expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
        expect(state.authentication.idToken).toBe(settings.id_token);
        expect(state.authentication.accessToken).toBe(settings.access_token);
    });

    test("Unsets oidc data", () => {
        const state = initialState;
        const settings: UserSettings = {
            id_token: "id_token",
            session_state: "",
            access_token: "access_token",
            refresh_token: "refresh_token",
            token_type: "",
            scope: "test_scope",
            profile: {
                name: "User Name",
                iss: "",
                sub: "",
                aud: "",
                exp: 0,
                iat: 0,
            },
            expires_at: 0,
            state: undefined,
        };
        const user: OidcUser = new OidcUser(settings);

        // apply mutation
        mutations.setOidcAuth(state, user);

        // assert result
        expect(state.isAuthenticated).toBe(true);
        expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
        expect(state.authentication.idToken).toBe(settings.id_token);
        expect(state.authentication.accessToken).toBe(settings.access_token);

        mutations.unsetOidcAuth(state);

        expect(state.isAuthenticated).toBe(false);
        expect(state.authentication.scopes).toBe(undefined);
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
