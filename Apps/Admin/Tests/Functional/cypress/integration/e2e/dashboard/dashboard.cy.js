const { localDevUri } = require("../../../support/constants");
import { verifyTestingEnvironment } from "../../../support/functions/environment";

describe("Dashboard", () => {
    beforeEach(() => {
        verifyTestingEnvironment();
        cy.login(Cypress.env("idir_username"), Cypress.env("idir_password"));
    });

    afterEach(() => {
        cy.log("Dashboard test finished.");
        cy.logout();
    });

    it("Verify dashboards counts.", () => {
        cy.log("Dashboard test started.");
        cy.get("[data-testid=total-registered-users]").contains(6);
        cy.get("[data-testid=total-dependents]").contains(2);
        cy.get("[data-testid=total-unique-users]").contains(2);
        cy.get("[data-testid=average-rating]").contains("4.00");

        cy.get("[data-testid=daily-data-table]")
            .first()
            .within(() => {
                cy.get(
                    "[data-testid=daily-data-total-registered-users]"
                ).contains("4");
                cy.get(
                    "[data-testid=daily-data-total-logged-in-users]"
                ).contains("6");
                cy.get("[data-testid=daily-data-dependents]").contains("2");
            });

        cy.log("Dashboard test finished.");
    });
});
