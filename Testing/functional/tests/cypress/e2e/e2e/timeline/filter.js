const { AuthMethod } = require("../../../support/constants");

function verifyActiveFilters(filterLabels) {
    cy.get("[data-testid=filterDropdown]").should(
        "have.css",
        "background-color",
        "rgb(0, 146, 241)"
    ); // has the '#0092f1' background-color
    filterLabels.forEach((label) => {
        cy.contains("[data-testid=filter-label]", label);
    });
}

describe("Disabled Filters", () => {
    beforeEach(() => {
        cy.enableModules("Medication");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Validate disabled filters", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=MedicationCount]").should("be.visible");
        cy.get("[data-testid=ImmunizationCount]").should("not.exist");
        cy.get("[data-testid=EncounterCount]").should("not.exist");
        cy.get("[data-testid=NoteCount]").should("not.exist");
        cy.get("[data-testid=LaboratoryCount]").should("not.exist");
        cy.get("[data-testid=AllLaboratoryCount]").should("not.exist");
        cy.get("[data-testid=MedicationRequestCount]").should("not.exist");
        cy.get("[data-testid=ClinicalDocumentCount]").should("not.exist");
        cy.get("[data-testid=HospitalVisitCount]").should("not.exist");
        cy.get("[data-testid=btnFilterCancel]").click();
    });
});

describe("Filters", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
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
        cy.get("[data-testid=AllLaboratoryCount]")
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
        cy.get("[data-testid=MedicationRequestCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=ClinicalDocumentCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=HospitalVisitCount]")
            .should("be.visible")
            .contains(countRegex);
        cy.get("[data-testid=btnFilterCancel]").click();
    });

    it("Validate Date Range Filter", () => {
        //Validate No records... text should be hidden by default (or with data)
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");

        // Validate "No records found with the selected filters" for a Date Range Filter
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2020-SEP-30");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2020-OCT-01")
            .focus();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.get("[data-testid=noTimelineEntriesText]")
            .children()
            .first()
            .should("have.text", "No records found with the selected filters");

        // Select 06/14/2020 to 06/14/2020 should display data for this date range.
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .focus()
            .clear()
            .type("2020-JUN-14");
        cy.get("[data-testid=filterEndDateInput] input")
            .focus()
            .clear()
            .type("2020-JUN-14")
            .focus();
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
        verifyActiveFilters(["From 2020-Jun-14 To 2020-Jun-14"]);
    });

    it("No Records on Linear Timeline", () => {
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("xxxx");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");
        cy.contains("[data-testid=filter-label]", '"xxxx"')
            .children("button")
            .click();
        cy.get("[data-testid=noTimelineEntriesText]").should("not.exist");
    });

    it("Filter Checkboxes are Visible", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Medication-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Note-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Immunization-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Laboratory-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Encounter-filter]").should("not.to.be.checked");
        cy.get("[data-testid=ClinicalDocument-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=HospitalVisit-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=btnFilterCancel]").click();
    });

    it("Filter Immunization", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Immunization"]);
    });

    it("Filter Immunization By Text", () => {
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("COVID");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=clear-filters-button]").click();

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("EK4241");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
        cy.get("[data-testid=clear-filters-button]").click();

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterTextInput]").type("Polio");
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=immunizationTitle]").should("be.visible");
    });

    it("Filter Medication", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Medication-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("be.visible");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Medication"]);
    });

    it("Filter Encounter", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Encounter-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("be.visible");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Health Visits"]);
    });

    it("Filter COVID-19", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Laboratory-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["COVID‑19 Tests"]);
    });

    it("Filter Laboratory", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=AllLaboratory-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Lab Results"]);
    });

    it("Filter Special Authority", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=MedicationRequest-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("be.visible");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Special Authority"]);
    });

    it("Filter Clinical Documents", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ClinicalDocument-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("be.visible");
        cy.get("[data-testid=hospitalvisitTitle]").should("not.exist");
        verifyActiveFilters(["Clinical Documents"]);
    });

    it("Filter Hospital Visits", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HospitalVisit-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("not.exist");
        cy.get("[data-testid=alllaboratoryTitle]").should("not.exist");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=medicationrequestTitle]").should("not.exist");
        cy.get("[data-testid=clinicaldocumentTitle]").should("not.exist");
        cy.get("[data-testid=hospitalvisitTitle]").should("be.visible");
        verifyActiveFilters(["Hospital Visits"]);
    });

    it("Validate Apply and Cancel buttons", () => {
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Medication-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Note-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Immunization-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Laboratory-filter]").should("not.to.be.checked");
        cy.get("[data-testid=AllLaboratory-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=Encounter-filter]").should("not.to.be.checked");
        cy.get("[data-testid=MedicationRequest-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=ClinicalDocument-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=HospitalVisit-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=Medication-filter]").click({ force: true });
        cy.get("[data-testid=Encounter-filter]").click({ force: true });
        cy.get("[data-testid=Laboratory-filter]").click({ force: true });
        cy.get("[data-testid=AllLaboratory-filter]").click({ force: true });
        cy.get("[data-testid=Note-filter]").click({ force: true });
        cy.get("[data-testid=MedicationRequest-filter]").click({ force: true });
        cy.get("[data-testid=ClinicalDocument-filter]").click({ force: true });
        cy.get("[data-testid=HospitalVisit-filter]").click({ force: true });
        cy.get("[data-testid=Medication-filter]").should("be.checked");
        cy.get("[data-testid=Note-filter]").should("be.checked");
        cy.get("[data-testid=Immunization-filter]").should("be.checked");
        cy.get("[data-testid=Laboratory-filter]").should("be.checked");
        cy.get("[data-testid=AllLaboratory-filter]").should("be.checked");
        cy.get("[data-testid=Encounter-filter]").should("be.checked");
        cy.get("[data-testid=MedicationRequest-filter]").should("be.checked");
        cy.get("[data-testid=ClinicalDocument-filter]").should("be.checked");
        cy.get("[data-testid=HospitalVisit-filter]").should("be.checked");
        cy.get("[data-testid=btnFilterCancel]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Medication-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Note-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Immunization-filter]").should("not.to.be.checked");
        cy.get("[data-testid=Laboratory-filter]").should("not.to.be.checked");
        cy.get("[data-testid=AllLaboratory-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=Encounter-filter]").should("not.to.be.checked");
        cy.get("[data-testid=MedicationRequest-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=ClinicalDocument-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=HospitalVisit-filter]").should(
            "not.to.be.checked"
        );
        cy.get("[data-testid=btnFilterCancel]").click();
        cy.get("[data-testid=filterContainer]").should("not.exist");
    });
});
