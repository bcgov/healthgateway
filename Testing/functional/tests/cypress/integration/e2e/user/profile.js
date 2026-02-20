const { AuthMethod } = require("../../../support/constants");

describe("User Profile", () => {
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

    it("Displays profile notification section and updates preferences on toggle", () => {
        cy.get("[data-testid=profile-notification-preferences-header]").should(
            "be.visible"
        );

        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");

        cy.get("[data-testid=info-tooltip-bc-cancer-screening-icon]")
            .filter(":visible")
            .first()
            .click();

        cy.get("[data-testid=info-tooltip-bc-cancer-screening]").should(
            "be.visible"
        );

        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("be.visible", "be.enabled", "be.checked");

        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("be.visible", "not.be.enabled");

        cy.intercept("PUT", "**/UserProfile/*/notificationsettings*").as(
            "updateNotificationSettings"
        );

        // Click the Vuetify switch track directly because the data-testid is applied
        // to the v-input wrapper, not the actual interactive element. Clicking the
        // wrapper does not reliably trigger the @update:model-value event, so we
        // target the internal .v-switch__track to ensure the toggle fires.
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        )
            .find(".v-switch__track")
            .click({ force: true });

        cy.wait("@updateNotificationSettings")
            .its("response.statusCode")
            .should("eq", 200);
    });
});
