const pcrTestUrl = "/pcrtest";

const feedbackTestKitCodeIsRequiredSelector =
    "[data-testid=feedback-test-kit-code-is-required]";
const feedbackFirstNameIsRequiredSelector =
    "[data-testid=feedback-first-name-is-required]";
const feedbackLastNameIsRequiredSelector =
    "[data-testid=feedback-last-name-is-required]";
const feedbackPhnIsRequiredSelector = "[data-testid=feedback-phn-is-required]";
const feedbackDoBIsRequiredSelector = "[data-testid=feedback-dob-is-required]";

const feedbackTestKitCodeValidSelector =
    "[data-testid=test-kit-code-is-invalid]";
const feedbackDoBValidSelector = "[data-testid=feedback-dob-is-valid]";
const feedbackPhoneNumberValidSelector = "[data-testid=feedback-dob-is-valid]";

function selectorShouldBeVisible(selector) {
    cy.get(selector).should("be.visible");
}

function clickSubmitButton() {
    cy.get("[data-testid=btn-submit]")
        .should("be.enabled", "be.visible")
        .click();
}

function inputFieldsShouldBeVisible() {
    selectorShouldBeVisible("[data-testid=test-kit-code-input]");
    selectorShouldBeVisible("[data-testid=first-name-input]");
    selectorShouldBeVisible("[data-testid=last-name-input]");
    selectorShouldBeVisible("[data-testid=phn-input]");
    selectorShouldBeVisible("[data-testid=pcr-no-phn-info-button]");
    selectorShouldBeVisible("[data-testid=formSelectYear]");
    selectorShouldBeVisible("[data-testid=formSelectMonth]");
    selectorShouldBeVisible("[data-testid=formSelectDay]");
    selectorShouldBeVisible("[data-testid=contact-phone-number-input]");
    selectorShouldBeVisible("[data-testid=test-taken-minutes-ago]");
    selectorShouldBeVisible("[data-testid=btn-cancel]");
    selectorShouldBeVisible("[data-testid=btn-submit]");
    selectorShouldBeVisible("[data-testid=pcr-privacy-statement]");
}

describe("Public PcrTest Registration Form", () => {
    beforeEach(() => {
        cy.enableModules();
        cy.visit(pcrTestUrl);
    });

    it("Register options buttons are available", () => {
        selectorShouldBeVisible("[data-testid=btn-login]");
        selectorShouldBeVisible("[data-testid=btn-manual]");
    });

    it("Inputs in the form are visible", () => {
        cy.get("[data-testid=btn-manual]").should("be.visible").click();
        inputFieldsShouldBeVisible();
    });

    it("Required Validations are visible", () => {
        clickSubmitButton();
        cy.get(feedbackTestKitCodeIsRequiredSelector).should("be.visible");
        cy.get().should("be.visible");
        cy.get().should("be.visible");
    });
});
