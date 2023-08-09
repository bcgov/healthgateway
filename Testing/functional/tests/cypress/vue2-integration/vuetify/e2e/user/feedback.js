const { AuthMethod } = require("../../../../support/constants");

describe("User Feedback", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Send feedback", () => {
        cy.get("[data-testid=menu-btn-feedback-link]").click();
        cy.get("[data-testid=feedback-comment-input]").type("Great job team!");
        cy.get("[data-testid=send-feedback-message-btn]").click();
        cy.get("[data-testid=feedback-no-need-btn]").should("be.visible");
        cy.get("[data-testid=feedback-update-my-email-btn]").should(
            "be.visible"
        );
    });
});
