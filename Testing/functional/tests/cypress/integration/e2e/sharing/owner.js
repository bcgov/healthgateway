const { AuthMethod } = require("../../../support/constants");

const invalidEmail = "invalidEmail";
const validDelegateInvite = {
    email: "test@test.com",
    nickname: "test",
    dataSources: [
        { dataSource: "BcCancerScreening", name: "BC Cancer Screening" },
    ],
    expiryDateRange: "3 months",
};

function testContactStep(testInvite) {
    cy.get("[data-testid=invitation-contact-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=invitation-dialog-confirm-button]")
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
    cy.get("[data-testid=invitation-dialog-confirm-button]").click();
}

function testDataSourceStep(testInvite) {
    cy.get("[data-testid=invitation-data-sources-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=invitation-dialog-confirm-button]")
                .click();
            cy.get("[data-testid=invitation-datasources]").should(
                "have.class",
                "v-input--error"
            );

            // Enter values
            for (var i = 0; i < testInvite.dataSources.length; i++) {
                cy.vSelect(
                    "[data-testid=invitation-datasources]",
                    testInvite.dataSources[i].name
                );
            }
        });
    // Move to next step
    cy.get("[data-testid=invitation-dialog-confirm-button]").click();
}

function testExpiryDateStep(testInvite) {
    cy.get("[data-testid=invitation-expiry-step]")
        .should("exist")
        .within(() => {
            // Test empty validation
            cy.document()
                .find("[data-testid=invitation-dialog-confirm-button]")
                .click();
            cy.get("[data-testid=invitation-expiry-range]").should(
                "have.class",
                "v-input--error"
            );

            // Enter values
            cy.vSelect(
                "[data-testid=invitation-expiry-range]",
                testInvite.expiryDateRange
            );
        });
    // Move to next step
    cy.get("[data-testid=invitation-dialog-confirm-button]").click();
}

function testReviewStep(testInvite) {
    cy.get("[data-testid=invitation-review-step]")
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
            for (var i = 0; i < testInvite.dataSources.length; i++) {
                const formattedDataSource = testInvite.dataSources[i].dataSource
                    .toLowerCase()
                    .trim();
                cy.get(
                    `[data-testid=review-datasource-${formattedDataSource}]`
                ).should("have.text", testInvite.dataSources[i].name);
            }
        });
    // Create the invitation
    cy.get("[data-testid=invitation-dialog-confirm-button]").click();
}

function testSharingCodeStep() {
    cy.get("[data-testid=invitation-sharing-code-step]")
        .should("exist")
        .within(() => {
            // Validate sharing code is displayed
            cy.get("[data-testid=sharing-code]").should("exist");
        });
    cy.get("[data-testid=invitation-dialog-confirm-button]").click();
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
        cy.get("[data-testid=start-new-invitation]").click();
        cy.get("[data-testid=delegation-invitate-dialog]").should("exist");
        testContactStep(validDelegateInvite);
        testDataSourceStep(validDelegateInvite);
        testExpiryDateStep(validDelegateInvite);
        testReviewStep(validDelegateInvite);
        testSharingCodeStep();
        cy.get("[data-testid=delegation-invitate-dialog]").should("not.exist");
    });
});
