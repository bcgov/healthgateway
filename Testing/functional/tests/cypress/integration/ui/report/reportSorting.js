import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("Reports - Medication", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Medication Statement Report with Unsorted Data", () => {
        cy.intercept("GET", `**/MedicationStatement/${HDID}`, {
            fixture: "Report/medicationStatementUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "Medications");
        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=medicationDateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=medicationDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=medicationDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Covid19", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    // AB#16941 - Skip test as Covid19 removed from report list.
    it.skip("Validate Covid19 Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Laboratory/Covid19Orders?hdid=${HDID}`, {
            fixture: "Report/covid19UnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "COVID‑19 Tests");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=covid19DateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=covid19DateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=covid19DateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Immunization", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Immunization Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Immunization?hdid=${HDID}`, {
            fixture: "Report/immunizationUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "Immunizations");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=immunizationDateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=immunizationDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=immunizationDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });

        cy.get("[data-testid=recommendationDateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date(
                    $dateItem.text() ? $dateItem.text().trim() : 0
                );
                cy.get("[data-testid=recommendationDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date(
                            $dateItem.text() ? $dateItem.text().trim() : 0
                        );
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=recommendationDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text()
                                        ? $dateItem.text().trim()
                                        : 0
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - MSP Visit", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate MSP Visit Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Encounter/${HDID}`, {
            fixture: "Report/mspVisitUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "Health Visits");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=mspVisitDateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=mspVisitDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=mspVisitDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Hospital Visits", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "hospitalVisit",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Hospital Visit Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Encounter/HospitalVisit/${HDID}`, {
            fixture: "Report/hospitalVisitUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "Hospital Visits");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=hospital-visit-date]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=hospital-visit-date]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=hospital-visit-date]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Notes (User-Entered)", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Note (User-Entered) Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Note/${HDID}`, {
            fixture: "Report/noteUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "My Notes");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=user-note-date]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=user-note-date]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=user-note-date]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Laboratory Tests", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Laboratory Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Laboratory/LaboratoryOrders?hdid=${HDID}`, {
            fixture: "Report/laboratoryUnSorted.json",
        });
        cy.vSelect("[data-testid=report-type]", "Lab Results");

        cy.get("[data-testid=report-sample]").should("be.visible");

        cy.get("[data-testid=labResultDateItem]")
            .first()
            .then(($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=labResultDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=labResultDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});
