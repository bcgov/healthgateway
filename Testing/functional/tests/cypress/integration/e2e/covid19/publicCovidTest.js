const dobYearSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectYear]";
const dobMonthSelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectMonth]";
const dobDaySelector =
    "[data-testid=dateOfBirthInput] [data-testid=formSelectDay]";
const collectionDateYearSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectYear]";
const collectionDateMonthSelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectMonth]";
const collectionDateDaySelector =
    "[data-testid=dateOfCollectionInput] [data-testid=formSelectDay]";

const publicLaboratoryResultModule = "PublicLaboratoryResult";
const laboratoryModule = "Laboratory";
const covidTestUrl = "/covidtest";

const phn = "9875813462";
const dobYear = "1950";
const dobMonth = "March";
const dobDay = "15";

const indeterminateCollectionDateYear = "2021";
const indeterminateCollectionDateMonth = "October";
const indeterminateCollectionDateDay = "7";

const pendingCollectionDateYear = "2021";
const pendingCollectionDateMonth = "November";
const pendingCollectionDateDay = "27";

const positiveCollectionDateYear = "2020";
const positiveCollectionDateMonth = "September";
const positiveCollectionDateDay = "29";

const negativeCollectionDateYear = "2021";
const negativeCollectionDateMonth = "November";
const negativeCollectionDateDay = "5";

const cancelledCollectionDateYear = "2020";
const cancelledCollectionDateMonth = "September";
const cancelledCollectionDateDay = "27";

const multipleCollectionDateYear = "2021";
const multipleCollectionDateMonth = "November";
const multipleCollectionDateDay = "24";

function selectOption(selector, option) {
    return cy.get(selector).should("be.visible", "be.enabled").select(option);
}

function enterCovidTestPHN(phn) {
    cy.get("[data-testid=phnInput]")
        .should("be.visible", "be.enabled")
        .clear()
        .type(phn);
}

function clickCovidTestEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

function enterFormInputs(
    collectionDateYear,
    collectionDateMonth,
    collectionDateDay
) {
    enterCovidTestPHN(phn);
    selectOption(dobYearSelector, dobYear);
    selectOption(dobMonthSelector, dobMonth);
    selectOption(dobDaySelector, dobDay);
    selectOption(collectionDateYearSelector, collectionDateYear);
    selectOption(collectionDateMonthSelector, collectionDateMonth);
    selectOption(collectionDateDaySelector, collectionDateDay);

    clickCovidTestEnterButton();
}

function checkResult(
    expectedResult,
    expectedTestStatus,
    expectedResultClass = null
) {
    cy.get("[data-testid=public-covid-test-result-form-title]").should(
        "be.visible"
    );

    cy.get("[data-testid=public-display-name]").should("be.visible");

    cy.get("[data-testid=test-type-1]").should("be.visible");

    const result = cy.get("[data-testid=test-outcome-span-1]");

    if (expectedResultClass != null) {
        result.should("have.class", expectedResultClass);
    }

    result.should("be.visible").contains(expectedResult);

    cy.get("[data-testid=collection-date-1]").should("be.visible");

    cy.get("[data-testid=test-status]")
        .should("be.visible")
        .contains(expectedTestStatus);

    cy.get("[data-testid=result-date-1]").should("be.visible");

    cy.get("[data-testid=reporting-lab]").should("be.visible");

    cy.get("[data-testid=result-description]").should("be.visible");
}

describe("Public COVID-19 Test Results", () => {
    beforeEach(() => {
        cy.enableModules([laboratoryModule, publicLaboratoryResultModule]);
        cy.visit(covidTestUrl);
    });

    it("See AB#12590", () => {
        // Intentionally blank
    });

    // it("Successful Response: Negative Result", () => {
    //     enterFormInputs(
    //         negativeCollectionDateYear,
    //         negativeCollectionDateMonth,
    //         negativeCollectionDateDay
    //     );
    //     checkResult("Negative", "Final", "text-success");
    // });

    // it("Successful Response: Positive Result", () => {
    //     enterFormInputs(
    //         positiveCollectionDateYear,
    //         positiveCollectionDateMonth,
    //         positiveCollectionDateDay
    //     );
    //     checkResult("Positive", "Final", "text-danger");
    // });

    // it("Successful Response: Indeterminate Result", () => {
    //     enterFormInputs(
    //         indeterminateCollectionDateYear,
    //         indeterminateCollectionDateMonth,
    //         indeterminateCollectionDateDay
    //     );
    //     checkResult("Indeterminate", "Final");
    // });

    // it("Successful Response: Pending Result", () => {
    //     enterFormInputs(
    //         pendingCollectionDateYear,
    //         pendingCollectionDateMonth,
    //         pendingCollectionDateDay
    //     );
    //     checkResult("Not Set", "Pending");
    // });

    // it("Successful Response: Cancelled Result", () => {
    //     enterFormInputs(
    //         cancelledCollectionDateYear,
    //         cancelledCollectionDateMonth,
    //         cancelledCollectionDateDay
    //     );
    //     checkResult("Cancelled", "Cancelled");
    // });

    // it("Successful Response: Multiple Results", () => {
    //     enterFormInputs(
    //         multipleCollectionDateYear,
    //         multipleCollectionDateMonth,
    //         multipleCollectionDateDay
    //     );
    //     cy.get(".covid-test-result").should("have.length.at.least", 2);
    // });
});
