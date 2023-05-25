import {
    performSearch,
    verifySupportTableResults,
} from "../../utilities/supportUtilities";

const email = "fakeemail@healthgateway.gov.bc.ca";
const emailHdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
const emailPhn = "9735353759";
const phn = "9735353315";
const phnDuplicate = "9873967163";
const phnInvalid = "9999999999";
const phnError = "9735361219";
const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const sms = "2506715000";

function verifyParameterIsRequired(queryType) {
    performSearch(queryType, null);
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
                fixture: "SupportService/users-phn.json",
            }
        );

        // HDID with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdid}&queryType=Hdid`,
            {
                fixture: "SupportService/users-hdid.json",
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

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify support queries", () => {
        performSearch("PHN", phn);
        verifySupportTableResults(hdid, phn);

        performSearch("HDID", hdid);
        verifySupportTableResults(hdid, phn);

        performSearch("SMS", sms);
        verifySupportTableResults(hdid, phn, 2);

        performSearch("Email", email);
        verifySupportTableResults(emailHdid, emailPhn);
    });

    it("Verify support query warnings", () => {
        performSearch("PHN", phnDuplicate);
        cy.get("[data-testid=user-banner-feedback-warning-message]").should(
            "be.visible"
        );
        verifySupportTableResults(hdid, phnDuplicate);
        cy.get("[data-testid=user-banner-feedback-warning-message]").within(
            () => {
                cy.get("button").parent(".mud-alert-close").click();
            }
        );
    });

    it("Verify error handling", () => {
        performSearch("PHN", phnError);
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
        performSearch("PHN", phnInvalid);
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
        performSearch("HDID", hdid);
        verifySupportTableResults(hdid, phn);
        cy.get("[data-testid=clear-btn]").click();
        cy.get("[data-testid=query-type-select]").should("have.value", "Hdid");
        cy.get("[data-testid=query-input]").should("be.empty");
        cy.get("[data-testid=user-table]").should("not.exist");
    });
});
