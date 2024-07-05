import {
    formatPhn,
    performSearch,
    verifySearchInput,
    verifySingleSupportResult,
    verifySupportTableResults,
} from "../../utilities/supportUtilities";

const email = "fakeemail@healthgateway.gov.bc.ca";
const emailHdid = "R43YCT4ZY37EIJLW2O5LV2I77BZA3K3M25EUJGWAVGVJ7JKBDKCQ";
const emailPhn = "9746209092";
const phn = "9735353315";
const phnDuplicate = "9873967163";
const phnInvalid = "9999999999";
const phnError = "9735361219";
const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const sms = "2506715000";

function verifyParameterIsRequired(queryType) {
    performSearch(queryType, null, {
        waitForUser: false,
        waitForPatientSupportDetails: false,
    });
    cy.get("div").contains("Search parameter is required").should("be.visible");
    cy.get("[data-testid=user-table]").should("not.exist");
}

describe("Support", () => {
    beforeEach(() => {
        // PHN with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phn}&queryType=Phn`,
            {
                fixture: "SupportService/users.json",
            }
        );

        // HDID with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdid}&queryType=Hdid`,
            {
                fixture: "SupportService/users.json",
            }
        );

        // SMS with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${sms}&queryType=Sms`,
            {
                fixture: "SupportService/users-sms.json",
            }
        );

        // Email with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${email}&queryType=Email`,
            {
                fixture: "SupportService/users-email.json",
            }
        );

        // PHN error
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phnError}&queryType=Phn`,
            {
                statusCode: 502,
                body: {
                    type: "Health Gateway Exception",
                    title: "Error during processing",
                    status: 502,
                    detail: "Communication error",
                },
            }
        );

        // PHN duplicate
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phnDuplicate}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-duplicate.json",
            }
        );

        // Patient support details
        cy.intercept("GET", "**/PatientSupportDetails*", {
            fixture: "SupportService/patient-details.json",
        });

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify support queries", () => {
        performSearch("PHN", phn);
        verifySingleSupportResult(hdid, phn);
        const formattedPhn = formatPhn(phn);
        verifySearchInput("Phn", formattedPhn);

        performSearch("HDID", hdid);
        verifySingleSupportResult(hdid, phn);
        verifySearchInput("Hdid", hdid);

        performSearch("SMS", sms);
        verifySupportTableResults(hdid, phn, 2);
        verifySearchInput("Sms", sms);

        performSearch("Email", email);
        verifySingleSupportResult(emailHdid, emailPhn);
        verifySearchInput("Email", email);
    });

    it("Verify support query warnings", () => {
        performSearch("PHN", phnDuplicate);
        cy.get("[data-testid=user-banner-feedback-warning-message]").should(
            "be.visible"
        );
        verifySingleSupportResult("", phnDuplicate, "PHN", phnDuplicate);
        cy.get("[data-testid=user-banner-feedback-warning-message]").within(
            () => {
                cy.get("button").parent(".mud-alert-close").click();
            }
        );
    });

    it("Verify error handling", () => {
        performSearch("PHN", phnError, {
            waitForUser: false,
            waitForPatientSupportDetails: false,
        });
        cy.get("[data-testid=user-banner-feedback-error-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=user-table]").should("not.exist");
        cy.get("[data-testid=user-banner-feedback-error-message]").within(
            () => {
                cy.get("button").parent(".mud-alert-close").click();
            }
        );
    });

    it("Verify query fails on invalid PHN", () => {
        performSearch("PHN", phnInvalid, {
            waitForUser: false,
            waitForPatientSupportDetails: false,
        });
        cy.get(".d-flex").contains("Invalid PHN").should("be.visible");
        cy.get("[data-testid=user-table]").should("not.exist");
    });

    it("Verify query fails without parameter", () => {
        verifyParameterIsRequired("PHN");
        verifyParameterIsRequired("HDID");
        verifyParameterIsRequired("SMS");
        verifyParameterIsRequired("Email");
        verifyParameterIsRequired("Dependent");
    });

    it("Verify clear button", () => {
        performSearch("SMS", sms, {
            waitForPatientSupportDetails: false,
        });
        verifySupportTableResults(hdid, phn, 2);
        cy.get("[data-testid=clear-btn]").click();
        cy.get("[data-testid=query-type-select]").should("have.value", "Sms");
        cy.get("[data-testid=query-input]").should("be.empty");
        cy.get("[data-testid=user-table]").should("not.exist");
    });
});
