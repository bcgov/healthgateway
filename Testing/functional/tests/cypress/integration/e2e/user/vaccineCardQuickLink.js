const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkMenuButtonSelector = "[data-testid=quick-link-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";
const vaccineCardQuickLinkCardSelector = "[data-testid=bc-vaccine-card-card]";
const vaccineCardAddQuickLinkCheckboxSelector =
    "[data-testid=bc-vaccine-card-filter]";

describe("Vaccine Card Quick Link", () => {
    it("Remove and Add Vaccine Card Quick Link", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Verifying vaccine quick link card is present");
        cy.get(vaccineCardQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing vaccine card quick link");
        cy.get(vaccineCardQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });

        cy.log("Verifying vaccine card quick link no longer exists");
        cy.get(vaccineCardQuickLinkCardSelector).should("not.exist");

        cy.log("Adding vaccine card quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(vaccineCardAddQuickLinkCheckboxSelector)
            .should("exist")
            .check({ force: true });
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying vaccine quick link card is present");
        cy.get(vaccineCardQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });

    it("Vaccine Card quick link not showing when module is disabled", () => {
        cy.enableModules(["Encounter"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Verifying vaccine card quicklink not present");
        cy.get(vaccineCardQuickLinkCardSelector).should("not.exist");

        cy.log("Verifying add vaccine card checkbox not present");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(vaccineCardAddQuickLinkCheckboxSelector).should("not.exist");
    });
});
