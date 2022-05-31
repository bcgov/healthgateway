const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";

describe("Federal Proof of Vaccination", () => {
    it("Save Federal Proof of Vaccination with Retry", () => {
        let isLoading = false;
        cy.intercept(
            "GET",
            "**/AuthenticatedVaccineStatus/pdf?hdid*",
            (req) => {
                if (!isLoading) {
                    req.reply({
                        fixture:
                            "ImmunizationService/vaccineProofNotLoaded.json",
                    });
                } else {
                    req.reply({
                        fixture: "ImmunizationService/vaccineProofLoaded.json",
                    });
                }
                isLoading = !isLoading;
            }
        );

        cy.enableModules(["Immunization", "FederalCardButton"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.wait(1000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.verifyDownload("VaccineProof.pdf");
    });

    it("Federal Proof of Vaccination Absent When Disabled", () => {
        cy.enableModules(["Immunization"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("not.exist");
    });
});
