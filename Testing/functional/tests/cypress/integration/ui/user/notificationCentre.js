const { AuthMethod } = require("../../../support/constants");
const notificationIdOne = "f57bcb39-64ca-0a17-5477-92ea7f084fbf";
const notificationIdTwo = "72c2d6c0-b370-24e6-0b5c-04e42b6511c8";
const notificationIdImms = "9eb24f30-ab74-4cdc-3280-08db134f5424";
const notificationIdBctOdr = "1f2d31d1-d540-4034-327f-08db134f5424";
const notificationIdOtherInternal = "37592c14-4cc1-4bd0-0b13-08db340e7e77";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const timelinePath = "/timeline";
const servicesPath = "/services";
const reportsPath = "/reports";
const immunizationTitle = "Immunizations";

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
        cy.get("[data-testid=notification-centre-button]").click();
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
            .should("have.class", "v-badge__badge")
            .contains("3");

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
            "v-badge__badge"
        );

        cy.reload();

        cy.get("[data-testid=notification-centre-button]")
            .get("span")
            .should("have.class", "v-badge__badge")
            .contains("3");
    });
});

describe("Categorized web alerts", () => {
    beforeEach(() => {
        cy.configureSettings({
            notificationCentre: {
                enabled: true,
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
            services: {
                enabled: true,
            },
        });

        cy.intercept("GET", `**/Notification/${HDID}`, {
            fixture: "NotificationService/notifications.json",
        });

        cy.intercept("GET", `**/Immunization?hdid*`, {
            fixture: "ImmunizationService/immunization.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );
    });

    it("Web alert category to pre-filtered timeline", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdImms +
                "-action-button]"
        )
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.location("pathname").should("eq", timelinePath);
        cy.contains("[data-testid=filter-label]", immunizationTitle);
    });

    it("Web alert category to services", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdBctOdr +
                "-action-button]"
        )
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.location("pathname").should("eq", servicesPath);
    });

    it("Web alert category to other internal link", () => {
        cy.get("[data-testid=notification-centre-button]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get(
            "[data-testid=notification-" +
                notificationIdOtherInternal +
                "-action-button]"
        )
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.location("pathname").should("eq", reportsPath);
    });
});
