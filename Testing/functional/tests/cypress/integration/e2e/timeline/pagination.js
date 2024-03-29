const { AuthMethod } = require("../../../support/constants");

describe("Pagination", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.checkTimelineHasLoaded();
    });

    it("Count Records", () => {
        cy.get("[data-testid=timelineCard]").then(($cards) => {
            cy.log(`Cards: ${$cards.length}`);
            cy.get("[data-testid=timeline-record-count]").contains(
                `Displaying 1 to ${$cards.length} out of `
            );
        });
    });

    it("Validating Navigation", () => {
        cy.get("[data-testid=entryCardDate]")
            .eq(3)
            .then((firstPageDateElement) => {
                cy.get("[data-testid=pagination]")
                    .find("[data-icon=chevron-right]")
                    .click({ force: true });

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
                cy.get("[data-testid=pagination]")
                    .find("[data-icon=chevron-left]")
                    .click({ force: true });

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
