import { deleteDownloadsFolder } from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");
const dashboardUrl = "/dashboard";

function interceptAuthenticatedVaccineStatus() {
    cy.intercept("GET", "**/v1/api/AuthenticatedVaccineStatus/pdf?hdid*", {
        fixture: "ImmunizationService/proofOfVaccination.json",
    });
}

describe("Dashboard - Proof of Vaccination Card", () => {
    before(() => {
        cy.enableModules(["Immunization", "FederalCardButton"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            dashboardUrl
        );
    });

    beforeEach(() => {
        deleteDownloadsFolder();
    });

    it("Dashboard - Federal Card button - Spinner displayed", () => {
        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
    });

    it("Dashboard - Federal Card button - Download confirmed", () => {
        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        interceptAuthenticatedVaccineStatus();
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.verifyDownload("VaccineProof.pdf");
    });
});
