const defaultHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const defaultNotificationFixture = "NotificationService/notifications.json";
const defaultPatientFixture = "PatientService/patientCombinedAddress.json";
const defaultUserProfileFixture = "UserProfileService/userProfile.json";
const defaultUserProfileStatusCode = 200;

export const CommunicationType = {
    Banner: 0,
    InApp: 2,
};

export const CommunicationFixture = {
    Banner: "CommunicationService/communicationBanner.json",
    InApp: "CommunicationService/communicationInApp.json",
};

/*
----------------------------------------------------------------------------------------------------
Usage for setupStandardFixtures(options = {})

Usage: setupStandardFixtures();
Usage: setupStandardFixtures({ <PatientHdid> });
Usage: setupStandardFixtures({ userProfileHdid: <UserProfileHdid> });
Usage: setupStandardFixtures({ userProfileStatusCode: <UserProfileStatusCode> });
Usage: setupStandardFixtures({ <SERVICE>Fixture: <ServiceFixture> });
Usage: setupStandardFixtures({ <IDENTIFIER>: <Identifier>, <SERVICE>Fixture: <ServiceFixture> });
----------------------------------------------------------------------------------------------------
*/
export function setupStandardFixtures(options = {}) {
    cy.log("Setting up standard intercepts.");

    const {
        patientHdid = defaultHdid,
        userProfileHdid = defaultHdid,
        notificationHdid = defaultHdid,
        notificationFixture = defaultNotificationFixture,
        patientFixture = defaultPatientFixture,
        userProfileFixture = defaultUserProfileFixture,
        userProfileBody = undefined,
        userProfileStatusCode = defaultUserProfileStatusCode,
    } = options;

    setupPatientFixture({
        hdid: patientHdid,
        patientFixture: patientFixture,
    });

    setupUserProfileFixture({
        hdid: userProfileHdid,
        userProfileFixture: userProfileFixture,
        userProfileBody,
        statusCode: userProfileStatusCode,
    });

    setupCommunicationFixture();

    setupCommunicationFixture({
        communicationType: CommunicationType.InApp,
        communicationFixture: CommunicationFixture.InApp,
    });

    setupNotificationFixture({
        hdid: notificationHdid,
        notificationFixture: notificationFixture,
    });
}

/*
----------------------------------------------------------------------------------------------------
Usage for setup<SERVICE>Fixture(options = {})

Usage: setup<SERVICE>Fixture();
Usage: setup<SERVICE>Fixture({ <Identifier> });
Usage: setup<SERVICE>Fixture({ <SERVICE>Fixture: <ServiceFixture> });
Usage: setup<SERVICE>Fixture({ <IDENTIFIER>: <Identifier>, <SERVICE>Fixture: <ServiceFixture> });
----------------------------------------------------------------------------------------------------
*/
export function setupCommunicationFixture(options = {}) {
    const {
        communicationType = CommunicationType.Banner,
        communicationFixture = CommunicationFixture.Banner,
    } = options;

    cy.intercept("GET", `**/Communication/${communicationType}`, {
        fixture: communicationFixture,
    });
}

export function setupPatientFixture(options = {}) {
    const { hdid = defaultHdid, patientFixture = defaultPatientFixture } =
        options;

    cy.intercept("GET", `**/Patient/${hdid}*`, {
        fixture: patientFixture,
    });
}

export function setupUserProfileFixture(options = {}) {
    const {
        hdid = defaultHdid,
        userProfileFixture = defaultUserProfileFixture,
        userProfileBody = undefined,
        statusCode = defaultUserProfileStatusCode,
    } = options;

    const urls = [
        `**/UserProfile/${hdid}?api-version=2.0`,
        `**/UserProfile/${hdid}/login?api-version=2.0`,
    ];

    urls.forEach((url) => {
        if (statusCode !== 200) {
            cy.intercept("GET", url, { statusCode });
            return;
        }

        if (userProfileBody !== undefined) {
            cy.intercept("GET", url, {
                statusCode: 200,
                body: userProfileBody,
            });
            return;
        }

        cy.intercept("GET", url, { fixture: userProfileFixture });
    });
}

export function setupNotificationFixture(options = {}) {
    const {
        hdid = defaultHdid,
        notificationFixture = defaultNotificationFixture,
    } = options;

    cy.intercept("GET", `**/Notification/${hdid}`, {
        fixture: notificationFixture,
    });
}

export function setupStandardAliases() {
    cy.log("Setting up standard aliases.");

    cy.intercept("GET", "**/Encounter/HospitalVisit/*").as("getHospitalVisit");
    cy.intercept("GET", "**/ClinicalDocument/*").as("getClinicalDocument");
    cy.intercept("GET", `**/Communication/*`).as("getCommunication");
    cy.intercept("GET", "**/Notification/*").as("getNotification");
    cy.intercept("GET", "**/Patient/*").as("getPatient");
    cy.intercept(
        "GET",
        "**/PatientData/*patientDataTypes=BcCancerScreening*"
    ).as("getPatientDataForBcCancerScreening");
    cy.intercept(
        "GET",
        "**/PatientData/*patientDataTypes=DiagnosticImaging*"
    ).as("getPatientDataForDiagnosticImaging");
    cy.intercept(
        "GET",
        "**/PatientData/*?patientDataTypes=OrganDonorRegistrationStatus*"
    ).as("getOrganDonorRegistrationStatus");
    // Terms of service is not a profile response (it has no blockedDataSources).
    cy.intercept({
        method: "GET",
        pathname: /\/UserProfile\/(?!termsofservice(?:\/|$))[^/]+(?:\/login)?$/,
        query: { "api-version": "2.0" },
    }).as("getUserProfile");
    cy.intercept("GET", "**/UserProfile/*/Dependent*").as("getDependent");
}
