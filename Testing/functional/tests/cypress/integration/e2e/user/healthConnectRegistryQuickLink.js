const { AuthMethod } = require("../../../support/constants");

const homePath = "/home";

const addQuickLinkButtonSelector = "[data-testid=add-quick-link-button]";
const addQuickLinkSubmitButtonSelector = "[data-testid=add-quick-link-btn]";
const quickLinkMenuButtonSelector = "[data-testid=card-menu-button]";
const quickLinkRemoveButtonSelector = "[data-testid=remove-quick-link-button]";
const healthConnectQuickLinkCardSelector =
    "[data-testid=health-connect-registry-card]";
const healthConnectAddQuickLinkChipSelector =
    "[data-testid=health-connect-registry-filter]";
const serviceName = "healthConnectRegistry";

describe("Health Connect Registry Card", () => {
    it("Remove and add health connect registry card quick link", () => {
        cy.configureSettings({
            services: {
                enabled: true,
                services: [
                    {
                        name: serviceName,
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

        cy.log("Verifying health connect quick link card is present");
        cy.get(healthConnectQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );

        cy.log("Removing health connect quick link");
        cy.get(healthConnectQuickLinkCardSelector).within(() => {
            cy.get(quickLinkMenuButtonSelector)
                .should("be.visible", "be.enabled")
                .click();
        });
        cy.get(quickLinkRemoveButtonSelector).should("be.visible").click();

        cy.log("Verifying health connect quick link no longer exists");
        cy.get(healthConnectQuickLinkCardSelector).should("not.exist");

        cy.log("Adding health connect quick link");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(healthConnectAddQuickLinkChipSelector).should("exist").click();
        cy.get(addQuickLinkSubmitButtonSelector)
            .should("be.visible")
            .should("be.enabled")
            .click();

        cy.log("Verifying add quick link button is disabled");
        cy.get(addQuickLinkButtonSelector).should(
            "be.visible",
            "not.be.enabled"
        );

        cy.log("Verifying health connect quick link card is present");
        cy.get(healthConnectQuickLinkCardSelector).should(
            "be.visible",
            "be.enabled"
        );
    });
});

describe("Disabling health connect services", () => {
    const testQuickLinkNotPresent = (cy) => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            homePath
        );
        cy.log("Verifying health connect quicklink not present");
        cy.get(healthConnectQuickLinkCardSelector).should("not.exist");

        cy.log("Verifying add health connect chip not present");
        cy.get(addQuickLinkButtonSelector)
            .should("be.visible", "be.enabled")
            .click();
        cy.get(healthConnectAddQuickLinkChipSelector).should("not.exist");
    };

    it("Should not show quick link and settings when health connect service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                services: [
                    {
                        name: serviceName,
                        enabled: true,
                    },
                ],
            },
        });
        testQuickLinkNotPresent(cy);
    });

    it("Should not show quick link and settings when health connect service is disabled", () => {
        cy.configureSettings({
            datasets: [
                {
                    name: "healthVisit",
                    enabled: true,
                },
            ],
            services: {
                enabled: true,
            },
        });
        testQuickLinkNotPresent(cy);
    });
});
