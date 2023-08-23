const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";

describe("Federal Proof of Vaccination", () => {
    it("Save Federal Proof of Vaccination", () => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*");

        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
});
