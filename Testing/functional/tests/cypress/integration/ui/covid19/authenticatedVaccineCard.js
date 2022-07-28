const { AuthMethod } = require("../../../support/constants");
const covid19Url = "/covid19";

describe("Authenticated Vaccine Card Downloads", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*", {
            fixture:
                "ImmunizationService/authenticatedVaccinationStatusLoaded.json",
        });
    });

    it("Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus?hdid=*", {
            statusCode: 429,
        });
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
    });

    it("Save Image", () => {
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

    it("Save As PDF Unsuccessful Response: Too Many Requests", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*", {
            statusCode: 429,
        });
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
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=too-many-requests-warning]").should("be.visible");
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
