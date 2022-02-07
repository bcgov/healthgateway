import { deleteDownloadsFolder } from "../../../support/utils";
const { AuthMethod } = require("../../../support/constants");
const homeUrl = "/home";

describe("Federal Proof of Vaccination", () => {
    it("Save Federal Proof of Vaccination with Retry", () => {
        deleteDownloadsFolder();

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

        cy.verifyDownload("VaccineProof.pdf");
    });
});
