const {
    setupTimelineFilter,
    testDatasetTimelineFiltering,
} = require("../../../support/functions/filter");

const filterTests = [
    {
        name: "Medication",
        dataset: "medication",
        filterSelector: "[data-testid=Medication-filter]",
        titleSelector: "[data-testid=medicationTitle]",
        activeFilter: "Medication",
        request: {
            endpoint: "**/MedicationStatement/*",
            alias: "getMedications",
            waitForData: (alias) => cy.wait(alias, { timeout: 120000 }),
        },
    },
    {
        name: "Encounter",
        dataset: "healthVisit",
        filterSelector: "[data-testid=HealthVisit-filter]",
        titleSelector: "[data-testid=healthvisitTitle]",
        activeFilter: "Health Visits",
    },
    {
        name: "Special Authority",
        dataset: "specialAuthorityRequest",
        filterSelector: "[data-testid=SpecialAuthorityRequest-filter]",
        titleSelector: "[data-testid=specialauthorityrequestTitle]",
        activeFilter: "Special Authority",
    },
    {
        name: "Clinical Documents",
        dataset: "clinicalDocument",
        filterSelector: "[data-testid=ClinicalDocument-filter]",
        titleSelector: "[data-testid=clinicaldocumentTitle]",
        activeFilter: "Clinical Documents",
    },
    {
        name: "Hospital Visits",
        dataset: "hospitalVisit",
        filterSelector: "[data-testid=HospitalVisit-filter]",
        titleSelector: "[data-testid=hospitalvisitTitle]",
        activeFilter: "Hospital Visits",
    },
    {
        name: "Cancer Screening",
        dataset: "bcCancerScreening",
        filterSelector: "[data-testid=BcCancerScreening-filter]",
        titleSelector: "[data-testid=bccancerscreeningTitle]",
        activeFilter: "BC Cancer Screening",
    },
];

describe("Record Filters", () => {
    filterTests.forEach((filterTest) => {
        it(`Filter ${filterTest.name}`, () => {
            setupTimelineFilter(filterTest.dataset, filterTest.request);
            testDatasetTimelineFiltering(
                filterTest.filterSelector,
                filterTest.titleSelector,
                [filterTest.activeFilter]
            );
        });
    });
});
