const { AuthMethod } = require("../../../support/constants");

const invalidEmail = "invalidEmail";
const validDelegateInvite = {
    email: "test@test.com",
    nickname: "test",
    recordTypes: [
        { entryType: "bcCancerScreening", name: "BC Cancer Screening" },
    ],
    expiryDateRange: "3 months",
};

function testContactStep(testInvite) {
    cy.get("[data-testid=delegation-contact-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=delegation-dialog-confirm-button]")
                .click();

            cy.get("[data-testid=delegate-email-input]").should(
                "have.class",
                "v-input--error"
            );
            cy.get("[data-testid=delegate-nickname-input]").should(
                "have.class",
                "v-input--error"
            );

            // Enter values
            cy.get("[data-testid=delegate-email-input] input").type(
                testInvite.email
            );
            cy.get("[data-testid=delegate-nickname-input] input").type(
                testInvite.nickname
            );
        });
    // Move to next step
    cy.get("[data-testid=delegation-dialog-confirm-button]").click();
}

function testRecordTypesStep(testInvite) {
    cy.get("[data-testid=delegation-record-types-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=delegation-dialog-confirm-button]")
                .click();
            cy.get("[data-testid=delegation-record-types-select]").should(
                "have.class",
                "v-input--error"
            );

            // Enter values
            for (var i = 0; i < testInvite.recordTypes.length; i++) {
                cy.vSelect(
                    "[data-testid=delegation-record-types-select]",
                    testInvite.recordTypes[i].name
                );
            }
        });
    // Move to next step
    cy.get("[data-testid=delegation-dialog-confirm-button]").click();
}

function testExpiryDateStep(testInvite) {
    cy.get("[data-testid=delegation-expiry-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=delegation-dialog-confirm-button]")
                .click();
            cy.get("[data-testid=delegation-expiry-select]").should(
                "have.class",
                "v-input--error"
            );

            // Enter values
            cy.vSelect(
                "[data-testid=delegation-expiry-select]",
                testInvite.expiryDateRange
            );
        });
    // Move to next step
    cy.get("[data-testid=delegation-dialog-confirm-button]").click();
}

function testReviewStep(testInvite) {
    cy.get("[data-testid=delegation-review-step]")
        .should("exist")
        .within(() => {
            // Validate captured data is reflected
            cy.get("span[data-testid=review-email]").should(
                "have.text",
                testInvite.email
            );
            cy.get("span[data-testid=review-nickname]").should(
                "have.text",
                testInvite.nickname
            );
            for (var i = 0; i < testInvite.recordTypes.length; i++) {
                const formattedEntryType = testInvite.recordTypes[i].entryType
                    .toLowerCase()
                    .trim();
                cy.get(
                    `[data-testid=review-record-type-${formattedEntryType}]`
                ).should("have.text", testInvite.recordTypes[i].name);
            }
        });
    // Create the invitation
    cy.get("[data-testid=delegation-dialog-confirm-button]").click();
}

function testSharingCodeStep() {
    cy.get("[data-testid=delegation-sharing-code-step]")
        .should("exist")
        .within(() => {
            // Validate sharing code is displayed
            cy.get("[data-testid=sharing-code]").should("exist");
        });
    cy.get("[data-testid=delegation-dialog-confirm-button]").click();
}

describe("Sharing by the owner", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "bcCancerScreening",
                    enabled: true,
                },
                {
                    name: "clinicalDocument",
                    enabled: true,
                },
            ],
            sharing: { enabled: true },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/sharing"
        );
    });

    it("Should be able to invite a delegate to share their records with", () => {
        cy.get("[data-testid=empty-sharing-page]").should("exist");
        cy.get("[data-testid=start-new-delegation]").click();
        cy.get("[data-testid=delegation-wizard-dialog]").should("exist");
        testContactStep(validDelegateInvite);
        testRecordTypesStep(validDelegateInvite);
        testExpiryDateStep(validDelegateInvite);
        testReviewStep(validDelegateInvite);
        testSharingCodeStep();
        cy.get("[data-testid=delegation-wizard-dialog]").should("not.exist");
    });
});
