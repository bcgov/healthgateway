const { AuthMethod, localDevUri } = require("../../../support/constants");

function useImmunizationFixture() {
    cy.intercept("GET", "/v1/api/Immunization", (req) => {
        req.reply((res) => {
            res.send({
                fixture: "ImmunizationService/immunization.json",
            });
        });
    });
}

describe("Immunization", () => {
    before(() => {
        let isLoading = false;
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.intercept("GET", "/v1/api/Immunization", (req) => {
            req.reply((res) => {
                if (!isLoading) {
                    res.send({
                        fixture: "ImmunizationService/immunizationrefresh.json",
                    });
                } else {
                    res.send({
                        fixture: "ImmunizationService/immunization.json",
                    });
                }
                isLoading = !isLoading;
            });
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate Immunization Loading", () => {
        cy.get("[data-testid=immunizationLoading]").should("be.visible");
        cy.get("[data-testid=immunizationLoading]").should("not.exist");
        cy.get("[data-testid=immunizationReady]").should("be.visible");
    });

    it("Validate COVID-19 Immunization Attachment Icons", () => {
        cy.log(
            "All valid COVID-19 immunizations should have attachment icons."
        );
        cy.get("[data-testid=cardBtn]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card)
                    .find("[data-testid=attachmentIcon]")
                    .should("exist");
            });

        cy.log(
            "All cards with attachment icons should be valid COVID-19 immunizations."
        );
        cy.get("[data-testid=attachmentIcon]")
            .closest("[data-testid=timelineCard]")
            .each((card) => {
                cy.wrap(card).find("[data-testid=cardBtn]").should("exist");
            });
    });

    it("Validate Card Details", () => {
        cy.get("[data-testid=cardBtn]")
            .closest("[data-testid=timelineCard]")
            .first()
            .click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProductTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProviderTitle]").should("be.visible");
        cy.get("[data-testid=immunizationLotTitle]").should("be.visible");

        // Verify Forecast
        cy.get("[data-testid=forecastDisplayName]")
            .first()
            .should("be.visible")
            .contains("Covid-19");
        cy.get("[data-testid=forecastDueDate]").first().should("be.visible");
        cy.get("[data-testid=forecastStatus]").first().should("be.visible");
    });

    it("Validate Proof of Immunization Card & Download", () => {
        useImmunizationFixture();
        cy.enableModules([
            "Immunization",
            "VaccinationStatus",
            "VaccinationStatusPdf",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=cardBtn]").first().click({ force: true });
        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");

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

    it("Validate Empty Title", () => {
        cy.enableModules("Immunization");
        cy.intercept("GET", "/v1/api/Immunization", {
            fixture: "ImmunizationService/immunizationEmptyName.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=immunizationTitle]")
            .should("be.visible")
            .should("have.text", "Immunization");
    });

    it("Validate Disabled Header Covid Card", () => {
        cy.intercept("GET", "/v1/api/Immunization", (req) => {
            req.reply((res) => {
                res.send({
                    fixture: "ImmunizationService/immunizationNoRecords.json",
                });
            });
        });
        cy.enableModules("Immunization");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=covidcard-btn]")
            .should("be.visible")
            .should("not.be.enabled");
    });

    it("Validate Header Covid Card", () => {
        useImmunizationFixture();
        cy.enableModules(["Immunization", "VaccinationStatus"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();

        cy.get("[data-testid=covidcard-btn]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
    });
});
