import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const startDateId = "[data-testid=start-date-input] input";
const endDateId = "[data-testid=end-date-input] input";

function enterDate(selector, currentDate, newDate) {
    cy.get(selector)
        .should("be.enabled", "be.visible")
        .should("have.value", currentDate)
        .click()
        .focus()
        .clear()
        .type(newDate);
}

function verifyDate(selector, date) {
    cy.get(selector).should("have.value", date);
}

function verifyDateRange(dateRange) {
    cy.get("[data-testid=selected-dates-filter]").contains(dateRange);
}

function verifyAdvancedFiltersVisible(visible) {
    if (visible) {
        cy.get("[data-testid=clear-btn]").should("be.visible");
        cy.get("[data-testid=apply-filter-btn]").should("be.visible");
        cy.get(startDateId).should("be.enabled", "be.visible");
        cy.get(endDateId).should("be.enabled", "be.visible");
    } else {
        cy.get("[data-testid=clear-btn]").should("not.be.visible");
        cy.get("[data-testid=apply-filter-btn]").should("not.be.visible");
        cy.get(startDateId).should("not.be.visible");
        cy.get(endDateId).should("not.be.visible");
    }
}

function replaceSpaceWithDash(source) {
    return source.replace(/ /g, "-");
}

describe("Report Filtering", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Dates, Cancel and Apply", () => {
        verifyAdvancedFiltersVisible(false);

        cy.get("[data-testid=advanced-btn]")
            .should("be.enabled", "be.visible")
            .click();

        verifyAdvancedFiltersVisible(true);

        enterDate(startDateId, "", "2020-FEB-03");
        enterDate(endDateId, "", "2020-FEB-05");

        cy.get("[data-testid=apply-filter-btn]").click();
        verifyDateRange("From 2020-Feb-03 Up To 2020-Feb-05");

        // Validate filters - Cancel  button
        cy.get("[data-testid=clear-btn").click();
        verifyAdvancedFiltersVisible(false);

        // Click on Advanced button see advanced filter options
        cy.get("[data-testid=advanced-btn]").click();
        verifyAdvancedFiltersVisible(true);

        // Cancel button should not set the newly entered values
        // Also, set new date range and apply
        enterDate(startDateId, "2020-FEB-03", "2021-FEB-03");
        enterDate(endDateId, "2020-FEB-05", "2021-FEB-05");

        cy.get("[data-testid=apply-filter-btn]").click();
        verifyDateRange("From 2021-Feb-03 Up To 2021-Feb-05");

        cy.get("[data-testid=clear-btn]").focus();
        verifyDate(startDateId, "2021-FEB-03");
        verifyDate(endDateId, "2021-FEB-05");

        // After clicking on Apply, advanced filter should not be visible
        verifyAdvancedFiltersVisible(false);

        // Click on advnaced button to open date range filter
        cy.get("[data-testid=advanced-btn]").click();
        verifyAdvancedFiltersVisible(true);
        verifyDate(startDateId, "2021-FEB-03");
        verifyDate(endDateId, "2021-FEB-05");
        verifyDateRange("From 2021-Feb-03 Up To 2021-Feb-05");

        // Click on advnaced button to close date range filter
        cy.get("[data-testid=advanced-btn]").click();
        verifyAdvancedFiltersVisible(false);
        verifyDateRange("From 2021-Feb-03 Up To 2021-Feb-05");
    });

    it("Validate Medication Exclusions", () => {
        cy.vSelect("[data-testid=report-type]", "Medications");
        cy.get("[data-testid=advancedPanel]").should("not.exist");
        cy.get("[data-testid=advanced-btn]")
            .should("be.enabled", "be.visible")
            .click();
        cy.get("[data-testid=medication-exclusion-filter]").should(
            "be.visible"
        );
        cy.get("[data-testid=medicationReportBrandNameItem]")
            .not(':contains("Pharmacist Assessment")')
            .first()
            .should("be.visible")
            .then(($brandName) => {
                const brandText = $brandName.text().trim();
                cy.vSelect(
                    "[data-testid=medication-exclusion-filter]",
                    brandText
                );
                cy.get("[data-testid=apply-filter-btn]").click();

                const excludedFilterId = `[data-testid='${replaceSpaceWithDash(
                    brandText
                )}-excluded']`;
                cy.get(excludedFilterId).contains(brandText);

                cy.get("[data-testid=medicationReportBrandNameItem]")
                    .not(':contains("Pharmacist Assessment")')
                    .first()
                    .should("be.visible")
                    .then(($filteredBrandName) => {
                        const filteredBrandText = $filteredBrandName
                            .text()
                            .trim();
                        expect(filteredBrandText).to.not.equal(brandText);
                    });

                cy.get("[data-testid=advanced-btn]").click();
                cy.get(excludedFilterId).contains(brandText);

                const clearExcludedFilterId = `[data-testid='${replaceSpaceWithDash(
                    brandText
                )}-clear-filter']`;
                cy.get(`${clearExcludedFilterId} .v-chip__close`).click();

                cy.get("[data-testid=medicationReportBrandNameItem]")
                    .not(':contains("Pharmacist Assessment")')
                    .first()
                    .should("be.visible")
                    .then(($unfilteredBrandName) => {
                        const unfilteredBrandText = $unfilteredBrandName
                            .text()
                            .trim();
                        expect(unfilteredBrandText).to.equal(brandText);
                    });

                cy.get("[data-testid=advancedPanel]").should("not.exist");
            });
    });
});
