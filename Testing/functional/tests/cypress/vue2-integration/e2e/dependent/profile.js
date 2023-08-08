const { AuthMethod } = require("../../../support/constants");
import {
    getCardSelector,
    getTabButtonSelector,
} from "../../../support/functions/dependent";

const existingDependent = {
    hdid: "162346565465464564565463257",
    phn: "9874307175",
    dateOfBirth: "2015-Aug-20",
    otherDelegateCount: 0,
};

describe("dependents - profile", () => {
    beforeEach(() => {
        cy.configureSettings({
            dependents: {
                enabled: true,
                timelineEnabled: true,
            },
        });
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/dependents"
        );
    });

    it("Validate profile tab data", () => {
        const hdid = existingDependent.hdid;

        const cardSelector = getCardSelector(hdid);
        const tabButtonSelector = getTabButtonSelector(hdid, "profile");
        const tabSelector = `[data-testid=profile-tab]`;

        cy.get(cardSelector)
            .should("be.visible")
            .within(() => {
                cy.get(tabButtonSelector)
                    .should("be.visible")
                    .should("not.be.disabled")
                    .click();

                cy.get(`[data-testid=dependent-phn]`)
                    .should("be.visible")
                    .within(() => {
                        cy.get("input").should(
                            "have.value",
                            existingDependent.phn
                        );
                    });

                cy.get(`[data-testid=dependent-date-of-birth]`)
                    .should("be.visible")
                    .within(() => {
                        cy.get("input").should(
                            "have.value",
                            existingDependent.dateOfBirth
                        );
                    });

                cy.get(`[data-testid=dependent-other-delegate-count]`)
                    .should("be.visible")
                    .within(() => {
                        cy.get("input").should(
                            "have.value",
                            existingDependent.otherDelegateCount
                        );
                    });
            });
    });
});
