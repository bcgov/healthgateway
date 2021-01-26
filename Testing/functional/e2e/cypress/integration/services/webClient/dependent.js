describe('WebClient Dependent Service', () => {
    const BASEURL = "/v1/api/UserProfile/"
    const HDID='P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A'
    const BOGUSHDID='BOGUSHDID'
    beforeEach(() => {
        cy.getTokens(Cypress.env('keycloak.username'), Cypress.env('keycloak.password'))
            .as("tokens")
    })

    it('Verify Get Dependents Unauthorized', () => {
        cy.request({ 
            url: `${BASEURL}${HDID}/Dependent`,
            followRedirect: false,
            failOnStatusCode: false
        })
        .should((response) => { expect(response.status).to.eq(401) })
    })    

    it('Verify Get Dependents Forbidden', () => {
        cy.get("@tokens").then(tokens => {
            cy.log('Tokens', tokens)
            cy.request({
                url: `${BASEURL}${BOGUSHDID}/Dependent`,
                followRedirect: false,
                failOnStatusCode: false,
                auth: {
                    bearer: tokens.access_token
                },
                headers: {
                    accept: 'application/json'
                }
            })
            .should((response) => { expect(response.status).to.eq(403) })       
        })
    }) 

    it('Verify Get Dependents Authorized', () => {
        cy.get("@tokens").then(tokens => {
            cy.log('Tokens', tokens)
            cy.request({
                url: `${BASEURL}${HDID}/Dependent`,
                followRedirect: false,
                auth: {
                    bearer: tokens.access_token
                },
                headers: {
                    accept: 'application/json'
                }
            })
            .should((response) => { 
                expect(response.status).to.eq(200)
                expect(response.body).to.not.be.null
            })          
        }) 
    })

    it('Verify Post Dependent Unauthorized', () => {
        cy.request({ 
            method: 'POST',
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false
        })
        .should((response) => { expect(response.status).to.eq(401) })
    })    

    it('Verify Post Dependent Forbidden', () => {
        cy.get("@tokens").then(tokens => {
            cy.log('Tokens', tokens)
            cy.request({
                method: 'POST',
                url: `${BASEURL}${BOGUSHDID}`,
                followRedirect: false,
                failOnStatusCode: false,
                auth: {
                    bearer: tokens.access_token
                },
                headers: {
                    accept: 'application/json'
                }
            })
            .should((response) => { expect(response.status).to.eq(403) })       
        })
    })

    it('Verify Put Dependent Not Found', () => {
        cy.request({ 
            method: 'PUT',
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false
        })
        .should((response) => { expect(response.status).to.eq(404) })
    })    

    it('Verify Delete Dependent Unauthorized', () => {
        cy.request({ 
            method: 'DELETE',
            url: `${BASEURL}${HDID}`,
            followRedirect: false,
            failOnStatusCode: false
        })
        .should((response) => { expect(response.status).to.eq(401) })
    })

    it('Verify Delete Dependent Forbidden', () => {
        cy.get("@tokens").then(tokens => {
            cy.log('Tokens', tokens)
            cy.request({
                method: 'DELETE',
                url: `${BASEURL}${BOGUSHDID}`,
                followRedirect: false,
                failOnStatusCode: false,
                auth: {
                    bearer: tokens.access_token
                },
                headers: {
                    accept: 'application/json'
                }
            })
            .should((response) => { expect(response.status).to.eq(403) })       
        })
    })
})