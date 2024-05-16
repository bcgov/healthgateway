const defaultHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const defaultNotificationFixture = "NotificationService/notifications.json";
const defaultPatientFixture = "PatientService/patientCombinedAddress.json";
const defaultUserProfileFixture = "UserProfileService/userProfile.json";
const defaultUserProfileStatusCode = 200;
const defaultTimeout = 60000;

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
        userProfileStatusCode = defaultUserProfileStatusCode,
    } = options;

    setupPatientFixture({
        hdid: patientHdid,
        patientFixture: patientFixture,
    });

    setupUserProfileFixture({
        hdid: userProfileHdid,
        userProfileFixture: userProfileFixture,
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
        statusCode = defaultUserProfileStatusCode,
    } = options;

    if (statusCode === 200) {
        cy.intercept("GET", `**/UserProfile/${hdid}`, {
            fixture: userProfileFixture,
        });
    } else {
        cy.intercept("GET", `**/UserProfile/${hdid}`, {
            statusCode,
        });
    }
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
    cy.intercept("GET", "**/UserProfile/*").as("getUserProfile");
}

export function waitForInitialDataLoad(username, config, path) {
    const featureToggle = config.webClient.featureToggleConfiguration;

    cy.log(`Username: ${username}`);
    cy.log(`Feature Toggle: ${JSON.stringify(featureToggle)}`);
    cy.log(`Path: ${path}`);

    cy.log("Wait on patient.");
    cy.wait("@getPatient", { timeout: defaultTimeout });

    waitForUserProfile(username).then((blockedDataSources) => {
        waitForClinicalDocument(featureToggle, path, blockedDataSources);
        waitForOrganDonorRegistratonStatusService(
            featureToggle,
            path,
            blockedDataSources
        );
        waitForPatientDataForBcCancerScreening(
            featureToggle,
            path,
            blockedDataSources
        );
        waitForPatientDataForDiagnosticImaging(
            featureToggle,
            path,
            blockedDataSources
        );
    });

    cy.log("Wait on communication.");
    cy.wait("@getCommunication", { timeout: defaultTimeout });

    waitForNotification(featureToggle);
}

function waitForUserProfile(username) {
    return new Cypress.Promise((resolve) => {
        let blockedDataSources;

        if (
            username !== Cypress.env("keycloak.deceased.username") &&
            username !== Cypress.env("keycloak.accountclosure.username")
        ) {
            cy.log("Wait on user profile.");
            cy.wait("@getUserProfile", { timeout: defaultTimeout }).then(
                (interception) => {
                    const responseBody = interception.response.body;
                    blockedDataSources =
                        responseBody.resourcePayload?.blockedDataSources;

                    cy.log(
                        `Get User Profile Blocked Data Sources: ${
                            blockedDataSources
                                ? JSON.stringify(blockedDataSources)
                                : "Blocked data sources are not available"
                        }`
                    );

                    resolve(blockedDataSources);
                }
            );
        } else {
            resolve(blockedDataSources);
        }
    });
}

function waitForNotification(featureToggle) {
    cy.log(
        `waitForNotification called - enabled: ${featureToggle.notificationCentre.enabled}`
    );

    if (
        featureToggle.notificationCentre &&
        featureToggle.notificationCentre.enabled
    ) {
        cy.log("Wait on notification.");
        cy.wait("@getNotification", { timeout: defaultTimeout });
    }
}

function waitForClinicalDocument(featureToggle, path, blockedDataSources) {
    const clinicalDocumentBlocked =
        checkClinicalDocumentBlocked(blockedDataSources);

    const clinicalDocumentEnabled = featureToggle?.datasets.some(
        (x) => x.enabled && x.name === "clinicalDocument"
    );

    const dependentClinicalDocumentEnabled =
        featureToggle.dependents.enabled &&
        featureToggle.dependents.datasets.some(
            (x) => x.enabled && x.name === "clinicalDocument"
        );

    cy.log(
        `waitForClinicalDocument called - enabled: ${clinicalDocumentEnabled} - dependent enabled: ${dependentClinicalDocumentEnabled} - blocked: ${clinicalDocumentBlocked}`
    );

    if (
        (clinicalDocumentEnabled || dependentClinicalDocumentEnabled) &&
        !clinicalDocumentBlocked &&
        isTimelineOrDependentsTimeline(path)
    ) {
        cy.log("Wait on clinical document.");
        cy.wait("@getClinicalDocument", { timeout: defaultTimeout });
    }
}

