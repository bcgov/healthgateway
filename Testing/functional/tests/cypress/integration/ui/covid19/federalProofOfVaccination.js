import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

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

        setupStandardFixtures();

        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=loadingSpinner]").should("not.exist");
        cy.verifyDownload("VaccineProof.pdf");
    });

    it("Federal Proof of Vaccination Absent When Disabled", () => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: false,
            },
            datasets: [{ name: "immunization", enabled: true }],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homeUrl
        );

        cy.get("[data-testid=proof-vaccination-card-btn]").should("not.exist");
    });
});
