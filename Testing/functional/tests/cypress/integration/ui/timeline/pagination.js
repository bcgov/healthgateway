import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

const pageSize = 25;
const recordCount = 30;

function setupMedicationRecords() {
    cy.fixture("MedicationService/medicationStatement.json").then(
        (response) => {
            const fixture = Cypress._.cloneDeep(response);
            const template = fixture.resourcePayload[0];
            fixture.resourcePayload = Array.from(
                { length: recordCount },
                (_, index) => ({
                    ...template,
                    prescriptionIdentifier: `pagination-${index}`,
                    dispensedDate: new Date(Date.UTC(2024, 0, 30 - index))
                        .toISOString()
                        .slice(0, 10),
                })
            );
            fixture.totalResultCount = recordCount;
            cy.intercept("GET", "**/MedicationStatement/*", fixture);
        }
    );
}

describe("Timeline Pagination", () => {
    beforeEach(() => {
        configureFakeSettings({
            datasets: [{ name: "medication", enabled: true }],
        });
        setupStandardFixtures();
        setupMedicationRecords();
        fakeLogin("/timeline");
    });

    it("Displays the record count and navigates between pages", () => {
        cy.contains(
            "[data-testid=timeline-record-count]",
            `Displaying 1 to ${pageSize} out of ${recordCount} records`
        ).should("be.visible");
        cy.contains("[data-testid=entryCardDate]", "2024-Jan-30").should(
            "be.visible"
        );

        cy.get("[data-testid=pagination]")
            .find("[data-icon=chevron-right]")
            .click({ force: true });
        cy.contains(
            "[data-testid=timeline-record-count]",
            `Displaying 26 to ${recordCount} out of ${recordCount} records`
        ).should("be.visible");
        cy.contains("[data-testid=entryCardDate]", "2024-Jan-05").should(
            "be.visible"
        );

        cy.get("[data-testid=pagination]")
            .find("[data-icon=chevron-left]")
            .click({ force: true });
        cy.contains("[data-testid=entryCardDate]", "2024-Jan-30").should(
            "be.visible"
        );
    });
});
