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

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });

    it("The url should be the unauthorized url if services is disabled", () => {
        cy.configureSettings({});

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );

        cy.url().should("include", servicesTestsConstants.unauthorized);
    });
});
