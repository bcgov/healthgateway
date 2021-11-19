const { AuthMethod } = require("../../../support/constants");
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("Reports - Medication Statement", () => {
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

    it("Validate Medication Report with Unsorted Data", () => {
        cy.intercept("GET", `**/v1/api/MedicationStatement/${HDID}`, (req) => {
            req.reply({
                fixture: "Report/medicationStatementUnSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Medications");
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
                        // Check if the sorting is descending
                        expect(firstDate).to.be.gte(secondDate);
                    });
            });
    });

    it("Validate Medication Report with Sorted Data", () => {
        cy.intercept("GET", `**/v1/api/MedicationStatement/${HDID}`, (req) => {
            req.reply({
                fixture: "Report/medicationStatementSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Medications");
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
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/reports"
        );
    });

    it("Validate Covid19 Report with Unsorted Data", () => {
        cy.intercept("GET", `**/v1/api/Laboratory?hdid=${HDID}`, (req) => {
            req.reply({
                fixture: "Report/covid19UnSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("COVID-19 Test Results");

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
                        // Check if the sorting is descending
                        expect(firstDate).to.be.gte(secondDate);
                    });
            });
    });

    it("Validate Covid19 Report with Sorted Data", () => {
        cy.intercept("GET", `**/v1/api/Laboratory?hdid=${HDID}`, (req) => {
            req.reply({
                fixture: "Report/covid19Sorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("COVID-19 Test Results");

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
        cy.intercept("GET", `**/v1/api/Immunization?hdid=${HDID}`, (req) => {
            req.reply({
                fixture: "Report/immunizationUnSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Immunizations");

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
                        // Check if the sorting is descending
                        expect(firstDate).to.be.gte(secondDate);
                    });
            });
    });

    it("Validate Immunization Report with Sorted Data", () => {
        cy.intercept("GET", `**/v1/api/Immunization?hdid=${HDID}`, (req) => {
            req.reply({
                fixture: "Report/immunizationSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Immunizations");

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
        cy.intercept("GET", `**/v1/api/Encounter/${HDID}`, (req) => {
            req.reply({
                fixture: "Report/mspVisitUnSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Health Visits");

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
                        // Check if the sorting is descending
                        expect(firstDate).to.be.gte(secondDate);
                    });
            });
    });

    it("Validate MSP Visit Report with Sorted Data", () => {
        cy.intercept("GET", `**/v1/api/Encounter/${HDID}`, (req) => {
            req.reply({
                fixture: "Report/mspVisitSorted.json",
            });
        });
        cy.get("[data-testid=reportType]")
            .should("be.enabled", "be.visible")
            .select("Health Visits");

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
                    });
            });
    });
});
