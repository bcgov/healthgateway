const { waitForImmunizations } = require("../../../support/functions/timeline");
const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

function setupImmunizationFilter() {
    setupTimelineFilter("immunization", {
        endpoint: "**/Immunization?hdid=*",
        alias: "getImmunizations",
        waitForData: waitForImmunizations,
    });
}

describe("Immunization Filters", () => {
    before(setupImmunizationFilter);

    it("Validate Immunization filters", () => {
        testDatasetTimelineFiltering(
            "[data-testid=Immunization-filter]",
            "[data-testid=immunizationTitle]",
            ["Immunization"]
        );
        cy.get("[data-testid=clear-filters-button]").click();

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
