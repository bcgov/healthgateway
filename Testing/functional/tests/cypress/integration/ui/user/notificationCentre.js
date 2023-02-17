const { AuthMethod } = require("../../../support/constants");
const notificationIdOne = "f57bcb39-64ca-0a17-5477-92ea7f084fbf";
const notificationIdTwo = "72c2d6c0-b370-24e6-0b5c-04e42b6511c8";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("Notification Centre", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: {
                name: "medication",
                enabled: true,
            },
            notificationCentre: {
                enabled: true,
            },
            waitingQueue: {
                enabled: false,
            },
        });

        cy.intercept("GET", `**/Notification/${HDID}`, {
            fixture: "NotificationService/notifications.json",
        });

        cy.intercept("DELETE", `**/Notification/${HDID}/*`, {
            statusCode: 200,
        });

        cy.intercept("DELETE", `**/Notification/${HDID}`, {
            statusCode: 200,
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Dismiss individual notification", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" + notificationIdOne + "-action-button]"
        ).should("be.visible");
        cy.get(
            "[data-testid=notification-" +
                notificationIdOne +
                "-dismiss-button]"
        ).should("be.visible");

        cy.get(
            "[data-testid=notification-" + notificationIdTwo + "-action-button]"
        ).should("not.exist");
        cy.get(
            "[data-testid=notification-" +
                notificationIdTwo +
                "-dismiss-button]"
        )
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdOne +
                "-dismiss-button]"
        ).should("be.visible");
        cy.get(
            "[data-testid=notification-" +
                notificationIdTwo +
                "-dismiss-button]"
        ).should("not.exist");

        cy.get("[data-testid=notification-centre-close-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=notification-centre-close-button]").should(
            "not.be.visible"
        );
    });

    it("Dismiss all notifications", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdOne +
                "-dismiss-button]"
        ).should("be.visible");
        cy.get(
            "[data-testid=notification-" +
                notificationIdTwo +
                "-dismiss-button]"
        ).should("be.visible");

        cy.get("[data-testid=notification-centre-dismiss-all-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdOne +
                "-dismiss-button]"
        ).should("not.exist");
        cy.get(
            "[data-testid=notification-" +
                notificationIdTwo +
                "-dismiss-button]"
        ).should("not.exist");
        cy.get("[data-testid=notification-centre-dismiss-all-button]").should(
            "not.exist"
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
