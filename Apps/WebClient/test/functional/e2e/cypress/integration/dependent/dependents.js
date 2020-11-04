const { AuthMethod } = require("../../support/constants")


describe('dependents', () => {
    const firstName = "Sam"
    const lastName = "Testfive"
    const doB = "2014-03-15"
    const phn = "9874307168"    
    const maskedPHN = "987438****"
    const gendeMale = "Male"
    const gendeFemale = "Female"
    const gendeUnknown = "Unknown"

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
        // Validate Gender Input  
        cy.get('[data-testid=genderInput]')
            .should('be.enabled')
            .select('').contains('Please select an option')
        cy.get('[data-testid=genderInput]')
            .select(gendeMale).should('have.value', gendeMale)
        cy.get('[data-testid=genderInput]')
            .select(gendeFemale).should('have.value', gendeFemale)
        cy.get('[data-testid=genderInput]')
            .select(gendeUnknown).should('have.value', 'NotSpecified')
        // Validate Cancel out of the form
        cy.get('[data-testid=cancelRegistrationBtn]')
            .should('be.enabled', 'be.visible')
            .click()
        // Validate the modal is done
        cy.get('[data-testid=newDependentModal]').should('not.exist')
    })
    
    it('Validate Add', () => {
        cy.get('[data-testid=addNewDependentBtn]')
            .click();
        
        cy.get('[data-testid=newDependentModalText]').should('exist', 'be.visible')
        
        cy.get('[data-testid=firstNameInput]')
            .type(firstName);
        cy.get('[data-testid=lastNameInput]')
            .type(lastName);
        cy.get('[data-testid=dateOfBirthInput]')
            .type(doB);
        cy.get('[data-testid=phnInput]')
            .type(phn);
        const genderInput = cy.get('[data-testid=genderInput]')
        genderInput.select(gendeFemale);
        cy.get('[data-testid=termsCheckbox]')
            .click({ force: true });

        cy.get('[data-testid=registerDependentBtn]').click(); 

        // Validate the modal is done 
        cy.get('[data-testid=newDependentModal]').should('not.exist')
    });

    it('Validate Dependent Tab', () => {
        // Validate the newly added dependent tab and elements are present        
        cy.get('[data-testid=dependentPHN]')
            .last().invoke('val')
            .then(phnNumber => expect(phnNumber).to.equal(maskedPHN));
        cy.get('[data-testid=dependentDOB]')
            .last().invoke('val')
            .then(dateOfBirth => expect(dateOfBirth).to.equal(doB));
        cy.get('[data-testid=dependentGender]')
            .last().invoke('val')
            .then(dateOfBirth => expect(dateOfBirth).to.equal(gendeFemale));        
    })

    it('Validate Covid Tab', () => {
        // Validate the tab and elements are present        
        cy.get('[data-testid=covid19TabTitle]').last().parent().click();
        cy.get('[data-testid=covid19NoRecords]').last().should('have.text', 'No records found.');
    })
}) 