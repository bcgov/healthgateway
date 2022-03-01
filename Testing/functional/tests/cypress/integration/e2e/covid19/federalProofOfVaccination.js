import { deleteDownloadsFolder } from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";

describe("Federal Proof of Vaccination", () => {
    it("Save Federal Proof of Vaccination", () => {
        deleteDownloadsFolder();

        cy.enableModules(["Immunization", "FederalCardButton"]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.intercept("GET", "**/v1/api/AuthenticatedVaccineStatus/pdf?hdid=*");

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
