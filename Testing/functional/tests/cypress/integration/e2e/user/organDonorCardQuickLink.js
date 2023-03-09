const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkMenuButtonSelector = "[data-testid=quick-link-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";
const organDonorQuickLinkCardSelector =
    "[data-testid=organ-donor-registration-card]";
const organDonorAddQuickLinkCheckboxSelector =
    "[data-testid=organ-donor-registration-filter]";

describe("Organ Donor Quick Link", () => {
    it("Remove and Add Organ Donor Card Quick Link", () => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: true,
                    },
                ],
            },
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );

        cy.log("Verifying organ donor quick link card is present");
        cy.get(organDonorQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing organ donor quick link");
        cy.get(organDonorQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible")
                .parent("a")
                .should("be.visible", "be.enabled")
                .click();
            cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();
        });

        cy.log("Verifying organ donor quick link no longer exists");
        cy.get(organDonorQuickLinkCardSelector).should("not.exist");

        cy.log("Adding organ donor quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(organDonorAddQuickLinkCheckboxSelector)
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

        cy.log("Verifying organ donor quick link card is present");
        cy.get(organDonorQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});

describe("Disabling organ donor services", () => {
    const testOrganDonorNotPresent = (cy) => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.log("Verifying organ donor quicklink not present");
        cy.get(organDonorQuickLinkCardSelector).should("not.exist");

        cy.log("Verifying add organ donor checkbox not present");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get(organDonorAddQuickLinkCheckboxSelector).should("not.exist");
    };

    it("Should not show quick link and settings when services module is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                enabled: false,
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: true,
                    },
                ],
            },
        });
        testOrganDonorNotPresent(cy);
    });

    it("Should not show quick link and settings when services.organDonor module is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                enabled: true,
                services: [
                    {
                        name: "organDonorRegistration",
                        enabled: false,
                    },
                ],
            },
        });
        testOrganDonorNotPresent(cy);
    });
});
