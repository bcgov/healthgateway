const {
    waitForCovid19Orders,
    waitForLaboratoryOrders,
} = require("../../../support/functions/timeline");
const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

describe("Laboratory Filters", () => {
    before(() => {
        setupTimelineFilter(
            ["covid19TestResult", "labResult"],
            [
                {
                    endpoint: "**/Laboratory/Covid19Orders*",
                    alias: "getCovid19Orders",
                    waitForData: waitForCovid19Orders,
                },
                {
                    endpoint: "**/Laboratory/LaboratoryOrders*",
                    alias: "getLaboratoryOrders",
                    waitForData: waitForLaboratoryOrders,
                },
            ]
        );
    });

    it("Validate Laboratory filters", () => {
        testDatasetTimelineFiltering(
            "[data-testid=Covid19TestResult-filter]",
            "[data-testid=covid19testresultTitle]",
            ["COVID‑19 Tests"]
        );
        cy.get("[data-testid=clear-filters-button]").click();

        testDatasetTimelineFiltering(
            "[data-testid=LabResult-filter]",
            "[data-testid=labresultTitle]",
            ["Lab Results"]
        );
    });
});
