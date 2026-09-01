const { AuthMethod } = require("../../../../support/constants");

const defaultTimeout = 60000;

describe("Laboratory Orders Queued", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "labResult",
                    enabled: true,
                },
            ],
        });
        cy.viewport("iphone-6");
        cy.intercept("GET", "**/Laboratory/LaboratoryOrders*").as(
            "getLaboratoryOrders"
        );
        cy.login(
            Cypress.env("keycloak.laboratory.queued.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
        cy.wait("@getLaboratoryOrders", { timeout: defaultTimeout })
            .its("response.statusCode")
            .should("eq", 200);
        cy.checkTimelineHasLoaded();
    });

    it("Show Queued Alert Message", () => {
        cy.log("Verifying queued alert message displays");

        cy.get("[data-testid=laboratory-orders-queued-alert-message]").should(
            "be.visible"
        );
    });
});
