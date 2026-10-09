import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const servicesTestsConstants = {
    servicesUrl: "/services",
    unauthorized: "/unauthorized",
};

describe("Authenticated Services View", () => {
    beforeEach(() => {
        setupStandardFixtures();
    });

    it("The url should be the services url if services enabled", () => {
        cy.configureSettings({
            services: {
                enabled: true,
            },
        });

        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/services",
                    "default",
                    { cacheAcrossSpecs: true }
                );
            }
        );

        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });

    it("The url should be the unauthorized url if services is disabled", () => {
        cy.configureSettings({});

        cy.env(["keycloak.password"]).then(
            ({ "keycloak.password": password }) => {
                cy.login(
                    Cypress.expose("keycloak.username"),
                    password,
                    AuthMethod.KeyCloak,
                    "/services",
                    "default",
                    { cacheAcrossSpecs: true }
                );
            }
        );

        cy.url().should("include", servicesTestsConstants.unauthorized);
    });
});
