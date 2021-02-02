const { AuthMethod, localDevUri } = require("../../support/constants");

function verifyActiveFilter(activeFilterCount) {
    cy.get("[data-testid=filterDropdown]").click();
    cy.get('[data-testid=filterDropdown]').should('have.css', 'background-color', 'rgb(0, 146, 241)'); // has the '#0092f1' background-color
    cy.get('[data-testid=filterDropdown] > span').contains(activeFilterCount); // has 1 active filter
    cy.viewport('iphone-6');
    cy.get('[data-testid=mobileFilterDropdown]').should('have.css', 'background-color', 'rgb(0, 146, 241)'); // has the '#0092f1' background-color
    cy.viewport(1000, 600);
    cy.wait(500);
    cy.get("[data-testid=filterDropdown]").focus().click({ force: true });
}

describe("Filters", () => {
    before(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
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

    it("Validate Date Range Filter", () => {
        //Validate No records... text should be hidden by default (or with data)
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        // Validate "No records found with the selected filters" for a Date Range Filter
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2020-10-01");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2020-10-02");
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=noTimelineEntriesText]").children().first().should("have.text", "No records found with the selected filters");
        
        // Select 06/14/2020 to 06/14/2020 should display data for this date range.
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear()
            .type("2020-06-14");
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear()
            .type("2020-06-14");
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
        verifyActiveFilter('2');

        // Clear date range filter for next tests
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear();
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear();
        cy.get("[data-testid=filterDropdown]").focus().click({ force: true });
    });

    it("No Records on Linear Timeline", () => {
        cy.get("[data-testid=filterTextInput]").type("xxxx");
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
    });

    it("No Records on Calendar Timeline", () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get("[data-testid=monthViewToggle]").first().click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=filterTextInput]").clear();
        cy.get("[data-testid=listViewToggle]").last().click();
    });

    it("Filter Checkboxes are Visible", () => {
        cy.get('[data-testid=filterDropdown]').click();
        cy.get("[data-testid=Medication-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Note-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Immunization-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Laboratory-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Encounter-filter]").should("not.to.be.checked");
    });

    it("Filter Immunization", () => {
        // if (Cypress.config().baseUrl != localDevUri) {
        //     cy.get("[data-testid=Immunization-filter]").click({ force: true });
        //     cy.get("[data-testid=immunizationTitle]").should("be.visible");
        //     cy.get("[data-testid=noteTitle]").should("not.exist");
        //     cy.get("[data-testid=encounterTitle]").should("not.exist");
        //     cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        //     cy.get("[data-testid=medicationTitle]").should("not.exist");
        //     cy.get('[data-testid=filterDropdown]').contains("Clear").click();
        // }
        // else {
        //     cy.log("Skipped Filter Immunization as running locally")
        // }
    });

    it("Filter Medication", () => {
        cy.get("[data-testid=Medication-filter]").click({ force: true });
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("be.visible");
        verifyActiveFilter('1');
        cy.get('[data-testid=filterContainer]').contains("Clear").click();
    });

    it("Filter Encounter", () => {
        cy.get("[data-testid=Encounter-filter]").click({ force: true });
        cy.get("[data-testid=encounterTitle]").should("be.visible");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        verifyActiveFilter('1');
        cy.get('[data-testid=filterContainer]').contains("Clear").click();
    });

    it("Filter Laboratory", () => {
        cy.get("[data-testid=Laboratory-filter]").click({ force: true });
        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        verifyActiveFilter('1');
        cy.get('[data-testid=filterContainer]').contains("Clear").click();
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
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=MedicationCount]").should("be.visible");
        cy.get("[data-testid=ImmunizationCount]").should("not.exist");
        cy.get("[data-testid=EncounterCount]").should("not.exist");
        cy.get("[data-testid=NoteCount]").should("not.exist");
        cy.get("[data-testid=LaboratoryCount]").should("not.exist");
    });
});
