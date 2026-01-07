import { AuthMethod } from "../../../support/constants";

describe("Notification Centre", () => {
    beforeEach(() => {
        cy.intercept("DELETE", "**/Notification/*").as("deleteNotification");

        cy.configureSettings({
            notificationCentre: {
                enabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );

        // Validate home page has displayed before clicking on notification.
        cy.get("[data-testid=health-records-card]").should("be.visible");
    });

    // AB#16927 Disable notifications while aligning Classic with Salesforce version
    it.skip("Get notifications", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible", "be.enabled")
            .click();

        cy.get("[data-testid=notification-centre-dismiss-all-button]")
            .should("be.visible", "be.enabled")
            .scrollIntoView()
            .click();

        cy.get("[data-testid=notifications-div]").should("not.exist");

        cy.get("[data-testid=notification-centre-close-button]")
            .should("be.visible", "be.enabled")
            .click();

        cy.wait("@deleteNotification");
        cy.get("[data-testid=notification-centre-close-button]").should(
            "not.be.visible"
        );
    });
});
