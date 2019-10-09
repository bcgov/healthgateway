import { mutations } from "@/store/modules/auth/mutations";
import { state as initialState } from "@/store/modules/auth/auth";
import { User as OidcUser, UserSettings } from "oidc-client";

describe("Auth mutations", () => {
  test("Sets oidcAuth authenticated", () => {
    let state = initialState;
    let settings: UserSettings = {
      id_token: "id_token",
      session_state: {},
      access_token: "access_token",
      refresh_token: "refresh_token",
      token_type: "",
      scope: "test_scope_a test_scope_b",
      profile: { name: "User Name" },
      expires_at: 0,
      state: undefined
    };

    let user: OidcUser = new OidcUser(settings);

    // apply mutation
    mutations.setOidcAuth(state, user);

    console.log(state.authentication.oidcUser);

    // assert result
    expect(state.isAuthenticated).toBe(true);
    expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
    expect(state.authentication.idToken).toBe(settings.id_token);
    expect(state.authentication.accessToken).toBe(settings.access_token);
    expect(state.authentication.oidcUser).not.toBe(undefined);
    expect(state.authentication.oidcUser!.profile.name).toBe(
      settings.profile.name
    );
  });

  test("Sets oidcAuth not authenticated", () => {
    let state = initialState;
    let settings: UserSettings = {
      id_token: "",
      session_state: {},
      access_token: "access_token",
      refresh_token: "refresh_token",
      token_type: "",
      scope: "test_scope",
      profile: { name: "User Name" },
      expires_at: 0,
      state: undefined
    };
    let user: OidcUser = new OidcUser(settings);

    // apply mutation
    mutations.setOidcAuth(state, user);

    // assert result
    expect(state.isAuthenticated).toBe(false);
    expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
    expect(state.authentication.idToken).toBe(settings.id_token);
    expect(state.authentication.accessToken).toBe(settings.access_token);
    expect(state.authentication.oidcUser).not.toBe(undefined);
    expect(state.authentication.oidcUser!.profile.name).toBe(
      settings.profile.name
    );
  });

  test("Unsets oidc data", () => {
    let state = initialState;
    let settings: UserSettings = {
      id_token: "id_token",
      session_state: {},
      access_token: "access_token",
      refresh_token: "refresh_token",
      token_type: "",
      scope: "test_scope",
      profile: { name: "User Name" },
      expires_at: 0,
      state: undefined
    };
    let user: OidcUser = new OidcUser(settings);

    // apply mutation
    mutations.setOidcAuth(state, user);

    // assert result
    expect(state.isAuthenticated).toBe(true);
    expect(state.authentication.scopes).toEqual(settings.scope.split(" "));
    expect(state.authentication.idToken).toBe(settings.id_token);
    expect(state.authentication.accessToken).toBe(settings.access_token);
    expect(state.authentication.oidcUser).not.toBe(undefined);
    expect(state.authentication.oidcUser!.profile.name).toBe(
      settings.profile.name
    );

    mutations.unsetOidcAuth(state);

    expect(state.isAuthenticated).toBe(false);
    expect(state.authentication.scopes).toBe(undefined);
    expect(state.authentication.idToken).toBe(undefined);
    expect(state.authentication.accessToken).toBe(undefined);
    expect(state.authentication.oidcUser).toBe(undefined);
  });

  test("Sets oidc isChecked", () => {
    let state = initialState;
    // assert result
    expect(state.authentication.isChecked).toBe(false);

    // apply mutation
    mutations.setOidcAuthIsChecked(state);
    expect(state.authentication.isChecked).toBe(true);
  });

  test("Sets oidc error", () => {
    let state = initialState;

    const errorMessage: string = "some error string";

    // apply mutation
    mutations.setOidcError(state, errorMessage);

    // assert result
    expect(state.error).toBe(errorMessage);

    // apply mutation
    const errorObject = { message: errorMessage, otherProp: false };
    mutations.setOidcError(state, errorObject);

    // assert result
    expect(state.error).toBe(errorObject.message);
  });
});
