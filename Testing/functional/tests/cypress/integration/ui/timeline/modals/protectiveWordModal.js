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
    it("Validates and accepts the protective word", () => {
        login();

        cy.contains(
            "[data-testid=protectiveWordModal]",
            "Restricted PharmaNet Records"
        ).should("be.visible");
        cy.get("[data-testid=protectiveWordModalText]")
            .should("contain", "Please enter the protective word")
            .and("contain", "For more information visit");
        cy.get("[data-testid=protectiveWordModalRulesHREF]")
            .should("be.visible")
            .and(
                "have.attr",
                "href",
                "https://www2.gov.bc.ca/gov/content/health/health-drug-coverage/pharmacare-for-bc-residents/pharmanet/protective-word-for-a-pharmanet-record"
            );
        cy.get("[data-testid=protectiveWordContinueBtn]").should("be.disabled");

        cy.get("[data-testid=protectiveWordInput] input").type("WRONGKEYWORD");
        cy.get("[data-testid=protectiveWordContinueBtn]").click();
        cy.contains(
            "[data-testid=protectiveWordInput]",
            "Invalid protective word. Try again."
        ).should("be.visible");

        cy.get("[data-testid=protectiveWordInput] input")
            .clear()
            .type("KEYWORD");
        cy.get("[data-testid=protectiveWordContinueBtn]").click();
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
        cy.get("[data-testid=timelineCard]").should("exist");
    });

    it("Dismisses the modal", () => {
        login();
        cy.get("[data-testid=protectiveWordCloseButton]")
            .should("be.visible")
            .click();
        cy.get("[data-testid=protectiveWordModal]").should("not.exist");
    });
});
