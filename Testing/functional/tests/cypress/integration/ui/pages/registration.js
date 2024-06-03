import { AuthMethod } from "../../../support/constants";
import { setupStandardFixtures } from "../../../support/functions/intercept";

const registrationPath = "/registration";
const homePath = "/home";
const hdid = "S22BPV6WHS5TRLBL4XKGQDBVDUKLPIRSBGYSEJAHYMYRP22SP2TA";

describe("Registration Page", () => {
    it("Registering with unchecked email and notification", () => {
        setupStandardFixtures({
            patientHdid: hdid,
            userProfileHdid: hdid,
            patientFixture: "PatientService/patientUnregistered.json",
            userProfileFixture:
                "UserProfileService/userProfileUnregistered.json",
        });

        cy.intercept("GET", "**/UserProfile/termsofservice", {
            fixture: "UserProfileService/termsOfService.json",
        });

        cy.intercept("GET", `**/UserProfile/${hdid}/Validate`, {
            fixture: "UserProfileService/userProfileValidate.json",
        });

        cy.intercept("POST", `**/UserProfile/${hdid}`, {
            fixture:
                "UserProfileService/userProfileUnregisteredRegistered.json",
        });

        cy.configureSettings({});
        cy.login(
            Cypress.env("keycloak.unregistered.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.contains("#subject", "Registration").should("be.visible");
        cy.location("pathname").should("eq", registrationPath);

        cy.get("[data-testid=sidebar]").should("not.exist");
        cy.get("[data-testid=footer]").should("not.exist");

        cy.get("[data-testid=emailCheckbox] input")
            .uncheck()
            .should("not.be.checked");
        cy.get("[data-testid=emailInput]")
            .should("be.visible", "be.enabled")
            .should("have.value", "");
        cy.get("[data-testid=emailConfirmationInput]")
            .should("be.visible", "be.enabled")
            .should("have.value", "");
        cy.get("[data-testid=sms-checkbox] input")
            .uncheck()
            .should("not.be.checked");
        cy.get("[data-testid=smsNumberInput]")
            .should("be.visible", "be.enabled")
            .should("have.value", "");
        cy.get("[data-testid=acceptCheckbox] input")
            .should("be.enabled")
            .check();
        cy.get("[data-testid=registerButton]")
            .should("be.visible", "be.enabled")
            .click();
        cy.location("pathname").should("eq", homePath);
    });
});
