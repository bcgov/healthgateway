import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const fakeSMSNumber = "2506714848";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";

describe("User Profile", () => {
    beforeEach(() => {
        cy.configureSettings({});

        setupStandardFixtures();

        cy.intercept("GET", "**/UserProfile/IsValidPhoneNumber/*", {
            body: true,
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );
    });

    // AB#16941 - Hide login history as that was not in Sales Force implementation
    it.skip("Validate Login History Sorted Descending", () => {
        cy.get("[data-testid=lastLoginDateItem]")
            .first()
            .then(($dateItem) => {
                // 1st login date in the list
                const firstDate = new Date($dateItem.text().trim());
                cy.get("[data-testid=lastLoginDateItem]")
                    .eq(1)
                    .then(($dateItem) => {
                        // 2nd login date in the list
                        const secondDate = new Date($dateItem.text().trim());
                        expect(firstDate).to.be.gte(secondDate);
                        // 3rd login date in the list
                        cy.get("[data-testid=lastLoginDateItem]")
                            .eq(2)
                            .then(($dateItem) => {
                                const lastDate = new Date(
                                    $dateItem.text().trim()
                                );
                                expect(firstDate).to.be.gte(lastDate);
                                expect(secondDate).to.be.gte(lastDate);
                            });
                    });
            });
    });

    it("Email address", () => {
        cy.intercept("PUT", `**/UserProfile/${HDID}/email?api-version=2.0`, {
            statusCode: 200,
            body: true,
        });

        cy.log("Edit email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=email-input] input").type(
            Cypress.env("emailAddress")
        );
        cy.readFile(
            "cypress/fixtures/UserProfileService/userProfile.json",
            (err, _data) => {
                if (err) {
                    throw err;
                }
            }
        ).then((data) => {
            data.email = Cypress.env("emailAddress");
            cy.intercept("GET", `**/UserProfile/${HDID}?api-version=2.0`, {
                ...data,
            });
        });
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=emailStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=resendEmailBtn]").should("be.visible");

        cy.log("Invalid email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=editEmailSaveBtn]").should("be.disabled");
        cy.get("[data-testid=email-input]")
            .find(".v-messages__message")
            .contains("New email must be different from the previous one")
            .should("be.visible");
        cy.get("[data-testid=editEmailCancelBtn]").click();
        cy.get("[data-testid=email-input] input").should(
            "have.attr",
            "readonly"
        );
        cy.get("[data-testid=email-input] input").should(
            "have.value",
            Cypress.env("emailAddress")
        );

        cy.log("Clear/OptOut email address");
        cy.get("[data-testid=editEmailBtn]").click();
        cy.get("[data-testid=email-input] input").clear();
        cy.get("[data-testid=emailOptOutMessage]").should("be.visible");
        cy.intercept("GET", `**/UserProfile/${HDID}?api-version=2.0`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.get("[data-testid=editEmailSaveBtn]").click();
        cy.get("[data-testid=loadingSpinner]").should("not.exist");
        cy.get("[data-testid=emailStatusOptedOut]").should("be.visible");
    });

    it("Verify SMS number", () => {
        cy.intercept("PUT", `**/UserProfile/${HDID}/sms?api-version=2.0`, {
            statusCode: 200,
            body: true,
        });
        cy.intercept("GET", `**/UserProfile/${HDID}/sms/validate/*`, {
            statusCode: 200,
            body: true,
        });

        cy.log("Verify SMS number");
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.intercept("GET", `**/UserProfile/${HDID}?api-version=2.0`, {
            fixture: "UserProfileService/userProfileSMSVerified.json",
        });
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.get("[data-testid=verifySMSModalCodeInput]")
            .should("be.visible")
            .find("input")
            .should("have.focus")
            .type("123456");

        cy.log("SMS number status Verified");
        cy.get("[data-testid=smsStatusNotVerified]").should("not.exist");
        cy.get("[data-testid=verifySMSBtn]").should("not.exist");
        cy.get("[data-testid=smsStatusVerified]").should("be.visible");

        cy.log("Edit SMS number");
        cy.intercept("GET", `**/UserProfile/${HDID}?api-version=2.0`, {
            fixture: "UserProfileService/userProfile.json",
        });
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput]")
            .find(".v-messages__message")
            .contains("New SMS number must be different from the previous one")
            .should("be.visible");
        cy.get("[data-testid=smsNumberInput] input")
            .clear()
            .type(fakeSMSNumber);
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=messageModalCloseButton]").click();
        cy.get("[data-testid=smsStatusNotVerified]").should("be.visible");
        cy.get("[data-testid=verifySMSBtn]")
            .should("be.visible")
            .should("be.enabled");

        cy.log("Clear/OptOut SMS number");
        cy.get("[data-testid=editSMSBtn]").click();
        cy.get("[data-testid=smsNumberInput] input").clear();
        cy.get("[data-testid=smsOptOutMessage]").should("be.visible");
        cy.readFile(
            "cypress/fixtures/UserProfileService/userProfile.json",
            (err, _data) => {
                if (err) {
                    throw err;
                }
            }
        ).then((data) => {
            data.smsNumber = undefined;
            cy.intercept("GET", `**/UserProfile/${HDID}?api-version=2.0`, {
                ...data,
            });
        });
        cy.get("[data-testid=saveSMSEditBtn]").click();
        cy.get("[data-testid=smsStatusOptedOut]").should("be.visible");
    });
});

