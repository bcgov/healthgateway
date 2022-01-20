require("cypress-xpath");
const { AuthMethod } = require("../../../support/constants");

describe("Pagination", () => {
    beforeEach(() => {
        cy.enableModules(["Medication", "Note"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Count Records", () => {
        // xpath is an additional library and we should probably not use much but this should
        // help in migrating over Selenium Tests
        cy.xpath('//*[contains(@class, "timelineCard")]').then((elements) => {
            cy.get("#listControls")
                .find(".col")
                .contains(`Displaying ${elements.length} out of `);
        });
    });

    it("Validating Navigation", () => {
        cy.get("[data-testid=entryCardDate]")
            .eq(3)
            .then((firstPageDateElement) => {
                cy.get("[data-testid=pagination]").contains("Next").click();

                cy.get("[data-testid=entryCardDate]")
                    .first()
                    .then((secondPageDateElement) => {
                        const firstDate = new Date(firstPageDateElement.text());
                        const secondDate = new Date(
                            secondPageDateElement.text()
                        );
                        expect(firstDate).to.be.greaterThan(secondDate);
                    });
            });

        cy.get("[data-testid=entryCardDate]")
            .eq(3)
            .then((secondPageDateElement) => {
                cy.get("[data-testid=pagination]").contains("Prev").click();

                cy.get("[data-testid=entryCardDate]")
                    .eq(3)
                    .then((firstPageDateElement) => {
                        const firstDate = new Date(firstPageDateElement.text());
                        const secondDate = new Date(
                            secondPageDateElement.text()
                        );
                        expect(firstDate).to.be.greaterThan(secondDate);
                    });
            });
    });
});
