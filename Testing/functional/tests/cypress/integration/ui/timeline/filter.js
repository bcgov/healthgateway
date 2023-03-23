const { AuthMethod } = require("../../../support/constants");

describe("Filters", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });
        cy.intercept("GET", "**/Immunization?*", {
            fixture: "ImmunizationService/immunization.json",
        });
        cy.intercept("GET", "**/Encounter/*", {
            fixture: "EncounterService/encounters.json",
        });
        cy.configureSettings({
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Verify filtered record count", () => {
        const unfilteredRecordsMessage = "Displaying 25 out of 34 records";

        cy.get("[data-testid=timeline-record-count]").contains(
            unfilteredRecordsMessage
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get("[data-testid=immunizationTitle]").should("be.visible");

        cy.get("[data-testid=timeline-record-count]").contains(
            "Displaying 9 out of 9 records"
        );

        cy.contains("[data-testid=filter-label]", "Immunizations")
            .children("button")
            .click();

        cy.get("[data-testid=timeline-record-count]").contains(
            unfilteredRecordsMessage
        );

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterStartDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=filterEndDateInput] input")
            .clear()
            .focus()
            .type("2022-JUN-09");
        cy.get("[data-testid=btnFilterApply]").focus().click();

        cy.contains(
            "[data-testid=filter-label]",
            "From 2022-Jun-09 To 2022-Jun-09"
        );

        cy.get("[data-testid=noTimelineEntriesText]").should("be.visible");

        cy.get("[data-testid=clear-filters-button]").click();
        cy.get("[data-testid=timeline-record-count]").contains(
            unfilteredRecordsMessage
        );
    });

    it("Verify immunization record alert appears when only immunization is selected", () => {
        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=Immunization-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-immunization-disclaimer-alert]"
        ).should("not.be.visible");
    });

    it("Verify clinical document record alert appears when only clinical document is selected", () => {
        cy.get(
            "[data-testid=linear-timeline-clinical-document-disclaimer-alert]"
        ).should("not.be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=ClinicalDocument-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();
        cy.get("[data-testid=btnFilterApply]").should("not.exist");

        cy.get(
            "[data-testid=linear-timeline-clinical-document-disclaimer-alert]"
        ).should("be.visible");

        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=HealthVisit-filter]").click({ force: true });
        cy.get("[data-testid=btnFilterApply]").click();

        cy.get(
            "[data-testid=linear-timeline-clinical-document-disclaimer-alert]"
        ).should("not.be.visible");
    });
});
