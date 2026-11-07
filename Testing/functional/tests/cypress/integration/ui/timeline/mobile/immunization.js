import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

describe("Immunization", () => {
    beforeEach(() => {
        cy.intercept("GET", "**/Immunization?hdid=*", {
            fixture: "ImmunizationService/immunization.json",
        });
        configureFakeSettings({
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        setupStandardFixtures();

        cy.viewport("iphone-6");
        fakeLogin("/timeline");
        cy.checkTimelineHasLoaded();
    });

    it("Validate Card Details on Mobile", () => {
        cy.get("[data-testid=timelineCard]").first().click();

        cy.get("[data-testid=entryDetailsCard]").should("be.visible");
        cy.get("[data-testid=backBtn]").should("be.visible");
        cy.get("[data-testid=entryCardDetailsTitle]").should("be.visible");
        cy.get("[data-testid=entryCardDate]").should("be.visible");
        cy.get("[data-testid=immunizationProductTitle]").should("be.visible");
        cy.get("[data-testid=immunizationAgentNameTitle]").should("be.visible");
        cy.get("[data-testid=immunizationProviderTitle]").should("be.visible");
        cy.get("[data-testid=immunizationLotTitle]").should("be.visible");
    });
});
