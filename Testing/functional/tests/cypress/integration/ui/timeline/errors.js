const { AuthMethod } = require("../../../support/constants");

describe("Banner Error", () => {
    before(() => {
        cy.intercept("GET", "/v1/api/Note/*", (req) => {
            req.reply((res) => {
                res.send({ fixture: "WebClientService/dbError.json" });
            });
        });
        cy.enableModules("Note");

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify banner error", () => {
        cy.get("[data-testid=errorTextDescription]").contains(
            `ClientApp got a Internal Communication error while processing a ${"Data Base"} request.`
        );
        cy.get("[data-testid=errorTextDetails]").contains("Error ABC");
        cy.get("[data-testid=errorSupportDetails]").contains(
            "If this issue persists, contact HealthGateway@gov.bc.ca and provide 123456789"
        );
    });
});
