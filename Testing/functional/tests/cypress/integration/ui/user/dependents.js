import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const sensitiveDocMessage =
    "The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off.";
const validHdid = "645645767756756767";

function getDate(value) {
    return new Date(value && value.trim().length !== 0 ? value.trim() : 0);
}

describe("COVID-19", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
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
            "/dependents"
        );
    });

    it("Validate Covid Tab with Results", () => {
        // Validate the tab and elements are present
        cy.get("[data-testid=covid19TabTitle]").click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .first()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");

        cy.get("[data-testid=covid19TabTitle]").click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=dependentCovidTestStatus]")
            .last()
            .should("not.be.empty");
        cy.get("[data-testid=dependentCovidTestLabResult]")
            .last()
            .should("not.be.empty");
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").last().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");
    });

    it("Validate Covid with multiple results", () => {
        cy.get("[data-testid=covid19TabTitle]").click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-06")
            .siblings("[data-testid=dependentCovidTestLabResult]")
            .contains("Negative")
            .siblings("[data-testid=dependent-covid-test-info-button]")
            .click();
        cy.get("[data-testid=dependent-covid-test-info-popover]").should(
            "be.visible"
        );
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-04")
            .siblings("[data-testid=dependentCovidTestLabResult]")
            .contains("Negative");
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-03")
            .siblings("[data-testid=dependentCovidTestStatus]")
            .contains("SomeOtherState");
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-02")
            .siblings("[data-testid=dependentCovidTestStatus]")
            .contains("SomeOtherState");
    });

    it("Validate successful vaccine proof download", () => {
        let isLoading = false;
        cy.intercept(
            "GET",
            "**/AuthenticatedVaccineStatus/pdf?hdid*",
            (req) => {
                if (!isLoading) {
                    req.reply({
                        fixture:
                            "ImmunizationService/vaccineProofNotLoaded.json",
                    });
                } else {
                    req.reply({
                        fixture: "ImmunizationService/vaccineProofLoaded.json",
                    });
                }
                isLoading = !isLoading;
            }
        );

        cy.get("[data-testid=covid19TabTitle]").click();
        cy.get(
            `[data-testid=download-proof-of-vaccination-btn-${validHdid}]`
        ).click({ force: true });
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.verifyDownload("VaccineProof.pdf");
    });

    it("Validate no records found modal", () => {
        let isLoading = false;
        cy.intercept(
            "GET",
            "**/AuthenticatedVaccineStatus/pdf?hdid=*",
            (req) => {
                if (!isLoading) {
                    req.reply({
                        fixture:
                            "ImmunizationService/vaccineProofNotLoaded.json",
                    });
                } else {
                    req.reply({
                        fixture:
                            "ImmunizationService/vaccineProofNotAvailable.json",
                    });
                }
                isLoading = !isLoading;
            }
        );

        cy.get("[data-testid=covid19TabTitle]").click();
        cy.get(
            `[data-testid=download-proof-of-vaccination-btn-${validHdid}]`
        ).click({ force: true });
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-ok-btn]")
            .should("be.visible")
            .click({ force: true });
    });
});

describe("Dependents - Immunization Tab - Enabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.configureSettings({
            dependents: {
                enabled: true,
            },
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
            "/dependents"
        );
    });

    it("Immunization - History Tab - Verify sort", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/dependentImmunization.json",
        });

        cy.log("Validating Immunization Tab - Verify sort");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`).click();

        // History tab
        cy.log("Validating History Tab");
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("button", "History").click();
            }
        );
        cy.get(
            `[data-testid=immunization-history-table-${dependentHdid}]`
        ).should("be.visible");

        // Verify history table has been sorted by due date descending
        cy.get(
            `[data-testid=history-immunization-date-${dependentHdid}-0]`
        ).then(($dateItem) => {
            // Column date in the 1st row in the table
            const firstDate = new Date($dateItem.text().trim());
            cy.get(
                `[data-testid=history-immunization-date-${dependentHdid}-1]`
            ).then(($dateItem) => {
                // Column date in the 2nd row in the table
                const secondDate = new Date($dateItem.text().trim());
                expect(firstDate).to.be.gte(secondDate);
            });
        });
    });

    it("Immunization - Forecast Tab - Verify sort", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/dependentImmunization.json",
        });

        cy.log("Validating Immunization Tab - Verify sort");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`).click();

        // Click download dropdown under Forecasts tab
        cy.log("Validating Forecasts Tab");
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`)
            .contains("button", "Forecasts")
            .click();
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${dependentHdid}]`
        ).click({ force: true });

        // Verify forecast table has been sorted by due date ascending
        cy.get(`[data-testid=forecast-due-date-${dependentHdid}-0]`).then(
            ($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = getDate($dateItem.text());
                cy.get(
                    `[data-testid=forecast-due-date-${dependentHdid}-1]`
                ).then(($dateItem) => {
                    // Column date in the 2nd row in the table
                    const secondDate = getDate($dateItem.text());
                    expect(firstDate).to.be.lte(secondDate);
                    // Column date in the last row in the table
                    cy.get(
                        `[data-testid=forecast-due-date-${dependentHdid}-4]`
                    ).then(($dateItem) => {
                        // Column date in the last row in the table
                        const lastDate = getDate($dateItem.text());
                        expect(firstDate).to.be.lte(lastDate);
                        expect(secondDate).to.be.lte(lastDate);
                    });
                });
            }
        );
    });

    it("Immunization Tab - No Data Found", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunizationNoRecords.json",
        });

        cy.log("Validating Immunization Tab - No Data Found");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`).click();
        cy.get(
            `[data-testid=immunization-history-no-rows-found-${dependentHdid}]`
        ).should("be.visible");
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).should("not.exist");
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("button", "Forecasts").click();
            }
        );
        cy.get(
            `[data-testid=immunization-forecast-no-rows-found-${dependentHdid}]`
        ).should("be.visible");
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${dependentHdid}]`
        ).should("not.exist");
    });
});

