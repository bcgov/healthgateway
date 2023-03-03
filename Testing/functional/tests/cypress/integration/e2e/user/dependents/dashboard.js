const { AuthMethod } = require("../../../../support/constants");

const validDependent = {
    firstName: "Sam ", // Aooend space to ensure field is trimmed
    lastName: "Testfive ", // Aooend space to ensure field is trimmed
    name: "Sam T",
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

const agedOutDependentHdid = "232434345442257";
const agedOoutDependentCardId = `[data-testid=dependent-card-${agedOutDependentHdid}]`;
const agedOoutDependentDivId = `[data-testid=dependent-is-expired-div-${agedOutDependentHdid}]`;
const agedOutDependentName = "John T";
const validDependentHdid = "162346565465464564565463257";
const validDependentTimelinePath = `/dependents/${validDependentHdid}/timeline`;
const validDependentClinicalDocumentsButtonId = `[data-testid=dependent-entry-type-ClinicalDocument-${validDependentHdid}]`;
const validDependentCovid19TestResultsButtonId = `[data-testid=dependent-entry-type-Covid19TestResult-${validDependentHdid}]`;
const validDependentFederalProofOfVaccinationButtonId = `[data-testid=proof-vaccination-card-btn-${validDependentHdid}]`;
const validDependentImmunizationsButtonId = `[data-testid=dependent-entry-type-Immunization-${validDependentHdid}]`;
const validDependentLabResultsButtonId = `[data-testid=dependent-entry-type-LabResult-${validDependentHdid}]`;

function validateDashboardCard(buttonId, filterType) {
    cy.get(buttonId).should("be.enabled", "be.visible").click();
    cy.location("pathname").should("eq", validDependentTimelinePath);
    cy.checkTimelineHasLoaded();

    cy.get("[data-testid=filterContainer]").should("not.exist");
    cy.get("[data-testid=filterDropdown]").click();
    cy.get(`[data-testid=${filterType}-filter]`).should("to.be.checked");
    cy.get("[data-testid=btnFilterCancel]").click();
}

describe("dependents - dashboard", () => {
    beforeEach(() => {
        cy.configureSettings({
            homepage: {
                showFederalProofOfVaccination: true,
            },
            dependents: {
                enabled: true,
                timelineEnabled: true,
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

    it("Validate dashboard immunizations tab click to timeline", () => {
        validateDashboardCard(
            validDependentImmunizationsButtonId,
            "Immunization"
        );
    });

    it("Validate dashboard lab results tab click to timeline", () => {
        validateDashboardCard(validDependentLabResultsButtonId, "LabResult");
    });

    it("Validate dashboard covid19 test results tab click to timeline", () => {
        validateDashboardCard(
            validDependentCovid19TestResultsButtonId,
            "Covid19TestResult"
        );
    });

    it("Validate dashboard clinical documents tab click to timeline", () => {
        validateDashboardCard(
            validDependentClinicalDocumentsButtonId,
            "ClinicalDocument"
        );
    });

    it("Validate dashboard aged out dependent and remove", () => {
        cy.get(agedOoutDependentCardId)
            .as("agedOutDependentCard")
            .within(() => {
                cy.get("[data-testid=dependentName]").contains(
                    agedOutDependentName
                );
                cy.get(agedOoutDependentDivId).should("be.visible");
            });

        cy.get("@agedOutDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });

        cy.get("[data-testid=confirmDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").should("be.visible");
        cy.get("[data-testid=cancelDeleteBtn]").click();

        cy.get("@agedOutDependentCard").within(() => {
            cy.get("[data-testid=dependentMenuBtn]").click();
            cy.get("[data-testid=deleteDependentMenuBtn]").click();
        });

        cy.get("[data-testid=confirmDeleteBtn]").click();
        cy.get("@agedOutDependentCard").should("not.exist");
    });

    it("Validate download of federal proof of vaccination", () => {
        cy.intercept("GET", "**/AuthenticatedVaccineStatus/pdf?hdid=*");

        cy.get(validDependentFederalProofOfVaccinationButtonId)
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=genericMessageModal]").should("be.visible");
        cy.get("[data-testid=genericMessageSubmitBtn]").click();

        cy.get("[data-testid=loadingSpinner]").should("be.visible");
        cy.verifyDownload("VaccineProof.pdf", {
            timeout: 60000,
            interval: 5000,
        });
    });

    it("Validate text fields on add dependent modal", () => {
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

    it("Validate maximum age check", () => {
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

    it("Validate data mismatch", () => {
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

    it("Validate no hdid", () => {
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

    it("Validate adding, viewing, and removing dependents", () => {
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
        cy.get(`[data-testid=dependent-card-${validDependent.hdid}]`)
            .as("newDependentCard")
            .within(() => {
                // Validate the newly added dependent tab and elements are present
                cy.get("[data-testid=dependentName]").contains(
                    validDependent.name
                );
            });

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

        cy.configureSettings({
            dependents: {
                enabled: true,
                timelineEnabled: true,
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
