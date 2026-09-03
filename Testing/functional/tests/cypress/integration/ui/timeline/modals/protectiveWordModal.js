import { AuthMethod } from "../../../../support/constants";
import { setupStandardFixtures } from "../../../../support/functions/intercept";

const medicationEndpoint = "**/MedicationStatement/*";
const protectedResponse = {
    resourcePayload: [],
    totalResultCount: 0,
    pageIndex: 0,
    pageSize: 0,
    resultStatus: 2,
    resultError: {
        actionCode: "PROTECTED",
        errorCode: "Medication-PROTECTED",
        resultMessage: "A protective word is required.",
    },
};

function setupProtectiveWordResponse() {
    cy.fixture("MedicationService/medicationStatement.json").then(
        (medications) => {
            cy.intercept("GET", medicationEndpoint, (request) => {
                if (request.headers.protectiveword === "KEYWORD") {
                    request.reply(medications);
                } else {
                    request.reply(protectedResponse);
                }
            });
        }
    );
}

function login() {
    cy.configureSettings({
        datasets: [{ name: "medication", enabled: true }],
    });
    setupStandardFixtures();
    setupProtectiveWordResponse();
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );
}

describe("Protective Word Modal", () => {
    it("Dismisses the modal", () => {
        login();
        cy.get("[data-testid=protectiveWordCloseButton]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
    });
});
