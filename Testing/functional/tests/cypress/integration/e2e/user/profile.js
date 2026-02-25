const { AuthMethod } = require("../../../support/constants");

describe("User Profile", () => {
    beforeEach(() => {
        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    it("Verify PHN and address label are visible", () => {
        cy.get("[data-testid=PHN]").should("be.visible").contains("9735353315");

        cy.get("[data-testid=postal-address-label]").should("be.visible");
    });
});

describe("User Profile Notification Settings", () => {
    const notificationId = "bc-cancer-screening";

    const sel = {
        header: "[data-testid=profile-notification-preferences-header]",
        label: (id) =>
            `[data-testid=profile-notification-preferences-${id}-label-value]`,
        email: (id) =>
            `[data-testid=profile-notification-preferences-${id}-email-value]`,
        sms: (id) =>
            `[data-testid=profile-notification-preferences-${id}-sms-value]`,
        tooltip: (id) => `[data-testid=info-tooltip-${id}]`,
        tooltipIcon: (id) => `[data-testid=info-tooltip-${id}-icon]`,
        tooltipLink: (id) => `[data-testid=info-tooltip-${id}-link]`,
    };

    function assertSwitch(id, channel, { enabled, checked }) {
        const selector = channel === "email" ? sel.email(id) : sel.sms(id);

        cy.get(selector)
            .should("be.visible")
            .find('input[type="checkbox"]')
            .should(enabled ? "be.enabled" : "be.disabled")
            .and(checked ? "be.checked" : "not.be.checked");
    }

    function toggleSwitch(id, channel) {
        const selector = channel === "email" ? sel.email(id) : sel.sms(id);

        // Click the Vuetify switch track directly because the data-testid is applied
        // to the v-input wrapper, not the actual interactive element.
        cy.get(selector).find(".v-switch__track").click({ force: true });
    }

    beforeEach(() => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: true,
                    type: [
                        {
                            name: "bcCancerScreening",
                            enabled: true,
                            preferences: {
                                email: true,
                                sms: true,
                            },
                        },
                    ],
                },
            },
        });
    });

    it("Displays profile notification section and updates preferences on toggle", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        cy.get(sel.header).should("be.visible");
        cy.get(sel.label(notificationId)).should("be.visible");

        cy.get(sel.tooltipIcon(notificationId))
            .filter(":visible")
            .first()
            .click();

        cy.get(sel.tooltip(notificationId)).should("be.visible");

        cy.get(sel.tooltipLink(notificationId))
            .should("be.visible")
            .and("have.attr", "href")
            .and("include", "/not-found");

        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });

        cy.intercept("PUT", "**/UserProfile/*/notificationsettings*").as(
            "updateNotificationSettings"
        );

        toggleSwitch(notificationId, "email");

        cy.wait("@updateNotificationSettings")
            .its("response.statusCode")
            .should("eq", 200);

        assertSwitch(notificationId, "email", { enabled: true, checked: true });
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });
    });

    it("Deleting email disables only email notification preferences", () => {
        cy.login(
            Cypress.env("keycloak.notfound.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });

        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=email-input] input").clear();
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        assertSwitch(notificationId, "email", {
            enabled: false,
            checked: false,
        });
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });
    });

    it("Deleting SMS number disables only SMS notification preferences", () => {
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });

        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput] input").clear();
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });
    });
});
