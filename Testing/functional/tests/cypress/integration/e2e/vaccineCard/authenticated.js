const { AuthMethod, localDevUri } = require("../../../support/constants");

describe("Authenticated User - Vaccine Card Page", () => {
    before(() => {
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
    });
    it("Vaccination Card - Partially Vaccinated 2 Valid Doses - Keycloak user", () => {
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
        cy.get("[data-testid=dose-2]").scrollIntoView().should("be.visible");

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
        cy.get("[data-testid=dose-2]").scrollIntoView().should("be.visible");

        // Navigate Right
        cy.get("[data-testid=vr-chevron-right-btn]")
            .should("be.enabled", "be.visible")
            .click();

        // Vaccine Card
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
    });

    it("Vaccination Card - Partially Vaccinated 1 Valid and 2 Invalid Doses - Keycloak user", () => {
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

    it("Vaccination Card - Save Image - Keycloak user", () => {
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
        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Vaccination Card - Save To Wallet - Keycloak user", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/covid19"
        );

        cy.get("[data-testid=save-dropdown-btn] .dropdown-toggle")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=save-to-wallet-dropdown-item]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Vaccination Card - Not Found - Save - Keycloak user", () => {
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
});
