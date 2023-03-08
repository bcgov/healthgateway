const { AuthMethod } = require("../../../support/constants");
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("Notification Centre", () => {
    beforeEach(() => {
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
    });

    it("Get notifications", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=notification-centre-dismiss-all-button]").should(
            "be.visible"
        );

        cy.get("[data-testid=notification-centre-close-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=notification-centre-close-button]").should(
            "not.be.visible"
        );
    });
});
