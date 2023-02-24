const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const encounterModule = "Encounter";
const immunizationModule = "Immunization";
const laboratoryModule = "Laboratory";
const allLaboratoryModule = "AllLaboratory";
const medicationModule = "Medication";
const medicationRequestModule = "MedicationRequest";
const noteModule = "Note";

const encounterTitle = "Health Visits";
const immunizationTitle = "Immunizations";
const laboratoryTitle = "COVID‑19 Tests";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkCheckboxSelector =
    "[data-testid=quick-link-modal-text] input[type=checkbox]";
const addQuickLinkCancelButtonSelector =
    "[data-testid=cancel-add-quick-link-btn]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const addQuickLinkModalTextSelector = "[data-testid=quick-link-modal-text]";
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
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
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
        cy.contains("[data-testid=filter-label]", laboratoryTitle);
        cy.get("[data-testid=filterContainer]").should("not.exist");
        cy.get("[data-testid=filterDropdown]").click();
        cy.get("[data-testid=filterContainer]").should("be.visible");
        cy.get("[data-testid=Covid19TestResult-filter]").should("be.checked");
        cy.get("[data-testid=covid19testresultTitle]").should("be.visible");

        cy.log("Returning to home page");
        cy.get("[data-testid=menu-btn-home-link]").should("be.visible").click();

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

    it("Add and Remove Multiple Quick Links", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying 3 checkboxes exist");
        cy.get(addQuickLinkCheckboxSelector).should("have.length", 3);

        cy.log("Selecting encounters and immunization checkboxes");
        getQuickLinkCheckbox(encounterModule)
            .should("exist")
            .check({ force: true });
        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .check({ force: true });

        cy.log("Adding quick links");
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying quick link cards are present");
        getQuickLinkCard(encounterTitle).should("be.visible", "be.enabled");
        getQuickLinkCard(immunizationTitle).should("be.visible", "be.enabled");

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying 1 checkbox remains");
        cy.get(addQuickLinkCheckboxSelector).should("have.length", 1);
        getQuickLinkCheckbox(laboratoryModule)
            .should("exist")
            .should("not.be.checked");

        cy.log("Cancelling add quick link modal");
        cy.get(addQuickLinkCancelButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Removing quick links");
        getQuickLinkCard(encounterTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });
        getQuickLinkCard(immunizationTitle).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });

        cy.log("Verifying quick link cards no longer exists");
        cy.contains(cardButtonTitleSelector, encounterTitle).should(
            "not.exist"
        );
        cy.contains(cardButtonTitleSelector, immunizationTitle).should(
            "not.exist"
        );

        cy.log("Opening add quick link modal");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying 3 checkboxes exist");
        cy.get(addQuickLinkCheckboxSelector).should("have.length", 3);
    });
});

describe("Add Quick Link Modal", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "covid19TestResult",
                    enabled: true,
                },
                {
                    name: "healthVisit",
                    enabled: true,
                },
                {
                    name: "immunization",
                    enabled: true,
                },
                {
                    name: "labResult",
                    enabled: true,
                },
                {
                    name: "medication",
                    enabled: true,
                },
                {
                    name: "specialAuthorityRequest",
                    enabled: true,
                },
                {
                    name: "note",
                    enabled: true,
                },
            ],
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
    });

    it("Add Quick Link  - Cancel when all selected", () => {
        cy.get(addQuickLinkButtonSelector).click();
        cy.get(addQuickLinkModalTextSelector).should("exist");

        // Check off
        getQuickLinkCheckbox(encounterModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(laboratoryModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(allLaboratoryModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(medicationRequestModule)
            .should("exist")
            .check({ force: true });

        // Cancel
        cy.get(addQuickLinkCancelButtonSelector).click();
        cy.get(addQuickLinkButtonSelector).click();
        cy.get(addQuickLinkModalTextSelector).should("exist");

        // Verify
        getQuickLinkCheckbox(encounterModule)
            .should("exist")
            .should("not.be.checked");
        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .should("not.be.checked");
        getQuickLinkCheckbox(laboratoryModule)
            .should("exist")
            .should("not.be.checked");
        getQuickLinkCheckbox(allLaboratoryModule)
            .should("exist")
            .should("not.be.checked");
        getQuickLinkCheckbox(medicationRequestModule)
            .should("exist")
            .should("not.be.checked");
    });

    it("Add Quick Link - 2 selected and 1 un-selected", () => {
        cy.get(addQuickLinkButtonSelector).click();
        cy.get(addQuickLinkModalTextSelector).should("exist");

        // Check off
        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(laboratoryModule)
            .should("exist")
            .check({ force: true });

        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .uncheck({ force: true });

        // Verify
        getQuickLinkCheckbox(encounterModule)
            .should("exist")
            .and("not.be.checked");
        getQuickLinkCheckbox(immunizationModule)
            .should("exist")
            .and("not.be.checked");
        getQuickLinkCheckbox(laboratoryModule)
            .should("exist")
            .and("be.checked");
        getQuickLinkCheckbox(allLaboratoryModule)
            .should("exist")
            .and("not.be.checked");
        getQuickLinkCheckbox(medicationRequestModule)
            .should("exist")
            .and("not.be.checked");
    });
});
