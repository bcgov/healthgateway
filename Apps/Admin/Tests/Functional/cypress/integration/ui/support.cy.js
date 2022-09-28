const email = "fakeemail@healthgateway.gov.bc.ca";
const emailNotFound = "fakeemail_noresults@healthgateway.gov.bc.ca";
const emailHdid = "DEV4FPEGCXG2NB5K2USBL52S66SC3GOUHWRP3GTXR2BTY5HEC4YA";
const phn = "9735353315";
const phnInvalid = "9999999999";
const phnNotFound = "9735361219";
const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const hdidNotFound = "P123456789";
const sms = "2501234567";
const smsNotFound = "5551234567";

function verifyTableResults(queryType) {
    // Expecting 1 row to return but you also need to consider the table headers. As a result, length should be 2.
    cy.get("[data-testid=message-verification-table]")
        .find("tr")
        .should("have.length", 2);

    // Hdid is unique and is used as a unique identifier for each row in the table.
    cy.get(`[data-testid=message-verification-table-hdid-${hdid}]`).contains(
        hdid
    );

    if (queryType === "SMS") {
        cy.get(`[data-testid=message-verification-table-phn-${hdid}]`).should(
            "be.empty"
        );
    } else {
        cy.get(`[data-testid=message-verification-table-phn-${hdid}]`).contains(
            phn
        );
    }
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
            `**/Support/Users?queryString=${hdid}&queryType=HDID`,
            {
                fixture: "SupportService/users-hdid.json",
            }
        );

        // SMS with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${sms}&queryType=SMS`,
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

        // PHN no results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phnNotFound}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-no-results.json",
            }
        );

        // HDID no results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdidNotFound}&queryType=HDID`,
            {
                fixture: "SupportService/users-hdid-no-results.json",
            }
        );

        // SMS no results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${smsNotFound}&queryType=SMS`,
            {
                fixture: "SupportService/users-sms-no-results.json",
            }
        );

        // Email no results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${emailNotFound}&queryType=Email`,
            {
                fixture: "SupportService/users-email-no-results.json",
            }
        );

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify support query.", () => {
        cy.log("Verify support query test started.");

        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        verifyTableResults("PHN");

        // Search by HDID.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("HDID")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(hdid);
        cy.get("[data-testid=search-btn]").click();
        verifyTableResults("HDID");

        // Search by SMS.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("SMS")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(sms);
        cy.get("[data-testid=search-btn]").click();
        verifyTableResults("SMS");

        // Search by Email.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Email")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(email);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=message-verification-table]")
            .find("tr")
            .should("have.length", 2);
        cy.get(
            `[data-testid=message-verification-table-hdid-${emailHdid}]`
        ).contains(emailHdid);
        cy.get(
            `[data-testid=message-verification-table-phn-${emailHdid}]`
        ).should("be.empty");

        cy.log("Verify support query test finished.");
    });

    it("Verify support no results query errors.", () => {
        cy.log("Verify phn returns no results test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phnNotFound);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-error-message]").should(
            "be.visible"
        );

        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.get("[data-testid=banner-feedback-error-message]").within(() => {
            cy.get("button").parent(".mud-alert-close").click();
        });

        cy.log("Verify phn returns no results test finished.");

        cy.log("Verify hdid returns no results test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("HDID")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(hdidNotFound);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-error-message]").should(
            "be.visible"
        );
        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.get("[data-testid=banner-feedback-error-message]").within(() => {
            cy.get("button").parent(".mud-alert-close").click();
        });

        cy.log("Verify hdid returns no results test finished.");

        cy.log("Verify sms returns no results test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("SMS")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(smsNotFound);
        cy.get("[data-testid=search-btn]").click();

        cy.get("[data-testid=message-verification-table]")
            .find("tr")
            .should("have.length", 1);

        cy.log("Verify sms returns no results test finished.");

        cy.log("Verify email returns no results test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Email")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(emailNotFound);
        cy.get("[data-testid=search-btn]").click();

        cy.get("[data-testid=message-verification-table]")
            .find("tr")
            .should("have.length", 1);

        cy.log("Verify email returns no results test finished.");
    });

    it("Verify invalid phn value.", () => {
        cy.log("Verify invalid phn test started.");

        // Search by invalid PHN.
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phnInvalid);
        cy.get("[data-testid=search-btn]").click();
        cy.get(".d-flex").contains("Invalid PHN").should("be.visible");
        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.log("Verify invalid phn test finished.");
    });

    it("Verify parameter value missing.", () => {
        cy.log("Verify phn parameter value missing test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear();
        cy.get("[data-testid=search-btn]").click();

        cy.get("div")
            .contains("Search parameter is required")
            .should("be.visible");
        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.log("Verify phn parameter value missing test finished.");

        cy.log("Verify hdid parameter value missing test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("HDID")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear();
        cy.get("[data-testid=search-btn]").click();

        cy.get("div")
            .contains("Search parameter is required")
            .should("be.visible");
        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.log("Verify hdid parameter value missing test finished.");

        cy.log("Verify sms parameter value missing test started.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("SMS")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear();
        cy.get("[data-testid=search-btn]").click();

        cy.get("div")
            .contains("Search parameter is required")
            .should("be.visible");

        cy.log("Verify sms parameter value missing test finished.");

        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("Email")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear();
        cy.get("[data-testid=search-btn]").click();

        cy.get("div")
            .contains("Search parameter is required")
            .should("be.visible");
        cy.get("[data-testid=message-verification-table]").should("not.exist");

        cy.log("Verify email parameter value missing test finished.");
    });
});

describe("Support - Client registry task handling", () => {
    beforeEach(() => {
        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify Phn user subject of overlay.", () => {
        const phn = "9873967163";
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phn}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-overlay.json",
            }
        );

        cy.log("Verify Phn user subject of overlay started.");

        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-warning-message]").should(
            "be.visible"
        );

        cy.log("Verify Phn user subject of overlay finished.");
    });

    it("Verify Phn user subject of potential duplicate.", () => {
        const phn = "9874359198";
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phn}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-potential-duplicate.json",
            }
        );

        cy.log("Verify Phn user subject of potential duplicate started.");

        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-warning-message]").should(
            "be.visible"
        );

        cy.log("Verify Phn user subject of potential duplicate finished.");
    });

    it("Verify Phn user subject of potential linkage.", () => {
        const phn = "9698653896";
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phn}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-potential-linkage.json",
            }
        );

        cy.log("Verify Phn user subject of potential linkage started.");

        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-warning-message]").should(
            "be.visible"
        );

        cy.log("Verify Phn user subject of potential linkage finished.");
    });

    it("Verify Phn user subject of review identifier.", () => {
        const phn = "9874166257";
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${phn}&queryType=Phn`,
            {
                fixture: "SupportService/users-phn-review-identifier.json",
            }
        );

        cy.log("Verify Phn user subject of review identifier started.");

        // Search by PHN
        cy.get("[data-testid=query-type-select]").click({ force: true });
        cy.get("[data-testid=query-type]")
            .contains("PHN")
            .click({ force: true });
        cy.get("[data-testid=query-input]").clear().type(phn);
        cy.get("[data-testid=search-btn]").click();
        cy.get("[data-testid=banner-feedback-warning-message]").should(
            "be.visible"
        );

        cy.log("Verify Phn user subject of review identifier finished.");
    });
});
