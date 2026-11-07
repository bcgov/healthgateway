import {
    configureFakeSettings,
    fakeLogin,
    useFakeEnvironment,
} from "../../../support/functions/fakeEnvironment";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Runs against the SPA alone: fixture configuration, fake Keycloak, every API call stubbed.
useFakeEnvironment();

const servicesTestsConstants = {
    servicesUrl: "/services",
    unauthorized: "/unauthorized",
};

describe("Authenticated Services View", () => {
    beforeEach(() => {
        setupStandardFixtures();
    });

    it("The url should be the services url if services enabled", () => {
        configureFakeSettings({
            services: {
                enabled: true,
            },
        });

        fakeLogin("/services");

        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });

    it("The url should be the unauthorized url if services is disabled", () => {
        configureFakeSettings({});

        fakeLogin("/services");

        cy.url().should("include", servicesTestsConstants.unauthorized);
    });
});
