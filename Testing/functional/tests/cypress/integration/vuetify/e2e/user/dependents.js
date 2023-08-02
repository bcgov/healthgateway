const { AuthMethod } = require("../../../../support/constants");

function triggerEmptyValidation(vuetifySelector) {
    cy.get(vuetifySelector + " input")
        .should("be.enabled")
        .clear()
        .blur();
    cy.get(vuetifySelector).should("have.class", "v-input--error");
}

describe("dependents", () => {
    const validDependent = {
        firstName: "Sam ", // Aooend space to ensure field is trimmed
        lastName: "Testfive ", // Aooend space to ensure field is trimmed
        wrongLastName: "Testfive2",
        invalidDoB: "2007-Aug-05",
        doB: "2014-Mar-15",
        phn: "9874307168",
        hdid: "645645767756756767",
    };

    const protectedDependentWithAllowedDelegation = {
        firstName: "Leroy Desmond",
        lastName: "Tobias",
        doB: "2015-Dec-02",
        phn: "9872868128",
        hdid: "35224807075386200",
    };

    const protectedDependentWithoutAllowedDelegation = {
        firstName: "Lenny Francesco",
        lastName: "Dansereau",
        doB: "2019-May-14",
        phn: "9872868103",
    };

    const noHdidDependent = {
        firstName: "Baby Girl",
        lastName: "Reid",
        doB: "2018-Feb-04",
        phn: "9879187222",
    };

    const validDependentHdid = "162346565465464564565463257";

    beforeEach(() => {
        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it.skip("Validate Text Fields on Add Dependent Modal", () => {
        //Validate Main Add Button
        cy.get("[data-testid=add-dependent-button]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );
        //Validate First Name
        triggerEmptyValidation("[data-testid=dependent-first-name-input]");
        // Validate Last Name
        triggerEmptyValidation("[data-testid=dependent-last-name-input]");
        //Validate Date of Birth
        cy.get("[data-testid=dependent-date-of-birth-input] input").should(
            "be.enabled"
        );
        // Validate PHN input
        triggerEmptyValidation("[data-testid=dependent-phn-input]");
        // Validate Cancel out of the form
        cy.get("[data-testid=cancel-dependent-registration-btn]")
            .should("be.enabled", "be.visible")
            .click();
        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");
    });

    it.skip("Validate Maximum Age Check", () => {
        // Validate that adding a dependent fails when they are over the age of 12
        cy.get("[data-testid=add-dependent-button]").click();
        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=dependent-first-name-input]").type(
            validDependent.firstName
        );
        cy.get("[data-testid=dependent-last-name-input]").type(
            validDependent.lastName
        );
        cy.get("[data-testid=dependent-date-of-birth-input]")
            .type(validDependent.invalidDoB)
            .should("have.class", "v-input--error");
        cy.get("[data-testid=dependent-phn-input]").type(validDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]")
            .should("be.disabled")
            .click({ force: true });

        // Validate the modal has not closed
        cy.get("[data-testid=add-dependent-dialog]").should("exist");

        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it.skip("Validate Data Mismatch", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(validDependent.wrongLastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=dependent-phn-input] input")
            .clear()
            .type(validDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is not done
        cy.get("[data-testid=add-dependent-dialog]").should("exist");
        cy.get("[data-testid=dependent-error-text]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=not-condensed-error-contact-message]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it.skip("Validate Add Protected PHN Without Allowed Delegation", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(protectedDependentWithoutAllowedDelegation.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(protectedDependentWithoutAllowedDelegation.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(protectedDependentWithoutAllowedDelegation.doB);
        cy.get("[data-testid=dependent-phn-input] input")
            .clear()
            .type(protectedDependentWithoutAllowedDelegation.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is not done
        cy.get("[data-testid=add-dependent-dialog]").should("exist");
        cy.get("[data-testid=dependent-error-text]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=condensed-error-contact-message]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it.skip("Validate Add Protected PHN With Allowed Delegation", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(protectedDependentWithAllowedDelegation.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(protectedDependentWithAllowedDelegation.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(protectedDependentWithAllowedDelegation.doB);
        cy.get("[data-testid=dependent-phn-input] input")
            .clear()
            .type(protectedDependentWithAllowedDelegation.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");

        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.get(
            `[data-testid=dependent-card-${protectedDependentWithAllowedDelegation.phn}]`
        )
            .as("newDependentCard")
            .within(() => {
                // Validate the newly added dependent tab and elements are present
                cy.get("[data-testid=dependentName]")
                    .contains(protectedDependentWithAllowedDelegation.firstName)
                    .contains(protectedDependentWithAllowedDelegation.lastName);

                cy.get("[data-testid=dependentPHN]").should(
                    "have.value",
                    protectedDependentWithAllowedDelegation.phn
                );

                cy.get("[data-testid=dependentDOB]").should(
                    "have.value",
                    protectedDependentWithAllowedDelegation.doB
                );
            });
    });

    it.skip("Validate No HDID", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input] input")
            .clear()
            .type(noHdidDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input] input")
            .clear()
            .type(noHdidDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(noHdidDependent.doB);
        cy.get("[data-testid=dependent-phn-input] input")
            .clear()
            .type(noHdidDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is not done
        cy.get("[data-testid=add-dependent-dialog]").should("exist");
        cy.get("[data-testid=dependent-error-text]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it("Validate Immunization History - Verify result and download", () => {
        cy.log(
            "Validating Immunization History Tab - Verify result and download"
        );

        cy.get(`[data-testid=immunization-tab-title-${validDependentHdid}]`)
            .parent()
            .click();

        // History tab
        cy.log("Validating history tab");
        cy.get(
            `[data-testid=immunization-tab-div-${validDependentHdid}]`
        ).within(() => {
            cy.contains("a", "History").click();
        });
        // Expecting more than 1 row to return because we also need to consider the table headers.
        cy.get(`[data-testid=immunization-history-table-${validDependentHdid}]`)
            .find("tr")
            .should("have.length.greaterThan", 1);

        // Click download dropdown under History tab
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${validDependentHdid}]`
        ).click();

        // Click PDF
        cy.get(
            `[data-testid=download-immunization-history-report-pdf-btn-${validDependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.pdf", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown under History tab
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${validDependentHdid}]`
        ).click();

        // Click CSV
        cy.get(
            `[data-testid=download-immunization-history-report-csv-btn-${validDependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.csv", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown under History tab
        cy.get(
            `[data-testid=download-immunization-history-report-btn-${validDependentHdid}]`
        ).click();

        // Click XLSX
        cy.get(
            `[data-testid=download-immunization-history-report-xlsx-btn-${validDependentHdid}]`
        ).click();

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate Immunization Forecast - Verify result and download", () => {
        cy.log(
            "Validating Immunization Forecast Tab - Verify result and download"
        );

        cy.get(`[data-testid=immunization-tab-title-${validDependentHdid}]`)
            .parent()
            .click();

        // Forecast tab
        cy.log("Validating forecast tab");
        cy.get(
            `[data-testid=immunization-tab-div-${validDependentHdid}]`
        ).within(() => {
            cy.contains("a", "Forecast").click();
        });

        // Expecting more than 1 row to return because we also need to consider the table headers.
        cy.get(
            `[data-testid=immunization-forecast-table-${validDependentHdid}]`
        )
            .find("tr")
            .should("have.length.greaterThan", 1);

        // Click download dropdown under Forecasts tab
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${validDependentHdid}]`
        ).click({ force: true });

        // Click PDF
        cy.get(
            `[data-testid=download-immunization-forecast-report-pdf-btn-${validDependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.pdf", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown under Forecasts tab
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${validDependentHdid}`
        ).click({ force: true });

        // Click CSV
        cy.get(
            `[data-testid=download-immunization-forecast-report-csv-btn-${validDependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.csv", {
            timeout: 60000,
            interval: 5000,
        });

        // Click download dropdown under Forecasts tab
        cy.get(
            `[data-testid=download-immunization-forecast-report-btn-${validDependentHdid}]`
        ).click({ force: true });

        // Click XLSX
        cy.get(
            `[data-testid=download-immunization-forecast-report-xlsx-btn-${validDependentHdid}]`
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate Lab Results - Verify result and download", () => {
        cy.log("Validating Lab Results Tab - Verify result and download");

        cy.get(`[data-testid=lab-results-tab-title-${validDependentHdid}]`)
            .parent()
            .click();

        // Expecting more than 1 row to return because also need to consider the table headers.
        cy.get(`[data-testid=lab-results-table-${validDependentHdid}]`)
            .find("tr")
            .should("have.length.greaterThan", 1);

        cy.get(
            `[data-testid=lab-results-report-download-button-${validDependentHdid}-0]`
        ).click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("Laboratory_Report_JENNIFER_TESTFOUR", {
            timeout: 60000,
            interval: 5000,
            contains: true,
        });
    });

    it("Validate Clinical Document - Verify result and download", () => {
        cy.log("Validating Clinical Document Tab - Verify result and download");

        cy.get(
            `[data-testid=clinical-document-tab-title-${validDependentHdid}]`
        )
            .parent()
            .click();

        // Expecting more than 1 row to return because also need to consider the table headers.
        cy.get(`[data-testid=clinical-document-table-${validDependentHdid}]`)
            .find("tr")
            .should("have.length.greaterThan", 1);

        cy.get(
            `[data-testid=clinical-document-report-download-button-${validDependentHdid}-0]`
        ).click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("Clinical_Document_JENNIFER_TESTFOUR.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate Adding, Viewing, and Removing Dependents", () => {
        cy.log("Adding dependent");

        cy.get("[data-testid=add-dependent-button]").click();
        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input]")
            .clear()
            .type(validDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=dependent-phn-input]")
            .clear()
            .type(validDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");

        cy.log("Validating dependent tab");

        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        cy.get(`[data-testid=dependent-card-${validDependent.phn}]`)
            .as("newDependentCard")
            .within(() => {
                // Validate the newly added dependent tab and elements are present
                cy.get("[data-testid=dependentName]")
                    .contains(validDependent.firstName)
                    .contains(validDependent.lastName);

                cy.get("[data-testid=dependentPHN]").should(
                    "have.value",
                    validDependent.phn
                );

                cy.get("[data-testid=dependentDOB]").should(
                    "have.value",
                    validDependent.doB
                );
            });

        cy.log("Validating COVID-19 tab");

        cy.get("@newDependentCard").within(() => {
            // Validate the tab and elements are present
            cy.get("[data-testid=covid19TabTitle]").parent().click();
            cy.get("[data-testid=dependentCovidTestDate]").each(($date) => {
                cy.wrap($date).contains(/\d{4}-[A-Z]{1}[a-z]{2}-\d{2}/);
            });
            cy.get("[data-testid=dependentCovidTestStatus]").each(($status) => {
                cy.wrap($status).should("not.be.empty");
            });
        });

        cy.setupDownloads();
        const sensitiveDocMessage =
            " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";

        cy.get("[data-testid=dependentCovidReportDownloadBtn]").first().click();
        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-text]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=generic-message-submit-btn]").click();
        cy.get("[data-testid=generic-message-modal]").should("not.exist");

        cy.log("Adding same dependent as another user");

        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=dependent-first-name-input]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input]")
            .clear()
            .type(validDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=dependent-phn-input]")
            .clear()
            .type(validDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");

        cy.log("Removing dependent from other user");
        cy.get("@newDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });
        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get("[data-testid=confirmDeleteBtn]").click();

        cy.log("Removing dependent from original user");

        cy.configureSettings({
            dependents: {
                enabled: true,
            },
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );

        cy.get("@newDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });
        cy.get("[data-testid=confirmDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").click();

        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get("@newDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });
        cy.get("[data-testid=confirmDeleteBtn]").click();

        cy.log("Validating Immunization tab - module disabled");
        cy.get(`[data-testid=immunization-tab-${validDependent.hdid}]`).should(
            "not.exist"
        );
    });
});
