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
        cy.get("[data-testid=timelineCard]").then(($cards) => {
            cy.get("[data-testid=timeline-record-count]").contains(
                `Displaying ${$cards.length} out of `
            );
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
