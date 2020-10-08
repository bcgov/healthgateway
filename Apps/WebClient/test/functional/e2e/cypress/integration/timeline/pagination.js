require('cypress-xpath')
const { AuthMethod } = require("../../support/constants")

describe('Pagination', () => {
    before(() => {
        cy.server();
        cy.fixture('AllDisabledConfig').then(config => {
            config.webClient.modules.Medication = true;
            cy.route('GET', '/v1/api/configuration/', config);            
        });
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

    it('Go to Next Page', () => {
        cy.get('[data-testid=dateGroup]')
            .first()
            .then((firstPageDateElement) => {
                cy.get('[data-testid=pagination]')
                    .contains("Next")
                    .click();

                cy.get('[data-testid=dateGroup]')
                    .first()
                    .then(secondPageDateElement => {
                        const firstDate = new Date(firstPageDateElement.text());
                        const secondDate = new Date(secondPageDateElement.text());
                        expect(firstDate).to.be.greaterThan(secondDate);
                    })
            });
    })

    it('Go to Previous Page', () => {
        cy.get('[data-testid=dateGroup]')
            .first()
            .then((secondPageDateElement) => {
                cy.get('[data-testid=pagination]')
                    .contains("Prev")
                    .click();

                cy.get('[data-testid=dateGroup]')
                    .first()
                    .then(firstPageDateElement => {
                        const firstDate = new Date(firstPageDateElement.text());
                        const secondDate = new Date(secondPageDateElement.text());
                        expect(firstDate).to.be.greaterThan(secondDate);
                    })
            });
    })
})