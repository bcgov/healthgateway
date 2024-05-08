const defaultHdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const defaultPatientFixture = "PatientService/patientCombinedAddress.json";
const defaultUserProfileFixture = "UserProfileService/userProfile.json";

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
    } = options;
    cy.intercept("GET", `**/UserProfile/${hdid}`, {
        fixture: userProfileFixture,
    });
}
