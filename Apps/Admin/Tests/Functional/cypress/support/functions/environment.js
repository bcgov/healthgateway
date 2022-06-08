const { localDevUri } = require("../constants");

export function verifyTestingEnvironment() {
    if (Cypress.config().baseUrl !== localDevUri) {
        cy.log("Tests are running in a valid environment.");
    } else {
        cy.log("Tests are running locally which is not a valid environment.");
        Cypress.runner.stop();
    }
}
