import { deleteDownloadsFolder } from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");
const dashboardUrl = "/dashboard";

function interceptAuthenticatedVaccineStatus() {
    cy.intercept("GET", "**/v1/api/AuthenticatedVaccineStatus/pdf?hdid*", {
        fixture: "ImmunizationService/proofOfVaccination.json",
    });
}

describe("Dashboard - Proof of Vaccination Card", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
        let isLoading = false;
        cy.intercept("GET", "**/v1/api/PublicVaccineStatus/pdf", (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: "ImmunizationService/vaccineProofNotLoaded.json",
                });
            } else {
                req.reply({
                    fixture: "ImmunizationService/vaccineProofLoaded.json",
                });
            }
            isLoading = !isLoading;
        });

        cy.enableModules(["Immunization", "FederalCardButton"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );
    });

    it("Dashboard - Federal Card button - Spinner displayed and download confirmed", () => {
        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.wait(10000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.verifyDownload("VaccineProof.pdf");
    });
});
