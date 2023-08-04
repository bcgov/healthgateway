const { AuthMethod } = require("../../../../support/constants");

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
        cy.get("[data-testid=advancedPanel]").should("not.be.visible");

        cy.get("[data-testid=advanced-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=start-date-input] input")
            .should("be.enabled", "be.visible")
            .should("have.value", "")
            .click()
            .focus()
            .type("2021-FEB-03");

        cy.get("[data-testid=end-date-input] input")
            .should("be.enabled", "be.visible")
            .should("have.value", "")
            .click()
            .focus()
            .type("2021-FEB-05")
            .focus();

        cy.get("[data-testid=apply-filter-btn]").click();
        cy.get("[data-testid=selected-dates-filter]").contains(
            "From 2021-Feb-03 Up To 2021-Feb-05"
        );

        // Validate filters - Cancel  button
        cy.get("[data-testid=advanced-btn]").click();

        // Cancel button should not set the newly entered values
        cy.get("[data-testid=start-date-input] input")
            .clear()
            .type("2020-FEB-03");
        cy.get("[data-testid=end-date-input] input").clear().type("2020-FEB-05");
        cy.get("[data-testid=clear-btn]").focus();
        cy.get("[data-testid=start-date-input] input").should(
            "have.value",
            "2020-FEB-03"
        );
        cy.get("[data-testid=end-date-input] input").should(
            "have.value",
            "2020-FEB-05"
        );
        cy.get("[data-testid=clear-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=advanced-btn]").click();
        cy.get("[data-testid=start-date-input] input").should(
            "have.value",
            "2021-FEB-03"
        );
        cy.get("[data-testid=end-date-input] input").should(
            "have.value",
            "2021-FEB-05"
        );

        cy.get("[data-testid=clear-filter] button").should("be.visible").click();
        cy.get("[data-testid=clear-filter] button").should("not.exist");
    });

    it("Validate Medication Exclusions", () => {
        cy.vSelect("[data-testid=report-type]", "Medications");

        cy.get("[data-testid=advancedPanel]").should("not.be.visible");

        cy.get("[data-testid=advanced-btn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=medication-exclusion-filter]").should("be.visible");

        cy.get("[data-testid=medicationReportBrandNameItem]")
            .should("be.visible")
            .first()
            .then(($brandName) => {
                const brandText = $brandName.text().trim();

                cy.get("[data-testid=medication-exclusion-filter] select")
                    .focus()
                    .select(brandText);

                cy.get("[data-testid=apply-filter-btn]").click();

                cy.get(
                    "[data-testid=medicationFilter] .b-form-tag-content span"
                ).should("contain", brandText);

                cy.get("[data-testid=medicationReportBrandNameItem]")
                    .should("be.visible")
                    .first()
                    .then(($filteredBrandName) => {
                        const filteredBrandText = $filteredBrandName
                            .text()
                            .trim();
                        expect(filteredBrandText).to.not.equal(brandText);
                    });

                cy.get("[data-testid=advanced-btn]").click();
                cy.get(
                    `[data-testid=medication-exclusion-filter] [title="${brandText}"] button`
                )
                    .should("be.enabled", "be.visible")
                    .click();

                cy.get("[data-testid=apply-filter-btn]").click();

                cy.get(
                    "[data-testid=medicationFilter] .b-form-tag-content"
                ).should("not.exist");

                cy.get("[data-testid=medicationReportBrandNameItem]")
                    .should("be.visible")
                    .first()
                    .then(($unfilteredBrandName) => {
                        const unfilteredBrandText = $unfilteredBrandName
                            .text()
                            .trim();
                        expect(unfilteredBrandText).to.equal(brandText);
                    });
            });
    });
});
