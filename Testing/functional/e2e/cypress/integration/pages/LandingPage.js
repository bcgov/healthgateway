describe('Landing Page', () => {
    beforeEach(() => {
        cy.visit('/')
    })

    it('Title', () => {
        cy.title().should('eq', 'Health Gateway')
    })

    it('Greeting', () => {
        cy.contains('h1', 'Health Gateway')
    })

    it('Sign Up Button', () => {
        cy.get('#btnStart').should('be.visible')
                           .should('have.attr', 'href', '/registration')
                           .should('have.text', 'Register')
    })

    it('Login Button', () => {
        cy.get('#btnLogin').should('be.visible')
                           .should('have.attr', 'href', '/login')
                           .should('have.text', 'Log in')
    })
})
