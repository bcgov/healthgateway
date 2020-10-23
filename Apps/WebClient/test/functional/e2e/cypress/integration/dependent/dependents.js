const { AuthMethod } = require("../../support/constants")

describe('dependents', () => {
    before(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = false
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = false
            config.webClient.modules.Medication = false
            config.webClient.modules.MedicationHistory = false
            config.webClient.modules.Note = false
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
            cy.login(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'), AuthMethod.KeyCloak, "/dependents");
        })
    })


    it('Validate Disqualidfied Birthdate and Cancel Button', () => {
        cy.get('[data-testid=addNewDependentBtn]')
        .click();
        cy.get('[data-testid=firstNameInput]')
        .type('John');
        cy.get('[data-testid=lastNameInput]')
        .type('Tester');
        cy.get('[data-testid=dateOfBirthInput]')
        .type('1988-01-21');
        cy.get('[data-testid=phnInput]')
            .type('9874307215');
        cy.get('[data-testid=dateOfBirthInput]')
        .clear()
        .type('2005-01-01');
    
        cy.get('[data-testid=cancelRegistrationBtn]')
            .click();
    });

    it('Validate Add', () => {
        cy.get('[data-testid=addNewDependentBtn]')
            .click();
        cy.get('[data-testid=firstNameInput]')
            .type('John');
        cy.get('[data-testid=lastNameInput]')
            .type('Tester');
        cy.get('[data-testid=dateOfBirthInput]')
            .type('2005-01-01');
        cy.get('[data-testid=phnInput]')
            .type('9874307215');
        cy.get('[data-testid=genderInput]').select('Male');
        cy.get('[data-testid=termsCheckbox]')
            .click({ force: true });
            cy.get('[data-testid=registerDependentBtn]')
            .click();
         
            
    });


})