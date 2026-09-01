const { waitForImmunizations } = require("../../../support/functions/timeline");
const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

describe("Immunization Filters", () => {
    before(() => {
        setupTimelineFilter("immunization", {
            endpoint: "**/Immunization?hdid=*",
            alias: "getImmunizations",
            waitForData: waitForImmunizations,
        });
    });

    it("Validate Immunization filters", () => {
        testDatasetTimelineFiltering("immunization");
        cy.get("[data-testid=clear-filters-button]").click();

        // Verify text matching against disease, vaccine code, and description.
        ["COVID", "EK4241", "Polio"].forEach((filterText, index, values) => {
            cy.get("[data-testid=filterDropdown]").click();
            cy.get("[data-testid=filterTextInput]").type(filterText);
            cy.get("[data-testid=btnFilterApply]").click();
            cy.get("[data-testid=immunizationTitle]").should("be.visible");
            if (index < values.length - 1) {
                cy.get("[data-testid=clear-filters-button]").click();
            }
        });
    });
});
