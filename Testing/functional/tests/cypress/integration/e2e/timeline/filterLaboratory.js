const {
    waitForCovid19Orders,
    waitForLaboratoryOrders,
} = require("../../../support/functions/timeline");
const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

describe("Laboratory Filters", () => {
    it("Filter COVID-19", () => {
        setupTimelineFilter("covid19TestResult", {
            endpoint: "**/Laboratory/Covid19Orders*",
            alias: "getCovid19Orders",
            waitForData: waitForCovid19Orders,
        });
        testDatasetTimelineFiltering(
            "[data-testid=Covid19TestResult-filter]",
            "[data-testid=covid19testresultTitle]",
            ["COVID‑19 Tests"]
        );
    });

    it("Filter Laboratory", () => {
        setupTimelineFilter("labResult", {
            endpoint: "**/Laboratory/LaboratoryOrders*",
            alias: "getLaboratoryOrders",
            waitForData: waitForLaboratoryOrders,
        });
        testDatasetTimelineFiltering(
            "[data-testid=LabResult-filter]",
            "[data-testid=labresultTitle]",
            ["Lab Results"]
        );
    });
});
