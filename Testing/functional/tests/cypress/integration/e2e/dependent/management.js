const { AuthMethod } = require("../../../support/constants");
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../support/functions/dependent";

const validDependent = {
    firstName: "Sam ", // Append space to ensure field is trimmed
    lastName: "Testfive ", // Append space to ensure field is trimmed
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
const agedOutDependentDivId = `[data-testid=dependent-is-expired-div-${agedOutDependentHdid}]`;
const agedOutDependentName = "John T";
const agedOutDependentRemoveButtonId = `[data-testid=remove-dependent-btn-${agedOutDependentHdid}]`;

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

    it("Validate and remove aged out dependent", () => {
        const hdid = agedOutDependentHdid;

        cy.get(getCardSelector(hdid))
            .as("agedOutDependentCard")
            .within(() => {
                cy.get("[data-testid=dependentName]").contains(
                    agedOutDependentName
                );
                cy.get(agedOutDependentDivId).should("be.visible");

                const profileTabButtonSelector = getTabButtonSelector(
                    hdid,
                    "profile"
                );
                cy.get(profileTabButtonSelector)
                    .should("be.visible")
                    .should("have.class", "disabled");
            });

        cy.get("@agedOutDependentCard").within(() => {
            cy.get(agedOutDependentRemoveButtonId).click();
        });

        cy.get("@agedOutDependentCard").should("not.exist");
    });

    it("Validate text fields on add dependent modal", () => {
        //Validate Main Add Button
        cy.get("[data-testid=add-dependent-button]")
            .should("be.enabled", "be.visible")
            .click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );
        //Validate First Name
        cy.get("[data-testid=dependent-first-name-input]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");
        // Validate Last Name
        cy.get("[data-testid=dependent-last-name-input]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");
        //Validate Date of Birth
        cy.get("[data-testid=dependent-date-of-birth-input] input").should(
            "be.enabled"
        );
        // Validate PHN input
        cy.get("[data-testid=dependent-phn-input]")
            .should("be.enabled")
            .clear()
            .blur()
            .should("have.class", "is-invalid");

        // Validate Cancel out of the form
        cy.get("[data-testid=cancel-dependent-registration-btn]")
            .should("be.enabled", "be.visible")
            .click();
        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");
    });

    it("Validate maximum age check on add dependent modal", () => {
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
        cy.get("[data-testid=dependent-date-of-birth-input] input").type(
            validDependent.invalidDoB
        );
        cy.get("[data-testid=dependent-phn-input]").type(validDependent.phn);
        cy.get("[data-testid=dependent-terms-checkbox] input").check({
            force: true,
        });

        cy.get("[data-testid=register-dependent-btn]").click();

        // Validate the modal has not closed
        cy.get("[data-testid=add-dependent-dialog]").should("exist");

        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it("Validate data mismatch on add dependent modal", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );data-testid=dependent-first-name-input

        cy.get("[data-testid=dependent-first-name-input]")
            .clear()
            .type(validDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input]")
            .clear()
            .type(validDependent.wrongLastName);
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

        // Validate the modal is not done
        cy.get("[data-testid=add-dependent-dialog]").should("exist");
        cy.get("[data-testid=dependent-error-text]").should(
            "exist",
            "be.visible",
            "not.be.empty"
        );
        cy.get("[data-testid=cancel-dependent-registration-btn]").click();
    });

    it("Validate no hdid on add dependent modal", () => {
        cy.get("[data-testid=add-dependent-button]").click();

        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );data-testid=dependent-first-name-input

        cy.get("[data-testid=dependent-first-name-input]")
            .clear()
            .type(noHdidDependent.firstName);
        cy.get("[data-testid=dependent-last-name-input]")
            .clear()
            .type(noHdidDependent.lastName);
        cy.get("[data-testid=dependent-date-of-birth-input] input")
            .clear()
            .type(noHdidDependent.doB);
        cy.get("[data-testid=dependent-phn-input]")
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

    // test should be skipped until the similar test for the old dependent page is removed
    it.skip("Validate adding, viewing, duplicate prevention and removing dependents", () => {
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
        cy.get(`[data-testid=dependent-card-${validDependent.hdid}]`)
            .as("newDependentCard")
            .within(() => {
                // Validate the newly added dependent tab and elements are present
                cy.get("[data-testid=dependentName]").contains(
                    validDependent.name
                );
            });

        cy.log("Validate duplicate dependent cannot be added by the same user");

        cy.get("[data-testid=add-dependent-button]").click();
        cy.get("[data-testid=new-dependent-modal-form]").should(
            "exist",
            "be.visible"
        );
        cy.get("[data-testid=dependent-phn-input]")
            .clear()
            .type(validDependent.phn)
            .blur();
        cy.get("[data-testid=errorDependentAlreadyAdded]").should("be.visible");
        cy.get("[data-testid=cancel-dependent-registration-btn]").click();

        // Validate the modal is done
        cy.get("[data-testid=add-dependent-dialog]").should("not.exist");

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
