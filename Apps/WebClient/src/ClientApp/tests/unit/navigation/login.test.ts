import { afterEach, describe, expect, it } from "@jest/globals";

import { Path } from "@/constants/path";
import type { IdentityProviderConfiguration } from "@/models/configData";
import { loginHref } from "@/navigation/login";
import { useConfigStore } from "@/stores/config.store";
import { configFixture } from "@/test/fixtures";

const bcsc =
    configFixture.webClient.mobile!.authentication!.identityProviders![0];
const keycloak: IdentityProviderConfiguration = {
    id: "KeyCloak",
    name: "KeyCloak",
    icon: "user-secret",
    hint: "",
    disabled: false,
};

/** Native (this project): the mobile list decides. */
const withMobileProviders = (
    identityProviders: IdentityProviderConfiguration[]
) =>
    useConfigStore.setState({
        config: {
            ...configFixture,
            webClient: {
                ...configFixture.webClient,
                mobile: {
                    ...configFixture.webClient.mobile,
                    authentication: {
                        ...configFixture.webClient.mobile!.authentication,
                        identityProviders,
                    },
                },
            },
        },
    });

describe("loginHref (native)", () => {
    afterEach(() => useConfigStore.setState({ config: undefined }));

    it("one provider: the login page, which starts it with no sheet", () => {
        withMobileProviders([bcsc]);
        expect(loginHref()).toBe(Path.Login);
    });

    it("more than one: the login sheet", () => {
        withMobileProviders([bcsc, keycloak]);
        expect(loginHref()).toBe(Path.LoginProviders);
    });

    it("only the enabled providers count", () => {
        withMobileProviders([bcsc, { ...keycloak, disabled: true }]);
        expect(loginHref()).toBe(Path.Login);
    });

    it("carries where to go after the login", () => {
        withMobileProviders([bcsc, keycloak]);
        expect(loginHref(Path.Registration)).toEqual({
            pathname: Path.LoginProviders,
            params: { redirect: Path.Registration },
        });
        withMobileProviders([bcsc]);
        expect(loginHref(Path.Registration)).toEqual({
            pathname: Path.Login,
            params: { redirect: Path.Registration },
        });
    });
});
