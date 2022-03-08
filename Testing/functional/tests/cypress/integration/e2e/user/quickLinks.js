const { AuthMethod } = require("../../../support/constants");

const laboratoryModule = "Laboratory";
const laboratoryTitle = "COVID‑19 Tests";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkCheckboxSelector =
    "[data-testid=quick-link-modal-text] input[type=checkbox]";
const addQuickLinkCancelButtonSelector =
    "[data-testid=cancel-add-quick-link-btn]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkCardSelector = "[data-testid=quick-link-card]";
const cardButtonTitleSelector = "[data-testid=card-button-title]";
const quickLinkMenuButtonSelector = "[data-testid=quick-link-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";

function getQuickLinkCheckbox(module) {
    return cy.get(`${addQuickLinkCheckboxSelector}[value=${module}]`);
}

function getQuickLinkCard(title) {
    return cy
        .contains(cardButtonTitleSelector, title)
        .parents(quickLinkCardSelector);
}

describe("Quick Links", () => {
    it("Add and Remove Quick Link", () => {
        cy.enableModules([laboratoryModule]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/home"
        );

        cy.log("Adding a quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        getQuickLinkCheckbox(laboratoryModule)
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

        cy.log("Verifying quick link card is present and links to timeline");
        getQuickLinkCard(laboratoryTitle)
            .should("be.visible", "be.enabled")
            .click();
        cy.checkTimelineHasLoaded();

        cy.log("Verifying filter is active");
        cy.get("[data-testid=filterDropdown] > span").contains(1);
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Laboratory-filter]").should("be.checked");
        cy.get("[data-testid=laboratoryTitle]").should("be.visible");

        cy.log("Returning to home page");
        cy.visit("/home");

        cy.log("Removing quick link");
        getQuickLinkCard(laboratoryTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });

        cy.log("Verifying quick link card no longer exists");
        cy.contains(cardButtonTitleSelector, laboratoryTitle).should(
            "not.exist"
        );

        cy.log("Verifying add quick link button has been re-enabled");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled");
    });
});
