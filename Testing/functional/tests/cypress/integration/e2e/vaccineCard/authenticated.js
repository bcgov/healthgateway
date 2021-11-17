import { deleteDownloadsFolder } from "../../../support/utils";
const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Authenticated User - Vaccine Card Page", () => {
    beforeEach(() => {
        deleteDownloadsFolder();
    });

    it("Vaccination Card - Partially Vaccinated 1 Valid Dose - Keycloak", () => {
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        // Vaccine Card
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");

        // Navigate Left
        cy.get("[data-testid=vc-chevron-left-btn]")
            .should("be.enabled", "be.visible")
            .click();

        // Vaccination Record
        cy.get("[data-testid=dose-1]").should("be.visible");

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

    it("Vaccination Card - Partially Vaccinated 1 Valid and 2 Invalid Doses - Keycloak", () => {
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.invaliddoses.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");

        // Navigate Left
        cy.get("[data-testid=vc-chevron-left-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=dose-1]").should("be.visible");
        cy.get("[data-testid=dose-2]").should("not.exist");
        cy.get("[data-testid=dose-3]").should("not.exist");
    });

    it("Vaccination Card - Save Image - Wallet Export Enabled - Keycloak", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "WalletExport",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-image-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });

    it("Vaccination Card - Not Found - Save - Keycloak", () => {
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.notfound.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=statusNotFound]").should("be.visible");
        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle").should(
            "not.exist"
        );
    });

    it("Vaccination Card - Save To Wallet - Wallet Export Enabled - Keycloak", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "WalletExport",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "be.visible"
        );
    });

    it("Vaccination Card - Save To Wallet - Wallet Export Disabled - Keycloak", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle").should(
            "not.exist"
        );
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "not.exist"
        );
        cy.get("[data-testid=save-card-btn]").should("be.visible");
    });

    it("Vaccination Card - Save Image - Wallet Export Disabled - Keycloak", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-card-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });

    it("Vaccination Card - Save as PDF - VaccinationExportPdf is disabled - KeyCloak", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "WalletExport",
        ]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]").should("not.exist");
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "be.visible"
        );
    });

    it("Vaccination Card - Save as PDF - VaccinationExportPdf is enabled - KeyCloak ", () => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "VaccinationExportPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.pdf");
    });
});
