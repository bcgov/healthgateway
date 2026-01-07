import { getTableRows, selectTab } from "../../utilities/sharedUtilities";
import { performSearch } from "../../utilities/supportUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const hdidPatientDeceased =
    "MPOLGP66AV4PPDB6ZMYEWQ63WKRYPM4EPDW5MSXA2LA65EQOEMCQ";
const hdidPatientNotUser =
    "C54JQKXANHJK7TIYBRCHJBOKXOIXASNQE76WSRCTOPKOXRMI5OAA";
const phn = "9735353315";
const phnPatientDeceased = "9873224879";
const phnPatientNotUser = "9872868128";
const datasetName = "Immunization";
const badRequest = 400;
const auditReasonInput = "Test block reason";
const auditReasonFeedback =
    "Response status code does not indicate success: 400 (Bad Request).";

function selectPatientTab(tabText) {
    selectTab("[data-testid=patient-details-tabs]", tabText);
}

describe("Patient details as admin", () => {
    beforeEach(() => {
        // PHN with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phn}`,
            {
                fixture: "SupportService/users.json",
            }
        );

        // PHN deceased
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phnPatientDeceased}`,
            {
                fixture: "SupportService/users-patient-deceased.json",
            }
        );

        // PHN not a user
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phnPatientNotUser}`,
            {
                fixture: "SupportService/users-patient-not-user.json",
            }
        );

        // Patient Details
        cy.intercept(
            "GET",
            `**/PatientSupportDetails?queryType=Phn&queryString=${phn}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        // Patient Details with deceased PHN
        cy.intercept(
            "GET",
            `**/PatientSupportDetails?queryType=Phn&queryString=${phnPatientDeceased}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        // Patient Details with not a user PHN
        cy.intercept(
            "GET",
            `**/PatientSupportDetails?queryType=Phn&queryString=${phnPatientNotUser}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        // Block Access
        cy.intercept(
            "PUT",
            `**/Support/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/BlockAccess`,
            {
                statusCode: badRequest,
            }
        );

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify patient details", () => {
        performSearch("PHN", phn);

        selectPatientTab("Profile");

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        selectPatientTab("Account");

        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdid);

        cy.get("[data-testid=api-registration]")
            .should("be.visible")
            .contains("True");

        cy.get("[data-testid=profile-created-datetime]").should("be.visible");
        cy.get("[data-testid=profile-last-login-datetime]").should(
            "be.visible"
        );
        getTableRows("[data-testid=messaging-verification-table]").should(
            "have.length",
            2
        );
    });

    it("Verify patient deceased", () => {
        performSearch("PHN", phnPatientDeceased);

        selectPatientTab("Profile");

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        selectPatientTab("Account");

        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdidPatientDeceased);
        cy.get("[data-testid=profile-created-datetime]").should("not.exist");
        cy.get("[data-testid=profile-last-login-datetime]").should("not.exist");
        cy.get("[data-testid=messaging-verification-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=no-hg-profile]").should("be.visible");

        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient is deceased");
    });

    it("Verify patient not a user", () => {
        performSearch("PHN", phnPatientNotUser);

        selectPatientTab("Profile");

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        selectPatientTab("Account");

        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdidPatientNotUser);

        cy.get("[data-testid=profile-created-datetime]").should("not.exist");
        cy.get("[data-testid=profile-last-login-datetime]").should("not.exist");
        cy.get("[data-testid=messaging-verification-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=no-hg-profile]").should("be.visible");

        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient is not a user");
    });

    it("Verify block access modal handles 400 error", () => {
        performSearch("PHN", phn);

        selectPatientTab("Account");

        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdid);

        selectPatientTab("Manage");

        cy.get("[data-testid=block-access-loader]").should("not.be.visible");

        cy.get(`[data-testid=block-access-switch-${datasetName}]`)
            .should("exist")
            .click();

        cy.get(`[data-testid=block-access-switch-${datasetName}]`).should(
            "be.checked"
        );

        cy.get("[data-testid=block-access-save]")
            .should("exist", "be.visible")
            .click();

        cy.get("[data-testid=audit-reason-input")
            .should("be.visible")
            .type(auditReasonInput);

        cy.get("body").click(0, 0);
        cy.get("[data-testid=audit-confirm-button]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=audit-reason-feedback]")
            .should("be.visible")
            .contains(auditReasonFeedback);

        cy.get("[data-testid=audit-cancel-button]")
            .should("be.visible")
            .click();
    });

    it("Verify print vaccine card shows error message", () => {
        cy.intercept("GET", `**/Document?phn=${phn}`, {
            statusCode: 500,
        });

        performSearch("PHN", phn);
        selectPatientTab("Profile");
        cy.scrollTo("bottom", { ensureScrollable: false });
        cy.get("[data-testid=print-button]").should("be.visible").click();
        cy.scrollTo("top", { ensureScrollable: false });
        cy.get(
            "[data-testid=user-banner-print-vaccine-card-error-message]"
        ).should("be.visible");
    });
});

describe("Patient details as support", () => {
    beforeEach(() => {
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phn}`,
            {
                fixture: "SupportService/users.json",
            }
        );

        // PHN deceased
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phnPatientDeceased}`,
            {
                fixture: "SupportService/users-patient-deceased.json",
            }
        );

        // PHN not a user
        cy.intercept(
            "GET",
            `**/Support/Users?queryType=Phn&queryString=${phnPatientNotUser}`,
            {
                fixture: "SupportService/users-patient-not-user.json",
            }
        );

        // Patient Details with deceased PHN
        cy.intercept(
            "GET",
            `**/PatientSupportDetails?queryType=Phn&queryString=${phnPatientDeceased}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        // Patient Details with not a user PHN
        cy.intercept(
            "GET",
            `**/PatientSupportDetails?queryType=Phn&queryString=${phnPatientNotUser}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        // Patient Details with one dose
        cy.intercept(
            "GET",
            `**/Support/PatientSupportDetails?queryType=Phn&queryString=${phn}&refreshVaccineDetails=False&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details-one-dose.json",
            }
        );

        // Patient Details with vaccine details with multiple doses
        cy.intercept(
            "GET",
            `**/Support/PatientSupportDetails?queryType=Phn&queryString=${phn}&refreshVaccineDetails=True&includeApiRegistration=True&includeImagingRefresh=True&includeLabsRefresh=True`,
            {
                fixture: "SupportService/patient-details.json",
            }
        );

        cy.login(
            Cypress.env("keycloak_support_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify patient details with phn and refresh", () => {
        performSearch("PHN", phn);
        cy.get("[data-testid=patient-phn]").should("be.visible").contains(phn);

        getTableRows("[data-testid=immunization-table]").should(
            "have.length",
            1
        );

        cy.get("[data-testid=refresh-button]").click();

        getTableRows("[data-testid=immunization-table]").should(
            "have.length",
            5
        );
    });

    it("Verify patient deceased", () => {
        performSearch("PHN", phnPatientDeceased);

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]").should("not.exist");
        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient is deceased");
    });

    it("Verify patient not a user", () => {
        performSearch("PHN", phnPatientNotUser);

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]").should("not.exist");
        cy.get(
            "[data-testid=user-banner-feedback-status-warning-message]"
        ).should("not.exist");
    });
});
