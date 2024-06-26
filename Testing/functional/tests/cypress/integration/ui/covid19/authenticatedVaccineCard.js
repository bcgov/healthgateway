import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const covid19Url = "/covid19";

describe("Authenticated Vaccine Card Downloads", () => {
    beforeEach(() => {
        setupStandardFixtures();

        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*", {
            fixture:
                "ImmunizationService/authenticatedVaccinationStatusLoaded.json",
        });
    });

    it("Save Image", () => {
        cy.configureSettings({
            datasets: [{ name: "immunization", enabled: true }],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-card-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });

    it("Save As PDF with Retry", () => {
        let isLoading = false;
        cy.intercept(
            "GET",
            "**/AuthenticatedVaccineStatus/pdf?hdid=*",
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
        cy.configureSettings({
            covid19: {
                proofOfVaccination: {
                    exportPdf: true,
                },
            },
            datasets: [{ name: "immunization", enabled: true }],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );
        cy.get("[data-testid=loadingSpinner]").should("not.exist");

        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.get("[data-testid=loadingSpinner]").should("not.exist");

        cy.verifyDownload("ProvincialVaccineProof.pdf");
    });
});
