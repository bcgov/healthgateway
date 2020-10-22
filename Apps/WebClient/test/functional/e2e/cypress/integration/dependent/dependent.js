const { AuthMethod } = require("../../support/constants")

describe('Notes', () => {
    before(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = false
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = true
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'),
                Cypress.env('keycloak.password'),
                AuthMethod.KeyCloak);
            cy.checkTimelineHasLoaded();
        })
    })

    it('Validate Add', () => {
        cy.get('[data-testid=addNoteBtn]')
            .click();
        cy.get('[data-testid=noteTitleInput]')
            .type('Note Title!');
        cy.get('[data-testid=noteDateInput]')
            .type('2050-01-01');
        cy.get('[data-testid=noteTextInput]')
            .type('Test');
        cy.get('[data-testid=saveNoteBtn]')
            .click();
        cy.get('[data-testid=noteTitle]')
            .first()
            .should('have.text', ' Note Title! ');
        cy.get('[data-testid=dateGroup]')
            .first()
            .should('have.text', ' Jan 1, 2050 ');
    });

    it('Validate Edit', () => {
        cy.get('[data-testid=noteMenuBtn]')
            .first()
            .click();
        cy.get('[data-testid=editNoteMenuBtn]')
            .first()
            .click();
        cy.get('[data-testid=noteTitleInput]')
            .clear()
            .type('Test Edit');
        cy.get('[data-testid=saveNoteBtn]')
            .click();
        cy.get('[data-testid=noteTitle]')
            .first()
            .should('have.text', ' Test Edit ');
    });

    it('Validate Delete', () => {
        cy.get('[data-testid=noteMenuBtn]')
            .click();
        cy.on('window:confirm', (str) => {
            expect(str).to.eq('Are you sure you want to delete this note?');
        });
        cy.get('[data-testid=deleteNoteMenuBtn]')
            .first()
            .click();
        cy.get('[data-testid=dateGroup]')
            .should('not.exist');
    })
})