describe("User Profile - Validate Address", () => {
    beforeEach(() => {
        cy.configureSettings({});

        cy.intercept("GET", "**/UserProfile/IsValidPhoneNumber/*", {
            body: true,
        });
    });

    it("Verify user has combined address", () => {
        setupStandardFixtures();
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("3815 HILLSPOINT STREET");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("CHATHAM, BC, V0G8B8");

        cy.get("[data-testid=physical-address-section").should("not.exist");
    });

    it("Verify user has different addresses", () => {
        setupStandardFixtures({
            patientFixture: "PatientService/patientDifferentAddress.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("8128 WILD CREEK DRIVE");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("HOLBORN, BC, V3A3P1");

        // Physical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("1234 LEE AVENUE");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("VICTORIA, BC, V9C6P1");
    });

    it("Verify user has no address", () => {
        setupStandardFixtures({
            patientFixture: "PatientService/patientNoAddress.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Address");
        cy.get("[data-testid=no-postal-address-text]").should("be.visible");

        // Physical Address
        cy.get("[data-testid=physical-address-section").should("not.exist");
    });

    it("Verify user has only physical address", () => {
        setupStandardFixtures({
            patientFixture: "PatientService/patientOnlyPhysicalAddress.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=no-postal-address-text]").should("be.visible");

        // Phsyical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("4790 RAD CLOSE");
        cy.get("[data-testid=physical-address-div]")
            .should("be.visible")
            .contains("HYTHE , BC, V5K 2X0");
    });

    it("Verify user has only postal address", () => {
        setupStandardFixtures({
            patientFixture: "PatientService/patientOnlyPostalAddress.json",
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Postal Address
        cy.get("[data-testid=postal-address-label]")
            .should("be.visible")
            .contains("Mailing Address");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("7826 UPPER AUSTIN LODGE L");
        cy.get("[data-testid=postal-address-div]")
            .should("be.visible")
            .contains("AXBRIDGE, BC, T8C 8G7");

        // Physical Address
        cy.get("[data-testid=physical-address-label]").should("be.visible");
        cy.get("[data-testid=no-physical-address-text]").should("be.visible");
    });
});

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
            email: overrides.email ?? "nobody@healthgateway.gov.bc.ca",
            isEmailVerified: overrides.isEmailVerified ?? true,
            smsNumber: overrides.smsNumber ?? "2506715000",
            isSMSNumberVerified: overrides.isSMSNumberVerified ?? true,
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
                    emailEnabled: overrides.emailEnabled ?? true,
                    smsEnabled: overrides.smsEnabled ?? true,
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
            sms: { enabled: false, checked: true },
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
            sms: { enabled: false, checked: true },
        },
        {
            name: "Displays notification preferences when sms is verified and preferences are on but email is not verified",
            profile: {
                isEmailVerified: false,
                isSMSNumberVerified: true,
                emailEnabled: true,
                smsEnabled: true,
            },
            email: { enabled: false, checked: true },
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
            email: { enabled: false, checked: true },
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
            email: { enabled: false, checked: true },
            sms: { enabled: false, checked: true },
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
            name: "Hides verification message when only email exists and is verified (SMS opted out)",
            profile: {
                email: "nobody@healthgateway.gov.bc.ca",
                isEmailVerified: true,
                smsNumber: undefined,
                // isSMSNumberVerified irrelevant when smsNumber is undefined
                isSMSNumberVerified: true,
            },
            shouldShow: false,
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
            name: "Hides verification message when only SMS exists and is verified (email opted out)",
            profile: {
                email: undefined,
                // isEmailVerified irrelevant when email is undefined
                isEmailVerified: true,
                smsNumber: "2506715000",
                isSMSNumberVerified: true,
            },
            shouldShow: false,
        },
        {
            name: "Hides verification message when neither email nor SMS exists (both opted out)",
            profile: {
                email: undefined,
                // isEmailVerified irrelevant when email is undefined
                isEmailVerified: true,
                smsNumber: undefined,
                // isSMSNumberVerified irrelevant when smsNumber is undefined
                isSMSNumberVerified: true,
            },
            shouldShow: false,
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
    it("Displays error when updating preferences on toggle", () => {
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
});
