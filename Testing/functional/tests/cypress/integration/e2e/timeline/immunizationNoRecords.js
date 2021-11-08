const { AuthMethod, localDevUri } = require("../../../support/constants");
describe("Immunization No Records", () => {
    beforeEach(() => {
        let isLoading = false;
        cy.enableModules("Immunization");
        cy.intercept("GET", "**/v1/api/Immunization?*", (req) => {
            req.reply((res) => {
                if (!isLoading) {
                    res.send({
                        fixture: "ImmunizationService/immunizationrefresh.json",
                    });
                } else {
                    res.send({
                        fixture:
                            "ImmunizationService/immunizationNoRecords.json",
                    });
                }
                isLoading = !isLoading;
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Immunization Loading", () => {
        cy.get("[data-testid=immunizationLoading]").should("be.visible");
        cy.get("[data-testid=immunizationLoading]").should("not.exist");
        cy.get("[data-testid=immunizationEmpty]").should("be.visible");
    });
});
