const { AuthMethod } = require("../../../support/constants");

describe("Report Filtering", () => {
    beforeEach(() => {
        cy.enableModules("Medication");
        cy.intercept("GET", "**/v1/api/MedicationStatement/*", {
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

        cy.get("[data-testid=advancedBtn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=startDateInput] input")
            .should("be.enabled", "be.visible")
            .should("have.value", "")
            .click()
            .focus()
            .type("2021-FEB-03");

        cy.get("[data-testid=endDateInput] input")
            .should("be.enabled", "be.visible")
            .should("have.value", "")
            .click()
            .focus()
            .type("2021-FEB-05")
            .focus();

        cy.get("[data-testid=applyFilterBtn]").click();
        cy.get("[data-testid=selectedDatesFilter]").contains(
            "From 2021-Feb-03 Up To 2021-Feb-05"
        );

        // Validate filters - Cancel  button
        cy.get("[data-testid=advancedBtn]").click();

        // Cancel button should not set the newly entered values
        cy.get("[data-testid=startDateInput] input")
            .clear()
            .type("2020-FEB-03");
        cy.get("[data-testid=endDateInput] input").clear().type("2020-FEB-05");
        cy.get("[data-testid=clearBtn]").focus();
        cy.get("[data-testid=startDateInput] input").should(
            "have.value",
            "2020-FEB-03"
        );
        cy.get("[data-testid=endDateInput] input").should(
            "have.value",
            "2020-FEB-05"
        );
        cy.get("[data-testid=clearBtn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=advancedBtn]").click();
        cy.get("[data-testid=startDateInput] input").should(
            "have.value",
            "2021-FEB-03"
        );
        cy.get("[data-testid=endDateInput] input").should(
            "have.value",
            "2021-FEB-05"
        );

        cy.get("[data-testid=clearFilter] button").should("be.visible").click();
        cy.get("[data-testid=clearFilter] button").should("not.exist");
    });

    it("Validate Medication Exclusions", () => {
        cy.get("[data-testid=reportType]").select("Medications");

        cy.get("[data-testid=advancedPanel]").should("not.be.visible");

        cy.get("[data-testid=advancedBtn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=medicationExclusionFilter]").should("be.visible");

        cy.get("[data-testid=medicationReportBrandNameItem]")
            .should("be.visible")
            .first()
            .then(($brandName) => {
                const brandText = $brandName.text().trim();

                cy.get("[data-testid=medicationExclusionFilter] select")
                    .focus()
                    .select(brandText);

                cy.get("[data-testid=applyFilterBtn]").click();

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

                cy.get("[data-testid=advancedBtn]").click();
                cy.get(
                    `[data-testid=medicationExclusionFilter] [title="${brandText}"] button`
                )
                    .should("be.enabled", "be.visible")
                    .click();

                cy.get("[data-testid=applyFilterBtn]").click();

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
