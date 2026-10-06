const defaultTimeout = 60000;

// Preserve existing readiness conditions while separating them from interception.
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
        waitForHospitalVisit(featureToggle, path, blockedDataSources);
    });

    cy.log("Wait on communication.");
    cy.wait("@getCommunication", { timeout: defaultTimeout });

    waitForNotification(featureToggle);
    waitForDependent(featureToggle, path);
}

function waitForUserProfile(username) {
    if (
        username === Cypress.env("keycloak.deceased.username") ||
        username === Cypress.env("keycloak.accountclosure.username")
    ) {
        return cy.wrap(undefined, { log: false });
    }

    cy.log("Wait on user profile.");
    return cy
        .wait("@getUserProfile", { timeout: defaultTimeout })
        .then((interception) => {
            const blockedDataSources =
                interception.response.body.blockedDataSources;

            cy.log(
                `Get User Profile Blocked Data Sources: ${
                    blockedDataSources
                        ? JSON.stringify(blockedDataSources)
                        : "Blocked data sources are not available"
                }`
            );

            return cy.wrap(blockedDataSources, { log: false });
        });
}

function waitForNotification(featureToggle) {
    cy.log(
        `waitForNotification called - enabled: ${featureToggle.notificationCentre.enabled}`
    );

    if (featureToggle.notificationCentre.enabled) {
        cy.log("Wait on notification.");
        cy.wait("@getNotification", { timeout: defaultTimeout });
    }
}

function waitForHospitalVisit(featureToggle, path, blockedDataSources) {
    const hospitalVisitBlocked = checkHospitalVisitBlocked(blockedDataSources);

    const hospitalVisitEnabled = featureToggle?.datasets.some(
        (x) => x.enabled && x.name === "hospitalVisit"
    );

    const dependentHospitalVisitDisabled =
        featureToggle.dependents.datasets.some(
            (x) => !x.enabled && x.name === "hospitalVisit"
        );

    cy.log(
        `waitForHospitalVisit called - enabled: ${hospitalVisitEnabled} - dependent disabled: ${dependentHospitalVisitDisabled} - blocked: ${hospitalVisitBlocked}`
    );

    if (
        !hospitalVisitBlocked &&
        hospitalVisitEnabled &&
        (isTimeline(path) ||
            (isDependentsTimeline(path) && !dependentHospitalVisitDisabled))
    ) {
        cy.log("Wait on hospital visit.");
        cy.wait("@getHospitalVisit", {
            timeout: defaultTimeout,
        });
    }
}

function waitForClinicalDocument(featureToggle, path, blockedDataSources) {
    const clinicalDocumentBlocked =
        checkClinicalDocumentBlocked(blockedDataSources);

    const clinicalDocumentEnabled = featureToggle?.datasets.some(
        (x) => x.enabled && x.name === "clinicalDocument"
    );

    const dependentClinicalDocumentDisabled =
        featureToggle.dependents.datasets.some(
            (x) => !x.enabled && x.name === "clinicalDocument"
        );

    cy.log(
        `waitForClinicalDocument called - enabled: ${clinicalDocumentEnabled} - dependent disabled: ${dependentClinicalDocumentDisabled} - blocked: ${clinicalDocumentBlocked}`
    );

    if (
        !clinicalDocumentBlocked &&
        clinicalDocumentEnabled &&
        (isTimeline(path) ||
            (isDependentsTimeline(path) && !dependentClinicalDocumentDisabled))
    ) {
        cy.log("Wait on clinical document.");
        cy.wait("@getClinicalDocument", { timeout: defaultTimeout });
    }
}

function waitForDependent(featureToggle, path) {
    cy.log(
        `waitForDependent called - enabled: ${featureToggle.dependents.enabled} - path: ${path}`
    );

    if (featureToggle.dependents.enabled && isDependents(path)) {
        cy.log("Wait on dependent.");
        cy.wait("@getDependent", { timeout: defaultTimeout });
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

    const dependentBcCancerScreeningDisabled =
        featureToggle.dependents.datasets.some(
            (x) => !x.enabled && x.name === "bcCancerScreening"
        );

    cy.log(
        `waitForPatientDataForBcCancerScreening called - enabled: ${bcCancerScreeningEnabled} - dependent disabled: ${dependentBcCancerScreeningDisabled} - blocked: ${bcCancerScreeningBlocked}`
    );

    if (
        !bcCancerScreeningBlocked &&
        bcCancerScreeningEnabled &&
        (isTimeline(path) ||
            (isDependentsTimeline(path) && !dependentBcCancerScreeningDisabled))
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

    const dependentDiagnosticImagingDisabled =
        featureToggle.dependents.datasets.some(
            (x) => !x.enabled && x.name === "diagnosticImaging"
        );

    cy.log(
        `waitForPatientDataForDiagnosticImaging called - enabled: ${diagnosticImagingEnabled} - dependent disabled: ${dependentDiagnosticImagingDisabled} - blocked: ${diagnosticImagingBlocked}`
    );

    if (
        !diagnosticImagingBlocked &&
        diagnosticImagingEnabled &&
        (isTimeline(path) ||
            (isDependentsTimeline(path) && !dependentDiagnosticImagingDisabled))
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
        featureToggle.services.services.some(
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
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("BcCancerScreening")
    );
}

function checkHospitalVisitBlocked(blockedDataSources) {
    return (
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("HospitalVisit")
    );
}

function checkClinicalDocumentBlocked(blockedDataSources) {
    return (
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("ClinicalDocument")
    );
}

function checkDiagnosticImagingBlocked(blockedDataSources) {
    return (
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("DiagnosticImaging")
    );
}

function checkOrganDonorRegistrationBlocked(blockedDataSources) {
    return (
        Array.isArray(blockedDataSources) &&
        blockedDataSources.includes("OrganDonorRegistration")
    );
}

function isDependents(path) {
    return path === "/dependents";
}

function isDependentsTimeline(path) {
    return path.startsWith("/dependents/") && path.endsWith("/timeline");
}

function isTimeline(path) {
    return path === "/timeline";
}
