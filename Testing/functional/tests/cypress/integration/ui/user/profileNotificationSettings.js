import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

describe("User Profile Notification Settings", () => {
    const notificationId = "bc-cancer-screening";

    const notificationSel = {
        header: "[data-testid=profile-notification-preferences-label]",
        label: `[data-testid=profile-notification-preferences-${notificationId}-label-value]`,
        email: `[data-testid=profile-notification-preferences-${notificationId}-email-value]`,
        sms: `[data-testid=profile-notification-preferences-${notificationId}-sms-value]`,
        empty: "[data-testid=profile-notification-preferences-empty]",
    };

    function buildUserProfileFixture(overrides = {}) {
        return {
            hdId: "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A",
            acceptedTermsOfService: true,

            email: Object.prototype.hasOwnProperty.call(overrides, "email")
                ? overrides.email
                : "nobody@healthgateway.gov.bc.ca",

            isEmailVerified: Object.prototype.hasOwnProperty.call(
                overrides,
                "isEmailVerified"
            )
                ? overrides.isEmailVerified
                : true,

            smsNumber: Object.prototype.hasOwnProperty.call(
                overrides,
                "smsNumber"
            )
                ? overrides.smsNumber
                : "2506715000",

            isSMSNumberVerified: Object.prototype.hasOwnProperty.call(
                overrides,
                "isSMSNumberVerified"
            )
                ? overrides.isSMSNumberVerified
                : true,

            hasTermsOfServiceUpdated: false,
            lastLoginDateTime: "2021-11-23T05:55:14Z",
            lastLoginDateTimes: [
                "2021-11-23T05:55:14Z",
                "2021-11-22T05:49:47Z",
                "2021-11-21T05:49:16Z",
            ],
            closedDateTime: null,
            preferences: {},
            hasTourUpdated: true,
            blockedDataSources: [],
            notificationSettings: [
                {
                    type: "BcCancerScreening",
                    emailEnabled: Object.prototype.hasOwnProperty.call(
                        overrides,
                        "emailEnabled"
                    )
                        ? overrides.emailEnabled
                        : true,
                    smsEnabled: Object.prototype.hasOwnProperty.call(
                        overrides,
                        "smsEnabled"
                    )
                        ? overrides.smsEnabled
                        : true,
                },
            ],
        };
    }

    function login() {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    }

    function assertSwitch(selector, { enabled, checked }) {
        cy.get(selector)
            .should("be.visible")
            .find('input[type="checkbox"]')
            .should(enabled ? "be.enabled" : "be.disabled")
            .and(checked ? "be.checked" : "not.be.checked");
    }

    function toggleSwitch(channel) {
        const selector =
            channel === "email" ? notificationSel.email : notificationSel.sms;

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
                            preferences: { email: true, sms: true },
                        },
                    ],
                },
            },
        });
    });

    // -------------------------------------------------------
    // Feature Toggle Tests
    // -------------------------------------------------------
    it("Featuretoggle - profile notification section is not visible", () => {
        cy.configureSettings({
            profile: { notifications: { enabled: false } },
        });

        setupStandardFixtures();
        login();

        cy.get(notificationSel.header).should("not.exist");
    });

    it("Featuretoggle - profile notification BC Cancer Screening type is not visible", () => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: true,
                    type: [{ name: "bcCancerScreening", enabled: false }],
                },
            },
        });

        setupStandardFixtures();
        login();

        cy.get(notificationSel.header).should("be.visible");
        cy.get(notificationSel.empty).should("be.visible");
    });

    it("Featuretoggle - profile notification BC Cancer Screening Sms preference is visible but Email preference is not", () => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: true,
                    type: [
                        {
                            name: "bcCancerScreening",
                            enabled: true,
                            preferences: { email: false, sms: true },
                        },
                    ],
                },
            },
        });

        setupStandardFixtures();
        login();

        cy.get(notificationSel.label).should("be.visible");
        cy.get(notificationSel.email).should("not.exist");
        cy.get(notificationSel.sms).should("be.visible");
    });

    it("Featuretoggle - profile notification BC Cancer Screening Email preference is visible but Sms preference is not", () => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: true,
                    type: [
                        {
                            name: "bcCancerScreening",
                            enabled: true,
                            preferences: { email: true, sms: false },
                        },
                    ],
                },
            },
        });

        setupStandardFixtures();
        login();

        cy.get(notificationSel.label).should("be.visible");
        cy.get(notificationSel.email).should("be.visible");
        cy.get(notificationSel.sms).should("not.exist");
    });

    // -------------------------------------------------------
    // Verification matrix:
    // Covers all combinations of email/SMS verification state
    // and persisted notification preferences (on/off).
    // Ensures UI correctly enables/disables and checks/unchecks switches
    // based on verification + saved settings.
    // -------------------------------------------------------
    const notificationPreferenceScenarios = [
        {
            name: "Displays notification preferences when email and SMS are verified and preferences are on",
            profile: {
                isEmailVerified: true,
                isSMSNumberVerified: true,
                emailEnabled: true,
                smsEnabled: true,
            },
            email: { enabled: true, checked: true },
            sms: { enabled: true, checked: true },
        },
        {
            name: "Displays notification preferences when email and SMS are verified but preferences are off",
            profile: {
                isEmailVerified: true,
                isSMSNumberVerified: true,
                emailEnabled: false,
                smsEnabled: false,
            },
            email: { enabled: true, checked: false },
            sms: { enabled: true, checked: false },
        },
        {
            name: "Displays notification preferences when email is verified and preferences are on but sms is not verified",
            profile: {
                isEmailVerified: true,
                isSMSNumberVerified: false,
                emailEnabled: true,
                smsEnabled: true,
            },
            email: { enabled: true, checked: true },
            sms: { enabled: false, checked: false }, // was true
        },
        {
            name: "Displays notification preferences when email is verified and preferences are off but sms is not verified",
            profile: {
                isEmailVerified: true,
                isSMSNumberVerified: false,
                emailEnabled: false,
                smsEnabled: true,
            },
            email: { enabled: true, checked: false },
            sms: { enabled: false, checked: false }, // was true
        },
        {
            name: "Displays notification preferences when sms is verified and preferences are on but email is not verified",
            profile: {
                isEmailVerified: false,
                isSMSNumberVerified: true,
                emailEnabled: true,
                smsEnabled: true,
            },
            email: { enabled: false, checked: false }, // was true
            sms: { enabled: true, checked: true },
        },
        {
            name: "Displays notification preferences when sms is verified and preferences are off but email is not verified",
            profile: {
                isEmailVerified: false,
                isSMSNumberVerified: true,
                emailEnabled: true,
                smsEnabled: false,
            },
            email: { enabled: false, checked: false }, // was true
            sms: { enabled: true, checked: false },
        },
        {
            name: "Disables notification preferences when email and SMS are not verified",
            profile: {
                isEmailVerified: false,
                isSMSNumberVerified: false,
                emailEnabled: true,
                smsEnabled: true,
            },
            email: { enabled: false, checked: false }, // was true
            sms: { enabled: false, checked: false }, // was true
        },
    ];

    notificationPreferenceScenarios.forEach((scenario) => {
        it(scenario.name, () => {
            setupStandardFixtures({
                userProfileBody: buildUserProfileFixture(scenario.profile),
            });

            login();

            cy.get(notificationSel.label).should("be.visible");

            assertSwitch(notificationSel.email, scenario.email);
            assertSwitch(notificationSel.sms, scenario.sms);
        });
    });

    // -------------------------------------------------------
    // Verification message visibility matrix:
    // Shows ONLY when at least one provided channel is unverified.
    // (Provided = email exists and/or smsNumber exists.)
    // -------------------------------------------------------
    const verificationMessageSel =
        "[data-testid=profile-notification-preferences-verification-message]";

    const verificationMessageScenarios = [
        {
            name: "Hides verification message when email and SMS are verified",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: true,
                smsNumber: "2506715000",
                isSMSNumberVerified: true,
            },
            shouldShow: false,
        },
        {
            name: "Shows verification message when email is unverified but SMS is verified",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: false,
                smsNumber: "2506715000",
                isSMSNumberVerified: true,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when SMS is unverified but email is verified",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: true,
                smsNumber: "2506715000",
                isSMSNumberVerified: false,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when email and SMS are unverified",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: false,
                smsNumber: "2506715000",
                isSMSNumberVerified: false,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when only email exists and is unverified (SMS opted out)",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: false,
                smsNumber: undefined,
                // isSMSNumberVerified irrelevant when smsNumber is undefined
                isSMSNumberVerified: true,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when only email exists and is verified (SMS opted out)",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: true,
                smsNumber: undefined,
                // isSMSNumberVerified irrelevant when smsNumber is undefined
                isSMSNumberVerified: true,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when only SMS exists and is unverified (email opted out)",
            profile: {
                email: undefined,
                // isEmailVerified irrelevant when email is undefined
                isEmailVerified: true,
                smsNumber: "2506715000",
                isSMSNumberVerified: false,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when only SMS exists and is verified (email opted out)",
            profile: {
                email: undefined,
                // isEmailVerified irrelevant when email is undefined
                isEmailVerified: true,
                smsNumber: "2506715000",
                isSMSNumberVerified: true,
            },
            shouldShow: true,
        },
        {
            name: "Shows verification message when neither email nor SMS exists (both opted out)",
            profile: {
                email: undefined,
                // isEmailVerified irrelevant when email is undefined
                isEmailVerified: true,
                smsNumber: undefined,
                // isSMSNumberVerified irrelevant when smsNumber is undefined
                isSMSNumberVerified: true,
            },
            shouldShow: true,
        },
    ];

    verificationMessageScenarios.forEach((scenario) => {
        it(scenario.name, () => {
            setupStandardFixtures({
                userProfileBody: buildUserProfileFixture(scenario.profile),
            });

            login();

            // sanity: section is present
            cy.get(notificationSel.header).should("be.visible");

            if (scenario.shouldShow) {
                cy.get(verificationMessageSel).should("be.visible");
            } else {
                cy.get(verificationMessageSel).should("not.exist");
            }
        });
    });

    // -------------------------------------------------------
    // Show error component scenario
    // -------------------------------------------------------
    it("Displays error when updating email preferences on toggle", () => {
        setupStandardFixtures({
            userProfileBody: buildUserProfileFixture({
                isEmailVerified: true,
                isSMSNumberVerified: true,
                emailEnabled: false,
                smsEnabled: false,
            }),
        });

        login();

        cy.get(notificationSel.label).should("be.visible");
        cy.get(notificationSel.email).should("be.visible");

        cy.intercept("PUT", "**/UserProfile/*/notificationsettings*", {
            statusCode: 500,
        }).as("updateNotificationSettingsFail");

        toggleSwitch("email");

        cy.wait("@updateNotificationSettingsFail")
            .its("response.statusCode")
            .should("eq", 500);

        cy.get(
            "[data-testid=profile-notification-preferences-save-error]"
        ).should("be.visible");

        cy.get(notificationSel.email)
            .find('input[type="checkbox"]')
            .should("not.be.checked");
    });

    // -------------------------------------------------------
    // Show 429 Too Many Requests scenario
    // -------------------------------------------------------
    it("Displays too many requests banner when updating email preferences returns 429", () => {
        setupStandardFixtures({
            userProfileBody: buildUserProfileFixture({
                isEmailVerified: true,
                isSMSNumberVerified: true,
                emailEnabled: false,
                smsEnabled: false,
            }),
        });

        login();

        cy.get(notificationSel.label).should("be.visible");
        cy.get(notificationSel.email).should("be.visible");

        cy.intercept("PUT", "**/UserProfile/*/notificationsettings*", {
            statusCode: 429,
        }).as("updateNotificationSettings429");

        toggleSwitch("email");

        cy.wait("@updateNotificationSettings429")
            .its("response.statusCode")
            .should("eq", 429);

        // Banner (TooManyRequestsComponent) should show
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");

        // Toggle should rollback to previous state (still unchecked)
        cy.get(notificationSel.email)
            .find('input[type="checkbox"]')
            .should("not.be.checked");
    });
});
