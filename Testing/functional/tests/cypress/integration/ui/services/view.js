import { AuthMethod } from "../../../support/constants";
import {
    CommunicationFixture,
    CommunicationType,
    setupCommunicationIntercept,
    setupPatientIntercept,
    setupUserProfileIntercept,
} from "../../../support/functions/intercept";

const servicesTestsConstants = {
    servicesUrl: "/services",
    unauthorized: "/unauthorized",
};

describe("Authenticated Services View", () => {
    beforeEach(() => {
        setupPatientIntercept();
        setupUserProfileIntercept();
        setupCommunicationIntercept();
        setupCommunicationIntercept({
            communicationType: CommunicationType.InApp,
            communicationFixture: CommunicationFixture.InApp,
        });

        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/services"
        );
    });

    it("The url should be the services url if services enabled", () => {
        cy.configureSettings({
            services: {
                enabled: true,
            },
        });
        cy.url().should("include", servicesTestsConstants.servicesUrl);
    });

    it("The url should be the unauthorized url if services is disabled", () => {
        cy.configureSettings({});
        cy.url().should("include", servicesTestsConstants.unauthorized);
    });
});
