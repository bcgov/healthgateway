const { AuthMethod } = require("../../../support/constants");
const covid19Url = "/covid19";

describe("Authenticated Vaccine Card", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/v1/api/AuthenticatedVaccineStatus?hdid=*", {
            fixture:
                "ImmunizationService/authenticatedVaccinationStatusLoaded.json",
        });
    });

    it("Save to Wallet Present When WalletExport Enabled", () => {
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "be.visible"
        );
    });

    it("Save to Wallet Absent When WalletExport Disabled", () => {
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle").should(
            "not.exist"
        );
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "not.exist"
        );
        cy.get("[data-testid=save-card-btn]").should("be.visible");
    });

    it("Save As PDF Absent When VaccinationExportPdf Is Disabled", () => {
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]").should("not.exist");
        cy.get("[data-testid=save-to-wallet-dropdown-item]").should(
            "be.visible"
        );
    });
});

describe("Authenticated Vaccine Card Downloads", () => {
    beforeEach(() => {
        cy.deleteDownloadsFolder();
        cy.intercept("GET", "**/v1/api/AuthenticatedVaccineStatus?hdid=*", {
            fixture:
                "ImmunizationService/authenticatedVaccinationStatusLoaded.json",
        });
    });

    it("Save Image When WalletExport Enabled", () => {
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

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

    it("Save Image When WalletExport Disabled", () => {
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-card-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("ProvincialVaccineProof.png");
    });

    it("Save As PDF with Retry", () => {
        let isLoading = false;
        cy.intercept(
            "GET",
            "**/v1/api/AuthenticatedVaccineStatus/pdf?hdid=*",
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
            covid19Url
        );
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        cy.wait(1000);

        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.verifyDownload("ProvincialVaccineProof.pdf");
    });
});
