const { AuthMethod } = require("../../../support/constants");

describe("dependents", () => {
    const validDependent = {
        firstName: "Sam",
        lastName: "Testfive",
        wrongLastName: "Testfive2",
        invalidDoB: "2007-Aug-05",
        doB: "2014-Mar-15",
        testDate: "2020-Mar-21",
        phn: "9874307168",
    };

    const noHdidDependent = {
        firstName: "Baby Girl",
        lastName: "Reid",
        doB: "2018-Feb-04",
        testDate: "2020-Mar-21",
        phn: "9879187222",
    };

    before(() => {
        cy.setupDownloads();
        cy.enableModules(["CovidLabResults", "Laboratory", "Dependent"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate Add, Fields and Cancel", () => {
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

        // Validate tesDate Input
        cy.get("[data-testid=testDateInput] input").should("be.enabled");

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
        cy.get("[data-testid=testDateInput] input").type(
            validDependent.testDate
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
        cy.get("[data-testid=testDateInput] input")
            .clear()
            .type(validDependent.testDate);
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
        cy.get("[data-testid=testDateInput] input")
            .clear()
            .type(noHdidDependent.testDate);
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

    it("Validate Add", () => {
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
        cy.get("[data-testid=testDateInput] input")
            .clear()
            .type(validDependent.testDate);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is done
        cy.get("[data-testid=newDependentModal]").should("not.exist");
    });

    it("Validate Dependent Tab", () => {
        cy.get("[data-testid=loadingSpinner]").should("not.be.visible");
        // Validate the newly added dependent tab and elements are present
        cy.get("[data-testid=dependentName]")
            .last()
            .contains(validDependent.firstName)
            .contains(validDependent.lastName);
        cy.get("[data-testid=dependentPHN]")
            .last()
            .invoke("val")
            .then((phnNumber) =>
                expect(phnNumber).to.equal(validDependent.phn)
            );
        cy.get("[data-testid=dependentDOB]")
            .last()
            .invoke("val")
            .then((dateOfBirth) =>
                expect(dateOfBirth).to.equal(validDependent.doB)
            );
    });

    it("Validate Covid Tab with Results", () => {
        let sensitiveDocMessage =
            " The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ";
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
        expect(cy.get("[data-testid=dependentCovidTestLocation]").last()).not.to
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

    it("Validate Add for another user", () => {
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
        cy.get("[data-testid=testDateInput] input")
            .clear()
            .type(validDependent.testDate);
        cy.get("[data-testid=phnInput]").clear().type(validDependent.phn);
        cy.get("[data-testid=termsCheckbox]").check({ force: true });

        cy.get("[data-testid=registerDependentBtn]").click();

        // Validate the modal is done
        cy.get("[data-testid=newDependentModal]").should("not.exist");

        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get("[data-testid=dependentMenuBtn]").last().click();
        cy.get("[data-testid=deleteDependentMenuBtn]").last().click();
        cy.get("[data-testid=confirmDeleteBtn]").click();
    });

    it("Validate Remove Dependent", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );

        cy.get("[data-testid=dependentMenuBtn]").last().click();
        cy.get("[data-testid=deleteDependentMenuBtn]").last().click();
        cy.get("[data-testid=confirmDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").click();

        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get("[data-testid=dependentMenuBtn]").last().click();
        cy.get("[data-testid=deleteDependentMenuBtn]").last().click();
        cy.get("[data-testid=confirmDeleteBtn]").click();
    });
});
