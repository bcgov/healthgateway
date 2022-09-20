const { AuthMethod } = require("../../../support/constants");
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

function getDate(value) {
    return new Date(value && value.trim().length !== 0 ? value.trim() : 0);
}

describe("Reports - Medication", () => {
    beforeEach(() => {
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
        ]);
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
        cy.get("[data-testid=reportType]").select("Medications");
        cy.get("[data-testid=reportSample]").should("be.visible");

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

    it("Validate Immunization Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Immunization?hdid=${HDID}`, {
            fixture: "Report/immunizationUnSorted.json",
        });
        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=reportSample]").should("be.visible");

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
                const firstDate = getDate($dateItem.text());
                cy.get("[data-testid=recommendationDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // Column date in the 2nd row in the table
                        const secondDate = getDate($dateItem.text());
                        expect(firstDate).to.be.gte(secondDate);
                        // Column date in the last row in the table
                        cy.get("[data-testid=recommendationDateItem]")
                            .eq(4)
                            .then(($dateItem) => {
                                // Column date in the last row in the table
                                const lastDate = getDate($dateItem.text());
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });
});

describe("Reports - Covid19", () => {
    beforeEach(() => {
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
            "AllLaboratory",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Covid19 Report with Unsorted Data", () => {
        cy.intercept("GET", `**/Laboratory/Covid19Orders?hdid=${HDID}`, {
            fixture: "Report/covid19UnSorted.json",
        });
        cy.get("[data-testid=reportType]").select("COVID-19 Test Results");

        cy.get("[data-testid=reportSample]").should("be.visible");

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
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
        ]);
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
        cy.get("[data-testid=reportType]").select("Immunizations");

        cy.get("[data-testid=reportSample]").should("be.visible");

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
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
        ]);
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
        cy.get("[data-testid=reportType]").select("Health Visits");

        cy.get("[data-testid=reportSample]").should("be.visible");

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

describe("Reports - Notes (User-Entered)", () => {
    beforeEach(() => {
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
            "Note",
        ]);
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
        cy.get("[data-testid=reportType]").select("My Notes");

        cy.get("[data-testid=reportSample]").should("be.visible");

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
        cy.enableModules([
            "Encounter",
            "Medication",
            "Laboratory",
            "Immunization",
            "MedicationRequest",
            "Note",
            "AllLaboratory",
        ]);
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
        cy.get("[data-testid=reportType]").select("Laboratory Tests");

        cy.get("[data-testid=reportSample]").should("be.visible");

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
