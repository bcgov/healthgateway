const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkMenuButtonSelector = "[data-testid=card-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";
const vaccineCardQuickLinkCardSelector = "[data-testid=bc-vaccine-card-card]";
const vaccineCardAddQuickLinkChipSelector =
    "[data-testid=bc-vaccine-card-filter]";

describe("Vaccine Card Quick Link", () => {
    it("Remove and Add Vaccine Card Quick Link", () => {
        cy.configureSettings({});

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
                .should("be.visible", "be.enabled")
                .click();
        });
        cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();

        cy.log("Verifying vaccine card quick link no longer exists");
        cy.get(vaccineCardQuickLinkCardSelector).should("not.exist");

        cy.log("Adding vaccine card quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(vaccineCardAddQuickLinkChipSelector).should("exist").click();
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        cy.log("Verifying vaccine quick link card is present");
        cy.get(vaccineCardQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});
