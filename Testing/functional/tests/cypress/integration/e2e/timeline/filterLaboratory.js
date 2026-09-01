const {
    waitForCovid19Orders,
    waitForLaboratoryOrders,
} = require("../../../support/functions/timeline");
const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

describe("Laboratory Filters", () => {
    const laboratoryDatasets = ["covid19TestResult", "labResult"];

    before(() => {
        setupTimelineFilter(laboratoryDatasets, [
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
        ]);
    });

    it("Validate Laboratory filters", () => {
        laboratoryDatasets.forEach((dataset, index) => {
            testDatasetTimelineFiltering(dataset);

            // Clear the first selection before validating the next dataset.
            if (index < laboratoryDatasets.length - 1) {
                cy.get("[data-testid=clear-filters-button]").click();
            }
        });
    });
});
