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
    it("Featuretoggle - Verify profile notification section is not visible", () => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: false,
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

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Section hidden => nothing in this feature should render
        cy.get("[data-testid=profile-notification-preferences-label]").should(
            "not.exist"
        );
    });

    it("Featuretoggle - Verify profile notification BC Cancer Screening is not visible", () => {
        cy.configureSettings({
            profile: {
                notifications: {
                    enabled: true,
                    type: [
                        {
                            name: "bcCancerScreening",
                            enabled: false,
                            preferences: { email: true, sms: true },
                        },
                    ],
                },
            },
        });

        setupStandardFixtures();

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Section visible but no enabled types => empty message
        cy.get("[data-testid=profile-notification-preferences-label]").should(
            "be.visible"
        );
        cy.get("[data-testid=profile-notification-preferences-empty]").should(
            "be.visible"
        );
    });

    it("Featuretoggle - Verify profile notification BC Cancer Screening Email Preference is not visible", () => {
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

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Row exists, but email column/switch should not render
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("not.exist");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("be.visible", "be.enabled");
    });

    it("Featuretoggle - Verify profile notification BC Cancer Screening Sms Preference is not visible", () => {
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

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Row exists, but sms column/switch should not render
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("be.visible", "be.enabled");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("not.exist");
    });

    it("Displays notification preferences when email and SMS are verified and preferences are on", () => {
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

        setupStandardFixtures({
            userProfileFixture:
                "UserProfileService/userProfileNotificationSettingVerified.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Row exists, both email/sms column/switch should render and checked
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("be.visible", "be.enabled", "be.checked");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("be.visible", "be.enabled", "be.checked");
    });

    it("Displays notification preferences when email and SMS are verified but preferences are off", () => {
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

        setupStandardFixtures({
            userProfileFixture:
                "UserProfileService/userProfileNotificationSettingPreferencesOff.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Row exists, both email/sms column/switch should render and not be checked
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("be.visible", "be.enabled", "not.be.checked");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("be.visible", "be.enabled", "not.be.checked");
    });

    it("Disables notification preferences when email and SMS are not verified", () => {
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

        setupStandardFixtures({
            userProfileFixture:
                "UserProfileService/userProfileNotificationSettingUnverified.json",
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/profile"
        );

        // Row exists, both email/sms column/switch should render but are disabled
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-label-value]"
        ).should("be.visible");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-email-value]"
        ).should("be.visible", "not.be.enabled");
        cy.get(
            "[data-testid=profile-notification-preferences-bc-cancer-screening-sms-value]"
        ).should("be.visible", "not.be.enabled");
    });
});
