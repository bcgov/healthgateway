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

    it('Offline', () => {
        cy.get('[data-testid=offlineMessage]').should('not.exist');
        cy.readConfig().as("config").then(config => {
            config.webClient['offlineMode'] = { startDateTime: "2021-01-17T12:00:00", endDateTime: "2121-01-21T12:00:00", message: "customized offline message", whitelist: []};
            cy.server();
            cy.route('GET', '/v1/api/configuration/', config);
        })
        cy.visit('/');
        cy.get('[data-testid=offlineMessage]').contains('customized offline message');
        cy.get('#btnLogin').should('not.exist');
        cy.get('#menuBtnLogin').should('not.exist');
        cy.get('footer > .navbar').should('not.visible');
    })
})
