require('cypress-xpath')
const { AuthMethod } = require("../../support/constants")

describe('Pagination', () => {
    beforeEach(() => {
        cy.server()
        cy.fixture('medicationEnabledConfig').as('config')
        cy.route('GET', '/v1/api/configuration/', '@config')
        cy.login(Cypress.env('keycloak.username'), 
                Cypress.env('keycloak.password'), 
                AuthMethod.KeyCloak);
        cy.checkTimelineHasLoaded();
    })

    it('Count Records', () => {
        // xpath is an additional library and we should probably not use it much but this should
        // help in migrating over Selenium Tests
        cy.xpath('//*[contains(@class, "entryCard")]')
            .then(elements => {                
                cy.get('#listControls').find('.col').contains(`Displaying ${elements.length} out of `)
            });
    })

    it('Validate Pages', () => {
        cy.get('[data-testid=dateGroup]')
            .first()
            .then((firstPageDateElement) => {
                cy.get('[data-testid=pagination]')
                    .contains("Next")
                    .click();

                cy.get('[data-testid=dateGroup]')
                    .first()
                    .then(nextPageDateElement => {
                        const firstDate = new Date(firstPageDateElement.text());
                        const nextPageDate = new Date(nextPageDateElement.text());
                        expect(firstDate).to.be.greaterThan(nextPageDate);
                    })

                cy.get('[data-testid=pagination]')
                    .contains("Prev")
                    .click();

                cy.get('[data-testid=dateGroup]')
                    .first()
                    .then(previousPageDateElement => {
                        expect(firstPageDateElement.text()).to.be.equal(previousPageDateElement.text());
                    })
            });
    })
})