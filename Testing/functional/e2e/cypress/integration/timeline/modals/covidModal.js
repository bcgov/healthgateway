const { AuthMethod } = require("../../../support/constants");

const BASEURL = "/v1/api/UserProfile/";
const HDID = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
let accessToken = {};
let userProfile = {};

function enableCovidModal(actionedCovidModalAt) {
    if (actionedCovidModalAt != undefined) {
        actionedCovidModalAt.value = "2019-12-02T15:48:13.981-08:00";
        cy.request({
            method: "PUT",
            url: `${BASEURL}${HDID}/preference`,
            auth: {
                bearer: accessToken,
            },
            headers: {
                accept: "application/json",
                "Content-Type": "application/json",
            },
            body: JSON.stringify(actionedCovidModalAt),
        }).should((response) => {
            cy.log(`response.body: ${JSON.stringify(response.body)}`);
        });
    }
}

describe("Validate Modals Popup", () => {
    beforeEach(() => {
        cy.getTokens(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password")
        ).as("tokens");
        cy.get("@tokens").then((tokens) => {
            cy.log("Tokens", tokens);
            accessToken = tokens.access_token;
            cy.request({
                url: `${BASEURL}${HDID}`,
                followRedirect: false,
                auth: {
                    bearer: accessToken,
                },
                headers: {
                    accept: "application/json",
                },
            }).should((response) => {
                userProfile = response.body.resourcePayload;
                cy.log(`response.body: ${JSON.stringify(response.body)}`);
                enableCovidModal(userProfile.preferences.actionedCovidModalAt);
            });
        });
    });

    it("Covid Modal", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=covidModal]").contains("COVID-19");
        cy.get("[data-testid=covidModalText]").contains(
            "Check the status of your COVID-19 test and view your result when it is available"
        );
        cy.get("[data-testid=covidViewResultBtn]")
            .should("be.visible")
            .contains("View Result");
        cy.reload();
        cy.get("[data-testid=covidModal]").contains("COVID-19");
        cy.get("[data-testid=covidModalText]").contains(
            "Check the status of your COVID-19 test and view your result when it is available"
        );
        cy.get("[data-testid=covidViewResultBtn]")
            .should("be.visible")
            .contains("View Result")
            .click();
        cy.get("[data-testid=covidModal]").should("not.exist");

        // Verify that only COVID-19 Tests filter is selected.
        cy.get("[data-testid=encounterTitle]").should("not.exist");
        cy.get("[data-testid=noteTitle]").should("not.exist");
        cy.get("[data-testid=immunizationTitle]").should("not.exist");
        cy.get("[data-testid=laboratoryTitle]").should("be.visible");
        cy.get("[data-testid=medicationTitle]").should("not.exist");
        cy.get("[data-testid=filterDropdown] > span").contains("1");
    });

    it("Dismiss Covid Modal", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.get("[data-testid=covidModal] header:first")
            .find("button")
            .should("have.text", "×")
            .click();
        cy.get("[data-testid=covidModal]").should("not.exist");

        // Verify that only COVID-19 Tests filter is NOT selected.
        cy.get("[data-testid=filterDropdown] > span").contains("0");

        // Verify that Covid Modal doens't display after reload the timeline.
        cy.reload();
        cy.get("[data-testid=covidModal]").should("not.exist");
        cy.get("[data-testid=timelineLabel]").should("be.visible");
    });
});
