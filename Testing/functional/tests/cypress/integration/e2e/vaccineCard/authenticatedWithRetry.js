const { AuthMethod } = require("../../../support/constants");
const method = "GET";
const covid19Url = "/covid19";
const fixtureLoadedPath = "ImmunizationService/vaccineProofLoaded.json";
const pathApi =
    "/v1/api/AuthenticatedVaccineStatus/pdf?hdid=P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const fixtureNotLoadedPath = "ImmunizationService/vaccineProofNotLoaded.json";

describe("Vaccination Card - Save as PDF - VaccinationExportPdf is enabled & Loaded is false - KeyCloak", () => {
    before(() => {
        let isLoading = false;
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "VaccinationExportPdf",
        ]);
        cy.intercept(method, pathApi, (req) => {
            if (!isLoading) {
                req.reply({
                    fixture: fixtureNotLoadedPath,
                });
            } else {
                req.reply({ fixture: fixtureLoadedPath });
            }
            isLoading = !isLoading;
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            covid19Url
        );
        cy.checkVaccineRecordHasLoaded();
    });

    it("Vaccine Record Retry", () => {
        cy.log("Load the fixture with loaded is false and retryin is 10000");
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
            "VaccinationExportPdf",
        ]);

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        cy.wait(10000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
    });
});
