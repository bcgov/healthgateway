const { AuthMethod } = require("../../../support/constants");

const sensitiveDocMessage =
    " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";
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
        cy.enableModules(["CovidLabResults", "Laboratory", "Dependent"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate Covid Tab with Results", () => {
        // Validate the tab and elements are present
        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .first()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").first().click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");

        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .last()
            .contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
        expect(cy.get("[data-testid=dependentCovidTestStatus]").last()).not.to
            .be.empty;
        expect(cy.get("[data-testid=dependentCovidTestLabResult]").last()).not
            .to.be.empty;
        cy.get("[data-testid=dependentCovidReportDownloadBtn]").last().click();
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");
    });

    it("Validate Covid with multiple results", () => {
        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get("[data-testid=dependentCovidTestDate]")
            .contains("td", "2020-Oct-06")
            .siblings("[data-testid=dependentCovidTestLabResult]")
            .contains("Negative")
            .next()
            .find("[data-testid=dependent-covid-test-info-button]")
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
});

describe("COVID-19 - Vaccine Proof download", () => {
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

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.enableModules([
            "CovidLabResults",
            "Laboratory",
            "Dependent",
            "Immunization",
        ]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );

        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get(
            `[data-testid=download-proof-of-vaccination-btn-${validHdid}]`
        ).click({ force: true });
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.wait(1000);
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
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

        cy.intercept("GET", "**/Laboratory/Covid19Orders*", {
            fixture: "LaboratoryService/covid19Orders.json",
        });
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });
        cy.enableModules([
            "CovidLabResults",
            "Laboratory",
            "Dependent",
            "Immunization",
        ]);

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );

        cy.get("[data-testid=covid19TabTitle]").last().parent().click();
        cy.get(
            `[data-testid=download-proof-of-vaccination-btn-${validHdid}]`
        ).click({ force: true });
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageOkBtn]")
            .should("be.visible")
            .click({ force: true });
    });
});

describe("Dependents - Immuniazation Tab - Enabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.enableModules([
            "Dependent",
            "Immunization",
            "DependentImmunizationTab",
            "DependentClinicalDocumentTab",
            "ClinicalDocument",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Immunization - History - Tab - Verify sort and download", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/dependentImmunization.json",
        });

        cy.log("Validating Immunization Tab - configuration enabled");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .parent()
            .click();

        // History tab
        cy.log("Validating History Tab");
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("a", "History").click();
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

        // Click download dropdown under History tab
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click PDF
        cy.get(
            `[data-testid=download-immunization-history-report-pdf-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.pdf", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click CSV
        cy.get(
            `[data-testid=download-immunization-history-report-csv-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.csv", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).click();

        // Click XLSX
        cy.get(
            `[data-testid=download-immunization-history-report-xlsx-btn-${dependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Immunization - Forecast - Tab - Verify sort and download", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/dependentImmunization.json",
        });

        cy.log("Validating Immunization Tab - configuration enabled");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .parent()
            .click();

        // Click download dropdown under Forecasts tab
        cy.log("Validating Forecasts Tab");
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${dependentHdid}]`
        ).click({ force: true });

        // Verify forecast table has been sorted by due date descending

        cy.get(`[data-testid=forecast-due-date-${dependentHdid}-0]`).then(
            ($dateItem) => {
                // Column date in the 1st row in the table
                const firstDate = getDate($dateItem.text());
                cy.get(
                    `[data-testid=forecast-due-date-${dependentHdid}-1]`
                ).then(($dateItem) => {
                    // Column date in the 2nd row in the table
                    const secondDate = getDate($dateItem.text());
                    expect(firstDate).to.be.gte(secondDate);
                    // Column date in the last row in the table
                    cy.get(
                        `[data-testid=forecast-due-date-${dependentHdid}-4]`
                    ).then(($dateItem) => {
                        // Column date in the last row in the table
                        const lastDate = getDate($dateItem.text());
                        expect(firstDate).to.be.gte(lastDate);
                        expect(secondDate).to.be.gte(lastDate);
                    });
                });
            }
        );

        // Click PDF
        cy.get(
            `[data-testid=download-immunization-forecast-report-pdf-btn-${dependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.pdf", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${dependentHdid}]`
        ).click({ force: true });

        // Click CSV
        cy.get(
            `[data-testid=download-immunization-forecast-report-csv-btn-${dependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.csv", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${dependentHdid}]`
        ).click({ force: true });

        // Click XLSX
        cy.get(
            `[data-testid=download-immunization-forecast-report-xlsx-btn-${dependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Immunization Tab - No Data Found", () => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunizationNoRecords.json",
        });

        cy.log("Validating Immunization Tab - No Data Found");

        cy.get(`[data-testid=immunization-tab-title-${dependentHdid}]`)
            .parent()
            .click();
        cy.get(
            `[data-testid=immunization-history-no-rows-found-${dependentHdid}]`
        ).should("be.visible");
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${dependentHdid}]`
        ).should("not.exist");
        cy.get(`[data-testid=immunization-tab-div-${dependentHdid}]`).within(
            () => {
                cy.contains("a", "Forecasts").click();
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

describe("Dependents - Clinical Document Tab - Enabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*/Dependent", {
            fixture: "UserProfileService/dependent.json",
        });

        cy.enableModules([
            "Dependent",
            "DependentClinicalDocumentTab",
            "ClinicalDocument",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Clinical Document Tab - No Data Found", () => {
        cy.intercept("GET", "**/ClinicalDocument/*", {
            fixture: "ClinicalDocumentService/clinicalDocumentNoRecords.json",
        });

        cy.log("Validating Clinical Document Tab - No Data Found");

        cy.get(`[data-testid=clinical-docuemnt-tab-title-${dependentHdid}]`)
            .parent()
            .click();

        cy.get(
            `[data-testid=clinical-document-no-records-${dependentHdid}]`
        ).should("be.visible");
    });
});

describe("Dependents Tabs Disabled", () => {
    const dependentHdid = "645645767756756767";
    beforeEach(() => {
        cy.enableModules(["Dependent"]);
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
            "not.exist"
        );
        cy.log("Validating Clinical Documents Tab - configuration disabled");
        cy.get(`[data-testid=clinical-document-tab-${dependentHdid}]`).should(
            "not.exist"
        );
    });
});
