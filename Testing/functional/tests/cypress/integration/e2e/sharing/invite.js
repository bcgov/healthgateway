const { AuthMethod } = require("../../../support/constants");

const seededSharingLink =
    "/sharing?invite=CfDJ8DPz8s64cm9Hn677BhEz35WPnicstsRyUaZ3qUwofvxkKZZuIcuw6RL2-UYMSGNj9kZpNkw-DrPotSXiePmPuYlmDyaqS6vuEMm0MxvG_qBdtj6rRZgk5hgqkMXQZGpnwl2NmlOhTpSX4QMp6yiGpCsPdt9AF3OjmocpBi7AVxLg";

describe("Sharing by the owner", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "immunizations",
                    enabled: true,
                },
            ],
            sharing: { enabled: true },
        });
    });

    it("Should be associated without error", () => {
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            seededSharingLink
        );
        cy.location("pathname").should("eq", "/sharing");
    });

    it("Should show an error if the invite was already associated with another account.", () => {
        cy.login(
            Cypress.env("keycloak.hthgtwy20.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            seededSharingLink
        );
        cy.location("pathname").should("eq", "/sharing");
        cy.get("[data-testid=errorBanner]").should("be.visible");
    });
});
