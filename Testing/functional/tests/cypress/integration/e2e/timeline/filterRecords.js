const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

const filterTests = [
    {
        dataset: "medication",
        filterSelector: "[data-testid=Medication-filter]",
        titleSelector: "[data-testid=medicationTitle]",
        activeFilter: "Medication",
        request: {
            endpoint: "**/MedicationStatement/*",
            alias: "getMedications",
            waitForData: (alias) => cy.wait(alias, { timeout: 60000 }),
        },
    },
    {
        dataset: "healthVisit",
        filterSelector: "[data-testid=HealthVisit-filter]",
        titleSelector: "[data-testid=healthvisitTitle]",
        activeFilter: "Health Visits",
    },
    {
        dataset: "specialAuthorityRequest",
        filterSelector: "[data-testid=SpecialAuthorityRequest-filter]",
        titleSelector: "[data-testid=specialauthorityrequestTitle]",
        activeFilter: "Special Authority",
    },
    {
        dataset: "clinicalDocument",
        filterSelector: "[data-testid=ClinicalDocument-filter]",
        titleSelector: "[data-testid=clinicaldocumentTitle]",
        activeFilter: "Clinical Documents",
    },
    {
        dataset: "hospitalVisit",
        filterSelector: "[data-testid=HospitalVisit-filter]",
        titleSelector: "[data-testid=hospitalvisitTitle]",
        activeFilter: "Hospital Visits",
    },
    {
        dataset: "bcCancerScreening",
        filterSelector: "[data-testid=BcCancerScreening-filter]",
        titleSelector: "[data-testid=bccancerscreeningTitle]",
        activeFilter: "BC Cancer Screening",
    },
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
            testDatasetTimelineFiltering(
                filterTest.filterSelector,
                filterTest.titleSelector,
                [filterTest.activeFilter]
            );

            if (index < filterTests.length - 1) {
                cy.get("[data-testid=clear-filters-button]").click();
            }
        });
    });
});