describe("Dependents - Lab Results Tab - Enabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.configureSettings({
            dependents: {
                enabled: true,
            },
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
            "/dependents"
        );
    });

    it("Lab Results Tab - Verify result and sort", () => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrders.json",
        });

        cy.log("Validating Lab Results Tab - Verify result and sort");

        cy.get(`[data-testid=lab-results-tab-title-${dependentHdid}]`).click();

        // Expecting 9 rows to return but we also need to consider the table headers.
        cy.get(`[data-testid=lab-results-table-${dependentHdid}]`)
            .find("tr")
            .should("have.length", 10);

        // Verify table has been sorted by date descending
        cy.get(`[data-testid=lab-results-date-${dependentHdid}-0]`).then(
            ($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = getDate($dateItem.text());
                cy.get(
                    `[data-testid=lab-results-date-${dependentHdid}-1]`
                ).then(($dateItem) => {
                    // Column date in the 2nd row in the table
                    const secondDate = getDate($dateItem.text());
                    expect(firstDate).to.be.gte(secondDate);
                });
            }
        );
    });

    it("Lab Results Tab - No Data Found", () => {
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*", {
            fixture: "LaboratoryService/laboratoryOrdersNoRecords.json",
        });

        cy.log("Validating Lab Results Tab - No Data Found");

        cy.get(`[data-testid=lab-results-tab-title-${dependentHdid}]`).click();

        cy.get(`[data-testid=lab-results-no-records-${dependentHdid}]`).should(
            "be.visible"
        );
    });
});

describe("Dependents - Clinical Document Tab - Enabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Clinical Document Tab - Verify result and sort", () => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocument.json",
        });

        cy.log("Validating Clinical Document Tab - Verify result and sort");

        cy.get(
            `[data-testid=clinical-document-tab-title-${dependentHdid}]`
        ).click();

        // Expecting 2 rows to return but we also need to consider the table headers.
        cy.get(`[data-testid=clinical-document-table-${dependentHdid}]`)
            .find("tr")
            .should("have.length", 3);

        // Verify document table has been sorted by due date descending
        cy.get(
            `[data-testid=clinical-document-service-date-${dependentHdid}-0]`
        ).then(($dateItem) => {
            // Column date in the 1st row in the table
            const firstDate = getDate($dateItem.text());
            cy.get(
                `[data-testid=clinical-document-service-date-${dependentHdid}-1]`
            ).then(($dateItem) => {
                // Column date in the 2nd row in the table
                const secondDate = getDate($dateItem.text());
                expect(firstDate).to.be.gte(secondDate);
            });
        });
    });

    it("Clinical Document Tab - No Data Found", () => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocumentNoRecords.json",
        });

        cy.log("Validating Clinical Document Tab - No Data Found");

        cy.get(
            `[data-testid=clinical-document-tab-title-${dependentHdid}]`
        ).click();

        cy.get(
            `[data-testid=clinical-document-no-records-${dependentHdid}]`
        ).should("be.visible");
    });
});

describe("Dependents - Tabs Disabled", () => {
    const dependentHdid = "645645767756756767";

    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.configureSettings({
            dependents: {
                enabled: true,
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Immunization and Clinical Documents Tabs - Configuration Disabled", () => {
        cy.log("Validating Immunization Tab - configuration disabled");
        cy.get(`[data-testid=immunization-tab-${dependentHdid}]`).should(
            "not.be.visible"
        );
        cy.log("Validating Clinical Documents Tab - configuration disabled");
        cy.get(`[data-testid=clinical-document-tab-${dependentHdid}]`).should(
            "not.be.visible"
        );
    });
});
