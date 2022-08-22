const { AuthMethod } = require("../../../support/constants");

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

    const noHdidDependent = {
        firstName: "Baby Girl",
        lastName: "Reid",
        doB: "2018-Feb-04",
        phn: "9879187222",
    };

    const validDependentHdid = "162346565465464564565463257";

    beforeEach(() => {
        cy.enableModules([
            "CovidLabResults",
            "Immunization",
            "Laboratory",
            "Dependent",
            "DependentImmunizationTab",
        ]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate Text Fields on Add Dependent Modal", () => {
        //Validate Main Add Button
        cy.get("[data-testid=addNewDependentBtn]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );
        //Validate First Name
        cy.get("[data-testid=firstNameInput]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");
        // Validate Last Name
        cy.get("[data-testid=lastNameInput]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");
        //Validate Date of Birth
        cy.get("[data-testid=dateOfBirthInput] input").should("be.enabled");
        // Validate PHN input
        cy.get("[data-testid=phnInput]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");

        // Validate Cancel out of the form
        cy.get("[data-testid=cancelRegistrationBtn]")
            .should("be.enabled", "be.visible")
            .click();
        // Validate the modal is done
        cy.get("[data-testid=newDependentModal]").should("not.exist");
    });

    it("Validate Maximum Age Check", () => {
        // Validate that adding a dependent fails when they are over the age of 12
        cy.get("[data-testid=addNewDependentBtn]").click();
        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=firstNameInput]").type(validDependent.firstName);
        cy.get("[data-testid=lastNameInput]").type(validDependent.lastName);
        cy.get("[data-testid=dateOfBirthInput] input").type(
            validDependent.invalidDoB
        );
        cy.get("[data-testid=phnInput]").type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal has not closed
        cy.get("[data-testid=newDependentModal]").should("exist");

        cy.get("[data-testid=cancelRegistrationBtn]").click();
    });

    it("Validate Data Mismatch", () => {
        cy.get("[data-testid=addNewDependentBtn]").click();

        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=firstNameInput]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=lastNameInput]")
            .clear()
            .type(validDependent.wrongLastName);
        cy.get("[data-testid=dateOfBirthInput] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is not done
        cy.get("[data-testid=newDependentModal]").should("exist");
        cy.get("[data-testid=dependentErrorText]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancelRegistrationBtn]").click();
    });

    it("Validate No HDID", () => {
        cy.get("[data-testid=addNewDependentBtn]").click();

        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=firstNameInput]")
            .clear()
            .type(noHdidDependent.firstName);
        cy.get("[data-testid=lastNameInput]")
            .clear()
            .type(noHdidDependent.lastName);
        cy.get("[data-testid=dateOfBirthInput] input")
            .clear()
            .type(noHdidDependent.doB);
        cy.get("[data-testid=phnInput]").clear().type(noHdidDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is not done
        cy.get("[data-testid=newDependentModal]").should("exist");
        cy.get("[data-testid=dependentErrorText]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancelRegistrationBtn]").click();
    });

    it("Validate Immunization - History - Tab", () => {
        cy.log("Validating Immunization Tab");

        cy.get(
            "[data-testid=immunization-tab-title-" + validDependentHdid + "]"
        )
            .parent()
            .click();

        // History tab
        cy.log("Validating history tab");
        cy.get(
            "[data-testid=immunization-tab-div-" + validDependentHdid + "]"
        ).within(() => {
            cy.contains("a", "History").click();
        });
        cy.get(
            "[data-testid=immunization-history-table-" +
                validDependentHdid +
                "]"
        ).should("be.visible");

        // Click download dropdown under History tab
        cy.get(
            "[data-testid=download-immunization-history-report-btn-" +
                validDependentHdid +
                "]"
        ).click();

        // Click PDF
        cy.get(
            "[data-testid=download-immunization-history-report-pdf-btn-" +
                validDependentHdid +
                "]"
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
            "[data-testid=download-immunization-history-report-btn-" +
                validDependentHdid +
                "]"
        ).click();

        // Click CSV
        cy.get(
            "[data-testid=download-immunization-history-report-csv-btn-" +
                validDependentHdid +
                "]"
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
            "[data-testid=download-immunization-history-report-btn-" +
                validDependentHdid +
                "]"
        ).click();

        // Click XLSX
        cy.get(
            "[data-testid=download-immunization-history-report-xlsx-btn-" +
                validDependentHdid +
                "]"
        ).click();

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate Immunization - Forecast - Tab", () => {
        cy.log("Validating Immunization Tab - configuration enabled");

        cy.get(
            "[data-testid=immunization-tab-title-" + validDependentHdid + "]"
        )
            .parent()
            .click();

        // Click download dropdown under Forecasts tab
        cy.log("Validating forecasts tab");
        cy.get(
            "[data-testid=download-immunization-forecast-report-btn-" +
                validDependentHdid +
                "]"
        ).click({ force: true });

        // Click PDF
        cy.get(
            "[data-testid=download-immunization-forecast-report-pdf-btn-" +
                validDependentHdid +
                "]"
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
            "[data-testid=download-immunization-forecast-report-btn-" +
                validDependentHdid +
                "]"
        ).click({ force: true });

        // Click CSV
        cy.get(
            "[data-testid=download-immunization-forecast-report-csv-btn-" +
                validDependentHdid +
                "]"
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
            "[data-testid=download-immunization-forecast-report-btn-" +
                validDependentHdid +
                "]"
        ).click({ force: true });

        // Click XLSX
        cy.get(
            "[data-testid=download-immunization-forecast-report-xlsx-btn-" +
                validDependentHdid +
                "]"
        ).click({ force: true });

        // Confirmation modal
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.verifyDownload("HealthGatewayDependentImmunizationReport.xlsx", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate Adding, Viewing, and Removing Dependents", () => {
        cy.log("Adding dependent");

        cy.get("[data-testid=addNewDependentBtn]").click();
        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=firstNameInput]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=lastNameInput]")
            .clear()
            .type(validDependent.lastName);
        cy.get("[data-testid=dateOfBirthInput] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is done
        cy.get("[data-testid=newDependentModal]").should("not.exist");

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
        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageText]").should(
            "have.text",
            sensitiveDocMessage
        );
        cy.get("[data-testid=genericMessageSubmitBtn]").click();
        cy.get("[data-testid=genericMessageModal]").should("not.exist");

        cy.log("Adding same dependent as another user");

        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
        cy.get("[data-testid=addNewDependentBtn]").click();

        cy.get("[data-testid=newDependentModalText]").should(
            "exist",
            "be.visible"
        );

        cy.get("[data-testid=firstNameInput]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=lastNameInput]")
            .clear()
            .type(validDependent.lastName);
        cy.get("[data-testid=dateOfBirthInput] input")
            .clear()
            .type(validDependent.doB);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is done
        cy.get("[data-testid=newDependentModal]").should("not.exist");

        cy.log("Removing dependent from other user");
        cy.get("@newDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });
        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get("[data-testid=confirmDeleteBtn]").click();

        cy.log("Removing dependent from original user");

        cy.enableModules(["CovidLabResults", "Laboratory", "Dependent"]);
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