function waitForPatientDataForBcCancerScreening(
    featureToggle,
    path,
    blockedDataSources
) {
    const bcCancerScreeningBlocked =
        checkBcCancerScreeningBlocked(blockedDataSources);

    const bcCancerScreeningEnabled = featureToggle?.datasets.some(
        (x) => x.enabled && x.name === "bcCancerScreening"
    );

    const dependentBcCancerScreeningEnabled =
        featureToggle.dependents.enabled &&
        featureToggle.dependents.datasets.some(
            (x) => x.enabled && x.name === "bcCancerScreening"
        );

    cy.log(
        `waitForPatientDataForBcCancerScreening called - enabled: ${bcCancerScreeningEnabled} - dependent enabled: ${dependentBcCancerScreeningEnabled} - blocked: ${bcCancerScreeningBlocked}`
    );

    if (
        (bcCancerScreeningEnabled || dependentBcCancerScreeningEnabled) &&
        !bcCancerScreeningBlocked &&
        isTimelineOrDependentsTimeline(path)
    ) {
        cy.log("Wait on patient data for bc cancer screening.");
        cy.wait("@getPatientDataForBcCancerScreening", {
            timeout: defaultTimeout,
        });
    }
}

function waitForPatientDataForDiagnosticImaging(
    featureToggle,
    path,
    blockedDataSources
) {
    const diagnosticImagingBlocked =
        checkDiagnosticImagingBlocked(blockedDataSources);

    const diagnosticImagingEnabled = featureToggle?.datasets.some(
        (x) => x.enabled && x.name === "diagnosticImaging"
    );

    const dependentDiagnosticImagingEnabled =
        featureToggle.dependents.enabled &&
        featureToggle.dependents.datasets.some(
            (x) => x.enabled && x.name === "diagnosticImaging"
        );

    cy.log(
        `waitForPatientDataForDiagnosticImaging called - enabled: ${diagnosticImagingEnabled} - dependent enabled: ${dependentDiagnosticImagingEnabled} - blocked: ${diagnosticImagingBlocked}`
    );

    if (
        (diagnosticImagingEnabled || dependentDiagnosticImagingEnabled) &&
        !diagnosticImagingBlocked &&
        isTimelineOrDependentsTimeline(path)
    ) {
        cy.log("Wait on patient data for diagnostic imaging.");
        cy.wait("@getPatientDataForDiagnosticImaging", {
            timeout: defaultTimeout,
        });
    }
}

function waitForOrganDonorRegistratonStatusService(
    featureToggle,
    path,
    blockedDataSources
) {
    const organDonorRegistrationBlocked =
        checkOrganDonorRegistrationBlocked(blockedDataSources);

    const organDonorRegistrationEnabled =
        featureToggle.services &&
        featureToggle.services.enabled &&
        featureToggle.datasets.some(
            (x) => x.enabled && x.name === "organDonorRegistration"
        );

    cy.log(
        `waitForOrganDonorRegistratonStatusService called - enabled: ${organDonorRegistrationEnabled} - blocked: ${organDonorRegistrationBlocked}`
    );

    if (
        organDonorRegistrationEnabled &&
        !organDonorRegistrationBlocked &&
        path === "/services"
    ) {
        cy.log("Wait on patient data for organ donor registration.");
        cy.wait("@getOrganDonorRegistrationStatus", {
            timeout: defaultTimeout,
        });
    }
}

function checkBcCancerScreeningBlocked(blockedDataSources) {
    return (
        blockedDataSources &&
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("BcCancerScreening")
    );
}

function checkClinicalDocumentBlocked(blockedDataSources) {
    return (
        blockedDataSources &&
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("ClinicalDocument")
    );
}

function checkDiagnosticImagingBlocked(blockedDataSources) {
    return (
        blockedDataSources &&
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("DiagnosticImaging")
    );
}

function checkOrganDonorRegistrationBlocked(blockedDataSources) {
    return (
        blockedDataSources &&
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("OrganDonorRegistration")
    );
}

function checkHealthConnectRegistryBlocked(blockedDataSources) {
    return (
        blockedDataSources &&
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("HelathConnectRegistry")
    );
}

function isTimelineOrDependentsTimeline(path) {
    return (
        path === "/timeline" ||
        (path.startsWith("/dependents/") && path.endsWith("/timeline"))
    );
}
