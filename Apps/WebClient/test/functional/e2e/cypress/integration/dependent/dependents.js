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

    it('Validate Add, Fields and Cancel', () => {         
        //Validate Main Add Button  
        cy.get('[data-testid=addNewDependentBtn]')
        .should('be.enabled', 'be.visible')
        .click()
        //Validate Modal appears - note this should use a modal ID but we don't have one
        cy.get('[data-testid=newDependentModalText]').should('exist', 'be.visible')   
        //Validate First Name  
        cy.get('[data-testid=firstNameInput]')
            .should('be.enabled')
            .clear()
            .blur()    
            .should('have.class', 'is-invalid')
        // Validate Last Name  
        cy.get('[data-testid=lastNameInput]')
            .should('be.enabled')
            .clear()
            .blur()
            .should('have.class', 'is-invalid')
        // Mandad this is broken and needs to be fixed to validate
        //   cy.get('[data-testid=dateOfBirthInput]')
        //     .should('be.enabled')
        // Validate PHN input
        cy.get('[data-testid=phnInput]')
            .should('be.enabled')
            .clear()
            .blur()
            .should('have.class', 'is-invalid')
        // Validate Gender Input  
        cy.get('[data-testid=genderInput]')
            .should('be.enabled')
            .select('').contains('Please select an option')
        cy.get('[data-testid=genderInput]')
            .select('Male').should('have.value', 'Male')
        cy.get('[data-testid=genderInput]')
            .select('Female').should('have.value', 'Female')
        cy.get('[data-testid=genderInput]')
            .select('Unknown').should('have.value', 'NotSpecified')
        // Validate Cancel out of the form
        cy.get('[data-testid=cancelRegistrationBtn]')
            .should('be.enabled', 'be.visible')
            .click()
        // Validate the modal is done, again should use modal id    
        cy.get('[data-testid=newDependentModalText]').should('not.exist')
    })

    it('Validate Add', () => {
        cy.get('[data-testid=addNewDependentBtn]')
            .click();
        //Validate Modal appears - note this should use a modal ID but we don't have one
        cy.get('[data-testid=newDependentModalText]').should('exist', 'be.visible')      
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
        // Validate the modal is done, again should use modal id    
        cy.get('[data-testid=newDependentModalText]').should('not.exist')

        // You should now validate that the Tabs appear 
    });

    it('Validate Dependent Tab', () => {
        // Validate the newly added dependent tab and elements are present            
    })

    it('Validate Covid Tab', () => {
        // Validate the tab and elements are present
    })
})