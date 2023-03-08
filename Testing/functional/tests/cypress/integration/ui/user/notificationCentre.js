const { AuthMethod } = require("../../../support/constants");
const notificationIdOne = "f57bcb39-64ca-0a17-5477-92ea7f084fbf";
const notificationIdTwo = "72c2d6c0-b370-24e6-0b5c-04e42b6511c8";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("Notification Centre", () => {
    beforeEach(() => {
        cy.configureSettings({
            notificationCentre: {
                enabled: true,
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

describe("Notification Badge", () => {
    beforeEach(() => {
        cy.configureSettings({
            notificationCentre: {
                enabled: true,
            },
        });

        cy.intercept("GET", `**/UserProfile/${HDID}`, {
            fixture: "UserProfileService/userProfile.json",
        });

        // The scheduledDateTimeUtc must be after user profile's last login in lastLoginDateTimes, which is the second entry
        // not the first entry. The first entry is the current login.
        cy.intercept("GET", `**/Notification/${HDID}`, {
            fixture: "NotificationService/notifications.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Verify notification badge", () => {
        cy.get("[data-testid=notification-centre-button]")
            .get("span")
            .should("have.class", "b-avatar-badge badge-danger")
            .contains("2");

        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=notification-centre-close-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=notification-centre-button]").should(
            "not.have.class",
            "b-avatar-badge badge-danger"
        );

        cy.reload();

        cy.get("[data-testid=notification-centre-button]")
            .get("span")
            .should("have.class", "b-avatar-badge badge-danger")
            .contains("2");
    });
});
