const { AuthMethod } = require("../../../support/constants");
const covid19Url = "/covid19";

function validateOnCovid19Page() {
    cy.url().should("include", covid19Url);
    cy.get(
        "[data-testid=breadcrumb-covid-19].v-breadcrumbs-item--active"
    ).should("be.visible");
}

describe("Authenticated Vaccine Card", () => {
    it("Validate Partially Vaccinated and PDF download", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
            "getVaccinationStatus"
        );
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*").as(
            "getVaccineProof"
        );

        cy.configureSettings({
            covid19: {
                proofOfVaccination: {
                    exportPdf: true,
                },
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
            covid19Url
        );

        // Check before wait on intercept
        validateOnCovid19Page();

        // Wait for request to complete
        cy.wait("@getVaccinationStatus").then(() => {
            // Vaccine Card
            cy.get("[data-testid=formTitleVaccineCard]", {}).should(
                "be.visible"
            );
            cy.get("[data-testid=statusPartiallyVaccinated]").should(
                "be.visible"
            );

            cy.intercept("GET", "**/Immunization?hdid=*").as("getImmunization");

            // Navigate Left
            cy.get("[data-testid=vc-chevron-left-btn]")
                .should("be.enabled", "be.visible")
                .click();

            // Wait for request to complete
            cy.wait("@getImmunization").then(() => {
                // Vaccination Record
                cy.get("[data-testid=dose-1]").should("be.visible");

                // Navigate Left
                cy.get("[data-testid=vr-chevron-left-btn]")
                    .should("be.enabled", "be.visible")
                    .click();

                // Vaccine Card
                cy.get("[data-testid=formTitleVaccineCard]").should(
                    "be.visible"
                );
                cy.get("[data-testid=statusPartiallyVaccinated]").should(
                    "be.visible"
                );

                // Navigate Right
                cy.get("[data-testid=vc-chevron-right-btn]")
                    .should("be.enabled", "be.visible")
                    .click();

                // Vaccination Record
                cy.get("[data-testid=dose-1]").should("be.visible");

                // Navigate Right
                cy.get("[data-testid=vr-chevron-right-btn]")
                    .should("be.enabled", "be.visible")
                    .click();

                // Vaccine Card
                cy.get("[data-testid=formTitleVaccineCard]").should(
                    "be.visible"
                );
                cy.get("[data-testid=statusPartiallyVaccinated]").should(
                    "be.visible"
                );

                // Click on Save button
                cy.get("[data-testid=save-dropdown-btn]")
                    .should("be.enabled", "be.visible")
                    .click();

                // Click on PDF option
                cy.get("[data-testid=save-as-pdf-dropdown-item]")
                    .should("be.visible")
                    .click();

                cy.get("[data-testid=generic-message-modal]").should(
                    "be.visible"
                );
                cy.get("[data-testid=generic-message-submit-btn]").click();

                cy.wait("@getVaccineProof");
                cy.verifyDownload("ProvincialVaccineProof.pdf", {
                    timeout: 60000,
                    interval: 5000,
                });
            });
        });
    });

    it("Save Button Absent When Status Is Not Found", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
            "getVaccinationStatus"
        );

        cy.configureSettings({
            covid19: {
                proofOfVaccination: {
                    exportPdf: true,
                },
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        cy.login(
            Cypress.env("keycloak.notfound.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );

        // Check before wait on intercept
        validateOnCovid19Page();

        cy.wait("@getVaccinationStatus").then(() => {
            cy.get("[data-testid=statusNotFound]").should("be.visible");
            cy.get("[data-testid=save-dropdown-btn]").should("not.exist");
        });
    });
});
