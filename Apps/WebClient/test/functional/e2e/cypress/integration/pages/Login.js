describe('Login Page', () => {
    beforeEach(() => {
        cy.visit('/login')
    })

    it('Validate URL', () => {
        cy.url().should('contains', 'https://dev.healthgateway.gov.bc.ca/login');
    })

    it('Greeting', () => {
        cy.contains('h3', 'Log In')
    })

    it('Menu Login', () => {
        cy.get('#menuBtnLogin')
                    .should('be.visible')
                    .should('have.attr', 'href', '/login')
                    .should('have.text', ' Login ')
    })

    it('Not Registered', () => {
    // Validates that the Not Registered text and link are available
    // it('Not Registered', () => {
    //     cy.get('.card-footer')
    //       .find('a')
    //                 //.contains('Not yet registered')
    //                 .should('be.visible')
    //                 .should('have.attr', 'href', '/RegistrationInfo')
    //                 .should('have.text', 'Sign Up')
    // })       
    })
})
