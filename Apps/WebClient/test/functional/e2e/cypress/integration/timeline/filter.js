const { AuthMethod, localDevUri } = require("../../support/constants");

describe("Filters", () => {
    before(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.closeCovidModal();
    });

    it("Validate Filter Counts", () => {
        const countRegex = /^.*?\((\d+)K?\).*$/;
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ImmunizationCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=MedicationCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=LaboratoryCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=EncounterCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=NoteCount]")
            .should("be.visible")
            .contains(countRegex);
    });

    it("No Records on Linear Timeline", () => {
        cy.get("[data-testid=filterTextInput]").type("xxxx");
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });

    it("No Records on Calendar Timeline", () => {
        cy.get("[data-testid=monthViewToggle]").first().click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=filterTextInput]").clear();
        cy.get("[data-testid=listViewToggle]").last().click();
    });

    it("Filter Checkboxes are Visible", () => {
        cy.get('[data-testid="filterDropdown"]').click();
        cy.get("[data-testid=Note-filter]").should("be.visible");
        cy.get("[data-testid=Medication-filter]").should("be.visible");
        cy.get("[data-testid=Immunization-filter]").should("be.visible");
        cy.get("[data-testid=Laboratory-filter]").should("be.visible");
        cy.get("[data-testid=Encounter-filter]").should("be.visible");
    });

    it("Filter Immunization", () => {
        if (Cypress.config().baseUrl != localDevUri) {
            cy.get("[data-testid=Immunization-filter]").click({ force: true });
            cy.get("[data-testid=immunizationTitle]").should("be.visible");
            cy.get("[data-testid=noteTitle]").should("not.be.visible");
            cy.get("[data-testid=encounterTitle]").should("not.be.visible");
            cy.get("[data-testid=laboratoryTitle]").should("not.be.visible");
            cy.get("[data-testid=medicationTitle]").should("not.be.visible");
            cy.get('[data-testid="filterDropdown"]').contains("Clear").click();
        }
        else {
            cy.log("Skipped Filter Immunization as running locally")
        }
    });

    it("Filter Medication", () => {
        cy.get("[data-testid=Medication-filter]").click({ force: true });
        cy.get("[data-testid=immunizationTitle]").should("not.be.visible");
        cy.get("[data-testid=noteTitle]").should("not.be.visible");
        cy.get("[data-testid=encounterTitle]").should("not.be.visible");
        cy.get("[data-testid=laboratoryTitle]").should("not.be.visible");
        cy.get("[data-testid=medicationTitle]").should("be.visible");
        cy.get('[data-testid="filterDropdown"]').contains("Clear").click();
    });

    it("Filter Encounter", () => {
        cy.get("[data-testid=Encounter-filter]").click({ force: true });
        cy.get("[data-testid=encounterTitle]").should("be.visible");
        cy.get("[data-testid=noteTitle]").should("not.be.visible");
        cy.get("[data-testid=immunizationTitle]").should("not.be.visible");
        cy.get("[data-testid=laboratoryTitle]").should("not.be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.be.visible");
        cy.get('[data-testid="filterDropdown"]').contains("Clear").click();
    });

    it("Filter Laboratory", () => {
        cy.get("[data-testid=Laboratory-filter]").click({ force: true });
        cy.get("[data-testid=encounterTitle]").should("not.be.visible");
        cy.get("[data-testid=noteTitle]").should("not.be.visible");
        cy.get("[data-testid=immunizationTitle]").should("not.be.visible");
        cy.get("[data-testid=laboratoryTitle]").should("be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.be.visible");
        cy.get('[data-testid="filterDropdown"]').contains("Clear").click();
    });

    it("Page size", () => {
        cy.get("[data-testid=linearTimelineData")
            .find("[data-testid=timelineCard]")
            .its("length")
            .should("eq", 25);

        cy.get("#entries-per-page")
            .as("range")
            .invoke("val", 1)
            .trigger("change");
        cy.get("[data-testid=linearTimelineData]")
            .find("[data-testid=timelineCard]")
            .its("length")
            .should("eq", 50);

        cy.get("#entries-per-page")
            .as("range")
            .invoke("val", 2)
            .trigger("change");
        cy.get("[data-testid=linearTimelineData]")
            .find("[data-testid=timelineCard]")
            .its("length")
            .should("eq", 100);

        cy.get("#entries-per-page")
            .as("range")
            .invoke("val", 0)
            .trigger("change");
        cy.get("[data-testid=linearTimelineData]")
            .find("[data-testid=timelineCard]")
            .its("length")
            .should("eq", 25);
    });

    it("Validate disabled filters", () => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = true
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
        })
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=MedicationCount]").should("be.visible");
        cy.get("[data-testid=ImmunizationCount]").should("not.be.visible");
        cy.get("[data-testid=EncounterCount]").should("not.be.visible");
        cy.get("[data-testid=NoteCount]").should("not.be.visible");
        cy.get("[data-testid=LaboratoryCount]").should("not.be.visible");
    });
});
