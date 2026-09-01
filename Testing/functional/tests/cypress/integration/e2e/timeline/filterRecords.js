const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

const filterTests = [
    {
        dataset: "medication",
        request: {
            endpoint: "**/MedicationStatement/*",
            alias: "getMedications",
            waitForData: (alias) => cy.wait(alias, { timeout: 60000 }),
        },
    },
    { dataset: "healthVisit" },
    { dataset: "specialAuthorityRequest" },
    { dataset: "clinicalDocument" },
    { dataset: "hospitalVisit" },
    { dataset: "bcCancerScreening" },
];

describe("Record Filters", () => {
    before(() => {
        setupTimelineFilter(
            filterTests.map((filterTest) => filterTest.dataset),
            filterTests.map((filterTest) => filterTest.request).filter(Boolean)
        );
    });

    it("Validate record filters", () => {
        filterTests.forEach((filterTest, index) => {
            testDatasetTimelineFiltering(filterTest.dataset);

            if (index < filterTests.length - 1) {
                cy.get("[data-testid=clear-filters-button]").click();
            }
        });
    });
});
