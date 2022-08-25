import { AuthMethod } from "../../../support/constants";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkCheckboxSelector =
    "[data-testid=quick-link-modal-text] input[type=checkbox]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const tooManyRequestsStatusCode = 429;
const serverErrorStatusCode = 500;

function testGetConfigurationError(statusCode = serverErrorStatusCode) {
    cy.intercept("GET", "/configuration", {
        statusCode,
    });
    cy.visit("/");

    if (statusCode === tooManyRequestsStatusCode) {
        cy.get("[data-testid=app-warning]").should("be.visible");
    } else {
        cy.get("[data-testid=app-error]").should("be.visible");
    }
}

function testGetProfileErrorOnLoad(statusCode = serverErrorStatusCode) {
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        "/home"
    );
    cy.intercept("GET", "**/UserProfile/*", {
        statusCode,
    });
    cy.reload();

    if (statusCode === tooManyRequestsStatusCode) {
        cy.get("[data-testid=app-warning]").should("be.visible");
    } else {
        cy.get("[data-testid=app-error]").should("be.visible");
    }
}

function testRegisterError(statusCode = serverErrorStatusCode) {
    const hdid = "S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA";
    cy.intercept("GET", `**/UserProfile/${hdid}`, {
        fixture: "UserProfileService/userProfileUnregistered.json",
    });
    cy.intercept("GET", `**/UserProfile/${hdid}/Validate`, {
        body: {
            resourcePayload: true,
            totalResultCount: 1,
            pageIndex: 0,
            pageSize: 1,
            resultStatus: 1, // success
        },
    });
    cy.intercept("POST", `**/UserProfile/${hdid}`, {
        statusCode,
    });
    cy.login(
        Cypress.env("keycloak.unregistered.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak
    );

    cy.location("pathname").should("eq", "/registration");
    cy.get("[data-testid=emailCheckbox]")
        .should("be.enabled")
        .check({ force: true });
    cy.get("[data-testid=emailInput]")
        .should("be.visible", "be.enabled")
        .type(Cypress.env("emailAddress"));
    cy.get("[data-testid=emailConfirmationInput]")
        .should("be.visible", "be.enabled")
        .type(Cypress.env("emailAddress"));
    cy.get("[data-testid=smsNumberInput]")
        .should("be.visible", "be.enabled")
        .type(Cypress.env("phoneNumber"));
    cy.get("[data-testid=acceptCheckbox]")
        .should("be.enabled")
        .check({ force: true });
    cy.get("[data-testid=registerButton]")
        .should("be.visible", "be.enabled")
        .click();

    if (statusCode === tooManyRequestsStatusCode) {
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    } else {
        cy.get("[data-testid=errorBanner]").should("be.visible");
    }
}

function testValidateEmailError(statusCode = serverErrorStatusCode) {
    cy.enableModules([]);
    cy.intercept("GET", "**/UserProfile/*/email/validate/dummyinvitekey", {
        statusCode,
    }).as("validateEmail");
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        "/validateEmail/dummyinvitekey"
    );

    if (statusCode === tooManyRequestsStatusCode) {
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    } else {
        cy.get("[data-testid=errorBanner]").should("be.visible");
    }
}

function testAddQuickLinkError(statusCode = serverErrorStatusCode) {
    cy.intercept("PUT", "**/UserProfile/*/preference", {
        statusCode,
    });
    cy.intercept("POST", "**/UserProfile/*/preference", {
        statusCode,
    });
    cy.login(
        Cypress.env("keycloak.username"),
        Cypress.env("keycloak.password"),
        AuthMethod.KeyCloak,
        "/home"
    );

    cy.get(addQuickLinkButtonSelector)
        .should("be.visible")
        .should("be.enabled")
        .click();
    cy.get(`${addQuickLinkCheckboxSelector}[value=Laboratory]`)
        .should("exist")
        .check({ force: true });
    cy.get(addQuickLinkSubmitButtonSelector)
        .should("be.visible")
        .should("be.enabled")
        .click()
        .should("be.disabled");

    if (statusCode === tooManyRequestsStatusCode) {
        cy.get("[data-testid=too-many-requests-error]").should("be.visible");
    } else {
        cy.get("[data-testid=quick-link-modal-error]").should("be.visible");
    }
}

describe("Error Alerts", () => {
    it("Error Retrieving Configuration", () => {
        testGetConfigurationError();
    });

    it("Error Retrieving Profile on Load", () => {
        testGetProfileErrorOnLoad();
    });

    it("Error Registering", () => {
        testRegisterError();
    });

    it("Error Validating Email", () => {
        testValidateEmailError();
    });

    it("Error Adding Quick Link", () => {
        testAddQuickLinkError();
    });
});

describe("429 Alerts", () => {
    it("429 Error Retrieving Configuration", () => {
        testGetConfigurationError(429);
    });

    it("429 Error Retrieving Profile on Load", () => {
        testGetProfileErrorOnLoad(429);
    });

    it("429 Error Registering", () => {
        testRegisterError(429);
    });

    it("429 Error Validating Email", () => {
        testValidateEmailError(429);
    });

    it("429 Error Adding Quick Link", () => {
        testAddQuickLinkError(429);
    });
});
