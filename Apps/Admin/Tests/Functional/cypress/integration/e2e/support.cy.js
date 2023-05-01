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

function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr.mud-table-row");
}

function verifyUserTableResults(expectedRowCount = 1) {
    getTableRows("[data-testid=user-table]")
        .should("have.length", expectedRowCount)
        .first((_$rows) => {
            cy.get(`[data-testid=user-table-hdid-${hdid}]`).contains(hdid);
            cy.get(`[data-testid=user-table-phn-${hdid}]`).contains(phn);
        });
}

describe("Support", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify support query.", () => {
        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        verifyUserTableResults();

        // Search by HDID.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("HDID")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(hdid);
        cy.get("[data-testid=search-btn]").click();
        verifyUserTableResults();

        // Search by SMS.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("SMS")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(sms);
        cy.get("[data-testid=search-btn]").click();
        verifyUserTableResults(2);

        // Search by Email.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Email")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(email);
        cy.get("[data-testid=search-btn]").click();
        getTableRows("[data-testid=user-table]").should("have.length", 1);
        cy.get(`[data-testid=user-table-hdid-${emailHdid}]`).contains(
            emailHdid
        );
        cy.get(`[data-testid=user-table-phn-${emailHdid}]`).contains(emailPhn);
    });

    it("Verify no results hdid query.", () => {
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("HDID")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(hdidNotFound);
        cy.get("[data-testid=search-btn]").click();
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

    it("Verify no results sms query.", () => {
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("SMS")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(smsNotFound);
        cy.get("[data-testid=search-btn]").click();
        getTableRows("[data-testid=user-table]").should("have.length", 0);
    });

    it("Verify no results email query.", () => {
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Email")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(emailNotFound);
        cy.get("[data-testid=search-btn]").click();
        getTableRows("[data-testid=user-table]").should("have.length", 0);
    });

    it("Verify dependents query returns results.", () => {
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Dependent")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(dependentPhn);
        cy.get("[data-testid=search-btn]").click();
        getTableRows("[data-testid=user-table]").should("have.length", 1);
    });
});
