import {
    formatPhn,
    performSearch,
    verifySearchInput,
    verifySingleSupportResult,
    verifySupportTableResults,
} from "../../utilities/supportUtilities";
import { getTableRows } from "../../utilities/sharedUtilities";

const email = "fakeemail@healthgateway.gov.bc.ca";
const emailNotFound = "fakeemail_noresults@healthgateway.gov.bc.ca";
const emailHdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
const emailPhn = "9735353759";
const phn = "9735353315";
const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const hdidNotFound = "P123456789";
const sms = "2506715000";
const smsNotFound = "5551234567";
const dependentPhn = "9874307175";

describe("Support", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify support query.", () => {
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
        getTableRows("[data-testid=user-table]").should("have.length", 2);

        performSearch("Email", email);
        verifySingleSupportResult(emailHdid, emailPhn);
        verifySearchInput("Email", email);
    });

    it("Verify no results hdid query.", () => {
        performSearch("HDID", hdidNotFound);
        getTableRows("[data-testid=user-table]").should("have.length", 0);
    });

    it("Verify no results sms query.", () => {
        performSearch("SMS", smsNotFound);
        getTableRows("[data-testid=user-table]").should("have.length", 0);
    });

    it("Verify no results email query.", () => {
        performSearch("Email", emailNotFound);
        getTableRows("[data-testid=user-table]").should("have.length", 0);
    });

    it("Verify dependents query returns results.", () => {
        performSearch("Dependent", dependentPhn);
        verifySingleSupportResult(hdid, phn);
    });
});
