const { AuthMethod } = require("../../../support/constants");
const covid19Url = "/covid19";

describe("Authenticated Vaccine Card", () => {
    it("Partially Vaccinated with 1 Valid Dose", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
            "getVaccinationStatus"
        );

        cy.enableModules(["Immunization"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );

        // Wait for request to complete
        cy.wait("@getVaccinationStatus");

        // Vaccine Card
        cy.get("[data-testid=formTitleVaccineCard]", { timeout: 60000 }).should(
            "be.visible"
        );
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");

        cy.intercept("GET", "**/Immunization?hdid=*").as("getImmunization");

        // Navigate Left
        cy.get("[data-testid=vc-chevron-left-btn]")
            .should("be.enabled", "be.visible")
            .click();

        // Wait for request to complete
        cy.wait("@getImmunization");

        // Vaccination Record
        cy.get("[data-testid=dose-1]", { timeout: 60000 }).should("be.visible");

        // Navigate Left
        cy.get("[data-testid=vr-chevron-left-btn]")
            .should("be.enabled", "be.visible")
            .click();

        // Vaccine Card
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");

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
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
    });

    it("Save Button Absent When Status Is Not Found", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
            "getVaccinationStatus"
        );

        cy.enableModules(["Immunization"]);
        cy.login(
            Cypress.env("keycloak.notfound.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );

        cy.wait("@getVaccinationStatus");

        cy.get("[data-testid=statusNotFound]", { timeout: 60000 }).should(
            "be.visible"
        );
        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle").should(
            "not.exist"
        );
    });

    it("Save As PDF", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*").as(
            "getVaccinationStatus"
        );
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*").as(
            "getVaccineProof"
        );

        cy.enableModules(["Immunization", "VaccinationExportPdf"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );

        cy.wait("@getVaccinationStatus");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle", {
            timeout: 60000,
        })
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.wait("@getVaccineProof");
        cy.verifyDownload("ProvincialVaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });
});
