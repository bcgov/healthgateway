const { AuthMethod } = require("../../../../support/constants");

const actionedCovidModalAt = "2019-12-02T15:48:13.981-08:00";
let interceptPreference = true;

describe("Validate Modals Popup", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/UserProfile/*", (req) => {
            req.reply((res) => {
                if (interceptPreference) {
                    res.body.resourcePayload.preferences.actionedCovidModalAt.value =
                        actionedCovidModalAt;
                }
            });
        });
        cy.enableModules("Laboratory");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Covid Modal", () => {
        cy.get("[data-testid=covidModal]").contains("COVID-19");
        cy.get("[data-testid=covidModalText]").contains(
            "Check the status of your COVID-19 test and view your result when it is available"
        );
        cy.get("[data-testid=covidViewResultBtn]")
            .should("be.visible")
            .contains("View Result");
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
        cy.get("[data-testid=covidModal] header:first")
            .find("button")
            .should("have.text", "×")
            .click();
        cy.get("[data-testid=covidModal]").should("not.exist");

        // Verify that only COVID-19 Tests filter is NOT selected.
        cy.get("[data-testid=filterDropdown] > span").contains("0");

        // Verify that Covid Modal doens't display after reload the timeline.
        interceptPreference = false;
        cy.reload();
        cy.checkTimelineHasLoaded();
        cy.get("[data-testid=covidModal]").should("not.exist");
        cy.get("[data-testid=timelineLabel]").should("be.visible");
    });
});
