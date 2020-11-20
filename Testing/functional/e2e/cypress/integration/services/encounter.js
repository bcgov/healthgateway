const { AuthMethod } = require("../../support/constants")

describe('Encounter Service', () => {
    beforeEach(() => {
        cy.readConfig().as("config")
        cy.getTokens(Cypress.env('keycloak.username'), Cypress.env('keycloak.password')).as("tokens")
    })

    it('Verify Swagger', () => {
        cy.get("@config").then(config => {
            cy.log(`Verifying Swagger exists for Encounter at Endpoint: ${config.serviceEndpoints.Encounter}swagger`)
            cy.visit(`${config.serviceEndpoints.Encounter}swagger`)
            .contains('Health Gateway Encounter Services documentation')
        })
    })

    it('Verify Encounter Unauthorized', () => {
        cy.get("@config").then(config => {
            cy.log(`Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`)
            cy.request({ 
            url: `${config.serviceEndpoints.Encounter}v1/api/Encounter/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A`,
            followRedirect: false,
            failOnStatusCode: false
            })
            .should((response) => { expect(response.status).to.eq(401) })
        })
    })    

    it('Verify Encounter Forbidden', () => {
        const HDID='BOGUSHDID'
        cy.get("@tokens").then(tokens => {
            cy.log('Tokens', tokens)
            cy.get("@config").then(config => {
            cy.log(`Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`)
            cy.request({
                url: `${config.serviceEndpoints.Encounter}v1/api/Encounter/${HDID}`,
                followRedirect: false,
                failOnStatusCode: false,
                auth: {
                bearer: tokens.access_token
                },
                headers: {
                'accept': 'application/json'
                }
            })
            .should((response) => { expect(response.status).to.eq(403) })       
            })
        })
    }) 

    it('Verify Encounter Authorized', () => {
        const HDID='P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A'
        cy.fixture('EncounterService/encounter.json').then((encounterResponse) => {
            cy.get("@tokens").then(tokens => {
                cy.log('Tokens', tokens)
                cy.get("@config").then(config => {
                cy.log(`Encounter Service Endpoint: ${config.serviceEndpoints.Encounter}`)
                cy.request({
                    url: `${config.serviceEndpoints.Encounter}v1/api/Encounter/${HDID}`,
                    followRedirect: false,
                    auth: {
                    bearer: tokens.access_token
                    },
                    headers: {
                    'accept': 'application/json'
                    }
                })
                .should((response) => { 
                    expect(response.status).to.eq(200)
                    expect(response.body).to.not.be.null
                    expect(response.body).to.deep.equal(encounterResponse)
                })          
                })
            })
        }) 
    })
})