const defaultHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
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
Usage for setupStandardIntercepts(options = {})

Usage: setupStandardIntercepts();
Usage: setupStandardIntercepts({ <PatientHdid> });
Usage: setupStandardIntercepts({ userProfileHdid: <UserProfileHdid> });
Usage: setupStandardIntercepts({ userProfileStatusCode: <UserProfileStatusCode> });
Usage: setupStandardIntercepts({ <SERVICE>Fixture: <ServiceFixture> });
Usage: setupStandardIntercepts({ <IDENTIFIER>: <Identifier>, <SERVICE>Fixture: <ServiceFixture> });
----------------------------------------------------------------------------------------------------
*/
export function setupStandardIntercepts(options = {}) {
    cy.log("Setting up standard intercepts.");
    const {
        patientHdid = defaultHdid,
        userProfileHdid = defaultHdid,
        patientFixture = defaultPatientFixture,
        userProfileFixture = defaultUserProfileFixture,
        userProfileStatusCode = defaultUserProfileStatusCode,
    } = options;

    setupPatientIntercept({
        hdid: patientHdid,
        patientFixture: patientFixture,
    });

    setupUserProfileIntercept({
        hdid: userProfileHdid,
        userProfileFixture: userProfileFixture,
        statusCode: userProfileStatusCode,
    });

    setupCommunicationIntercept();

    setupCommunicationIntercept({
        communicationType: CommunicationType.InApp,
        communicationFixture: CommunicationFixture.InApp,
    });
}

export function setupWaitStandardIntercepts() {
    cy.log("Setting up wait standard intercepts.");
    cy.intercept("GET", "**/Patient/*").as("getPatient");
    cy.intercept("GET", "**/UserProfile/*").as("getUserProfile");
    cy.intercept("GET", "**/Communication/*").as("getCommunication");
    cy.intercept("GET", "**/ClinicalDocument/*").as("getClinicalDocument");
    cy.intercept("GET", "**/PatientData/*").as("getPatientData");
}

export function waitStandardIntercepts() {
    cy.log("Waiting on standard intercepts.");

    const waitForIntercept = (alias, timeout) => {
        return cy.intercept(`@${alias}`, { timeout }).then((interception) => {
            if (interception) {
                cy.wait(`@${alias}`);
            } else {
                cy.log(`Intercept "${alias}" not found.`);
            }
        });
    };

    // Wait for each intercept with a reasonable timeout
    waitForIntercept("getPatient", 5000);
    waitForIntercept("getUserProfile", 5000);
    waitForIntercept("getCommunication", 5000);
    waitForIntercept("getClinicalDocument", 5000);
    waitForIntercept("getPatientData", 5000);
}

/*
----------------------------------------------------------------------------------------------------
Usage for setup<SERVICE>Intercept(options = {})

Usage: setup<SERVICE>Intercept();
Usage: setup<SERVICE>Intercept({ <Identifier> });
Usage: setup<SERVICE>Intercept({ <SERVICE>Fixture: <ServiceFixture> });
Usage: setup<SERVICE>Intercept({ <IDENTIFIER>: <Identifier>, <SERVICE>Fixture: <ServiceFixture> });
----------------------------------------------------------------------------------------------------
*/
export function setupCommunicationIntercept(options = {}) {
    const {
        communicationType = CommunicationType.Banner,
        communicationFixture = CommunicationFixture.Banner,
    } = options;

    cy.intercept("GET", `**/Communication/${communicationType}`, {
        fixture: communicationFixture,
    });
}

export function setupPatientIntercept(options = {}) {
    const { hdid = defaultHdid, patientFixture = defaultPatientFixture } =
        options;
    cy.intercept("GET", `**/Patient/${hdid}*`, {
        fixture: patientFixture,
    });
}

export function setupUserProfileIntercept(options = {}) {
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
