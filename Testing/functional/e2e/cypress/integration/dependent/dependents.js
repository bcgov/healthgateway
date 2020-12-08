const { AuthMethod } = require("../../support/constants")

describe('dependents', () => {
    const firstName = "Sam"
    const lastName = "Testfive"
    const doB = "2014-03-15"
    const invalidDoB = "2007-08-05"
    const testDate = "2020-03-21"
    const phn = "9874307168"    
   

    before(() => {
        cy.readConfig().as("config").then(config => {
            config.webClient.modules.CovidLabResults = true
            config.webClient.modules.Comment = false
            config.webClient.modules.Encounter = false
            config.webClient.modules.Immunization = false
            config.webClient.modules.Laboratory = true
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
        //Validate Date of Birth
        cy.get('[data-testid=dateOfBirthInput]')
            .should('be.enabled')
        // Validate PHN input
        cy.get('[data-testid=phnInput]')
            .should('be.enabled')
            .clear()
            .blur()
            .should('have.class', 'is-invalid')

        // Validate tesDate Input
        cy.get('[data-testid=testDateInput]')
            .should('be.enabled')

        // Validate Cancel out of the form
        cy.get('[data-testid=cancelRegistrationBtn]')
            .should('be.enabled', 'be.visible')
            .click()
        // Validate the modal is done
        cy.get('[data-testid=newDependentModal]').should('not.exist')
    })

    it('Validate Maximum Age Check', () => {
        // Validate that adding a dependent fails when they are over the age of 12
        cy.get('[data-testid=addNewDependentBtn]')
            .click();
        cy.get('[data-testid=newDependentModalText]').should('exist', 'be.visible')
        cy.get('[data-testid=firstNameInput]')
            .type(firstName);
        cy.get('[data-testid=lastNameInput]')
            .type(lastName);
        cy.get('[data-testid=dateOfBirthInput]')
            .type(invalidDoB);
        cy.get('[data-testid=testDateInput]')
            .type(testDate);
        cy.get('[data-testid=phnInput]')
            .type(phn);
        cy.get('[data-testid=termsCheckbox]')
            .check({ force: true });

        cy.get('[data-testid=registerDependentBtn]').click(); 
        
        // Validate the modal has not closed
        cy.get('[data-testid=newDependentModal]').should('exist')

        cy.get('[data-testid=cancelRegistrationBtn]').click(); 
    })
    
    it('Validate Add', () => {
        cy.get('[data-testid=addNewDependentBtn]')
            .click();
        
        cy.get('[data-testid=newDependentModalText]').should('exist', 'be.visible')
        
        cy.get('[data-testid=firstNameInput]')
            .clear()
            .type(firstName);
        cy.get('[data-testid=lastNameInput]')
            .clear()
            .type(lastName);
        cy.get('[data-testid=dateOfBirthInput]')
            .clear()
            .type(doB);
        cy.get('[data-testid=testDateInput]')
            .clear()
            .type(testDate);
        cy.get('[data-testid=phnInput]')
            .clear()
            .type(phn);
        cy.get('[data-testid=termsCheckbox]')
            .check({ force: true });

        cy.get('[data-testid=registerDependentBtn]').click(); 

        // Validate the modal is done 
        cy.get('[data-testid=newDependentModal]').should('not.exist')
    });

    it('Validate Dependent Tab', () => {
        // Validate the newly added dependent tab and elements are present   
        cy.get('[data-testid=dependentName]')
            .contains(firstName.toUpperCase())
            .contains(lastName.toUpperCase())  
        cy.get('[data-testid=dependentPHN]')
            .last().invoke('val')
            .then(phnNumber => expect(phnNumber).to.equal(phn));
        cy.get('[data-testid=dependentDOB]')
            .last().invoke('val')
            .then(dateOfBirth => expect(dateOfBirth).to.equal(doB));          
    })


    it('Validate Covid Tab with Results', () => {
        let sensitiveDocMessage = ' The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off. ';
        // Validate the tab and elements are present        
        cy.get('[data-testid=covid19TabTitle]')
            .last()
            .parent()
            .click();
        cy.get('[data-testid=dependentCovidTestDate]')
            .first()
            .contains(/\d{4}-\d{2}-\d{2}/);
        cy.get('[data-testid=dependentCovidReportDownloadBtn]')
            .first()
            .click();
        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');
        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocMessage);
        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');

        cy.get('[data-testid=covid19TabTitle]')
            .last()
            .parent()
            .click();
        cy.get('[data-testid=dependentCovidTestDate]')
            .last()
            .contains(/\d{4}-\d{2}-\d{2}/);
        cy.get('[data-testid=dependentCovidTestLocation]')
            .last()
            .should('have.text', ' Fha ');
        cy.get('[data-testid=dependentCovidTestLabResult]')
            .last()
            .should('have.text', ' NotSet ');
        cy.get('[data-testid=dependentCovidReportDownloadBtn]')
            .last()
            .click();
        cy.get('[data-testid=genericMessageModal]')
            .should('be.visible');
        cy.get('[data-testid=genericMessageText]')
            .should('have.text', sensitiveDocMessage);
        cy.get('[data-testid=genericMessageSubmitBtn]')
            .click();            
        cy.get('[data-testid=genericMessageModal]')
            .should('not.exist');
    })
    

    it('Validate Remove Dependent', () => {       
        cy.get('[data-testid=dependentMenuBtn]').last().click();
        cy.get('[data-testid=deleteDependentMenuBtn]').last().click();
        cy.get('[data-testid=confirmDeleteBtn]').should('be.visible');
        cy.get('[data-testid=cancelDeleteBtn]').should('be.visible');
        cy.get('[data-testid=cancelDeleteBtn]').click();
        
        // Now click the "Yes, I'm sure" to confirm deletion
        cy.get('[data-testid=dependentMenuBtn]').last().click();
        cy.get('[data-testid=deleteDependentMenuBtn]').last().click();
        cy.get('[data-testid=confirmDeleteBtn]').click();
    })
}) 