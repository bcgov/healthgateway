describe('Error Pages', () => {
    beforeEach(() => {
    })

    it('HTTP 401', () => {
        cy.visit('/unauthorized')
        cy.contains('h1', '401')
        cy.contains('h2', 'Unauthorized')
        cy.contains('You do not have permission to view this page.')
    })

    it('HTTP 404', () => {
        cy.visit('/nonexistentpage')
        cy.contains('h1', '404')
        cy.contains('h2', 'Page not Found')
        cy.contains('The page you where looking for does not exist.')
    })
})