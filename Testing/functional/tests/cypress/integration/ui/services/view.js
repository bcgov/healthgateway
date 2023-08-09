const { AuthMethod } = require("../../../../support/constants");

const servicesTestsConstants = {
    servicesUrl: "/services",
    unauthorized: "/unauthorized",
};

describe("Authenticated Services View", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );
    });

    it("The url should be the services url if services enabled", () => {
        cy.configureSettings({
            services: {
                enabled: true,
            },
        });
        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });

    it("The url should be the unauthorized url if services is disabled", () => {
        cy.configureSettings({});
        cy.url().should("include", servicesTestsConstants.unauthorized);
    });
});
