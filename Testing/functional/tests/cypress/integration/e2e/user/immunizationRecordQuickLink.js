const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkMenuButtonSelector = "[data-testid=quick-link-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";
const immunizationRecordQuickLinkCardSelector =
    "[data-testid=immunization-record-card]";
const immunizationRecordFilterSelector =
    "[data-testid=immunization-record-filter]";

describe("Immunization Record Quick Link", () => {
    it("Remove and Add Immunization Record Quick Link", () => {
        cy.enableModules([]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Verifying immunization record card is present");
        cy.get(immunizationRecordQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing immunization record card");
        cy.get(immunizationRecordQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });

        cy.log("Verifying immunization record card no longer exists");
        cy.get(immunizationRecordQuickLinkCardSelector).should("not.exist");

        cy.log("Adding immunization record card");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(immunizationRecordFilterSelector)
            .should("exist")
            .check({ force: true });
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("not.be.enabled");

        cy.log("Verifying immunization record card is present");
        cy.get(immunizationRecordQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});
