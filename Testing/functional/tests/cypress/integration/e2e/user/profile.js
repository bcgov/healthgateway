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
        learnMore: (id) =>
            `[data-testid=profile-notification-preferences-${id}-learn-more]`,
        modal: "[data-testid=information-modal]",
        modalParagraph: (index) =>
            `[data-testid=information-modal-paragraph-${index}]`,
        modalClose: "[data-testid=message-modal-close-button]",
        modalOk: "[data-testid=generic-message-ok-btn]",
    };

    function assertSwitch(id, channel, { enabled, checked }) {
        const selector = channel === "email" ? sel.email(id) : sel.sms(id);

        cy.get(selector)
            .scrollIntoView()
            .should("be.visible")
            .find('input[type="checkbox"]')
            .should(enabled ? "be.enabled" : "be.disabled")
            .and(checked ? "be.checked" : "not.be.checked");
    }

    function toggleSwitch(id, channel) {
        const selector = channel === "email" ? sel.email(id) : sel.sms(id);

        cy.get(selector)
            .scrollIntoView()
            .should("be.visible")
            .find(".v-switch__track")
            .click({ force: true });
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
        // UI profileNotificatonSettngs is using keycloak.username
        cy.login(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        cy.get(sel.header).should("be.visible");
        cy.get(sel.label(notificationId)).should("be.visible");

        cy.log("Assert email switch before toggle");
        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        cy.log("Assert sms switch before toggle");
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });

        cy.intercept("PUT", "**/UserProfile/*/notificationsettings*").as(
            "updateNotificationSettings"
        );

        cy.log("Toggle email switch");
        toggleSwitch(notificationId, "email");

        cy.wait("@updateNotificationSettings")
            .its("response.statusCode")
            .should("eq", 200);

        cy.log("Assert email switch after toggle");
        assertSwitch(notificationId, "email", { enabled: true, checked: true });
        cy.log("Assert sms switch after toggle");
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });

        cy.get(sel.learnMore(notificationId))
            .scrollIntoView()
            .should("be.visible")
            .and("contain.text", "LEARN MORE")
            .click();

        cy.get(sel.modal).should("be.visible");
        cy.contains(
            "[data-testid=information-modal]",
            "BC Cancer Screening Program Letters"
        ).should("be.visible");

        cy.get(sel.modalParagraph(0))
            .should("be.visible")
            .and(
                "contain.text",
                "Cancer screening program letters are available on Health Gateway."
            )
            .and(
                "contain.text",
                "Health Gateway can email you when new letters are added"
            );

        cy.get(sel.modalParagraph(1))
            .should("be.visible")
            .and(
                "contain.text",
                "Signing up for email notifications will not change the paper letters that will be mailed to you."
            );

        cy.get(sel.modalParagraph(2))
            .should("be.visible")
            .and(
                "contain.text",
                "For more information about BC Cancer Program Letters visit"
            );

        cy.get("[data-testid=information-modal-link-2-1]")
            .should("be.visible")
            .and("contain.text", "screeningbc.ca/contact")
            .and(
                "have.attr",
                "href",
                "https://www.bccancer.bc.ca/screening/contact"
            );

        cy.get(sel.modalOk).should("be.visible").click();
        cy.get(sel.modal).should("not.exist");
    });

    it("Deleting email disables only email notification preferences", () => {
        cy.login(
            Cypress.env("keycloak.notfound.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        cy.log("Assert email switch before delete");
        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        cy.log("Assert sms switch before delete");
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });

        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=email-input] input").clear();
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        cy.log("Assert email switch after delete");
        assertSwitch(notificationId, "email", {
            enabled: false,
            checked: false,
        });
        cy.log("Assert sms switch after delete");
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });
    });

    it("Deleting SMS number disables only SMS notification preferences", () => {
        cy.login(
            Cypress.env("keycloak.protected.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        cy.log("Assert email switch before delete");
        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        cy.log("Assert sms switch before delete");
        assertSwitch(notificationId, "sms", { enabled: true, checked: false });

        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput] input").clear();
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("be.visible");

        cy.log("Assert email switch after dekete");
        assertSwitch(notificationId, "email", {
            enabled: true,
            checked: false,
        });
        cy.log("Assert sms switch after delete");
        assertSwitch(notificationId, "sms", { enabled: false, checked: false });
    });
});
