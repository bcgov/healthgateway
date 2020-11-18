const { AuthMethod } = require("../../support/constants")

describe('User Feedback', () => {
    beforeEach(() => {
        cy.readConfig().as("config")
        cy.getTokens(Cypress.env('keycloak.username'), Cypress.env('keycloak.password')).as("tokens")
    })

    it('Verify Swagger', () => {
      cy.get("@config").then(config => {
        cy.log(`Verifying Swagger exists for Patient at Endpoint: ${config.serviceEndpoints.Patient}swagger`)
        cy.visit(`${config.serviceEndpoints.Patient}swagger`)
          .contains('Health Gateway Patient Services documentation')
      })
    })
    
    it('Verify Patient Unauthorized', () => {
      cy.get("@config").then(config => {
        cy.log(`Patient Service Endpoint: ${config.serviceEndpoints.Patient}`)
        cy.request({
          url: `${config.serviceEndpoints.Patient}v1/api/Patient/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A`,
          followRedirect: false,
          failOnStatusCode: false
        })
        .should((response) => { expect(response.status).to.eq(401) })
      })
    })    

    it('Verify Patient Forbidden', () => {
      const HDID='BOGUSHDID'
      cy.get("@tokens").then(tokens => {
        cy.log('Tokens', tokens)
        cy.get("@config").then(config => {
          cy.log(`Patient Service Endpoint: ${config.serviceEndpoints.Patient}`)
          cy.request({
            url: `${config.serviceEndpoints.Patient}v1/api/Patient/${HDID}`,
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

    it('Verify Patient Authorized', () => {
      cy.fixture('PatientService/patient.json').then((patientResponse) => {
      const HDID=patientResponse.resourcePayload.hdid
      cy.get("@tokens").then(tokens => {
        cy.log('Tokens', tokens)
        cy.get("@config").then(config => {
          cy.log(`Patient Service Endpoint: ${config.serviceEndpoints.Patient}`)
          cy.request({
            url: `${config.serviceEndpoints.Patient}v1/api/Patient/${HDID}`,
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
            expect(response.body).to.deep.equal(patientResponse)
          })          
        })
      })
    }) 
  })
})
