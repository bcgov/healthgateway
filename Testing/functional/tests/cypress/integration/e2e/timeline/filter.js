const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

describe("Timeline Filter E2E", () => {
    before(() => {
        // Keep one real-data smoke test to verify that records returned by the
        // backend can be filtered. Detailed filter behavior uses UI fixtures.
        setupTimelineFilter("medication", {
            endpoint: "**/MedicationStatement/*",
            alias: "getMedications",
            waitForData: (alias) => cy.wait(alias, { timeout: 60000 }),
        });
    });

    it("Filters real timeline data by record type", () => {
        testDatasetTimelineFiltering("medication");
    });
});
