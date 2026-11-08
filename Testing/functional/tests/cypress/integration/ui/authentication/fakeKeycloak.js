import { setupFakeEnvironment } from "../../../support/functions/fakeEnvironment";
import {
    fakeKeycloakState,
    FakeUser,
    setFakeKeycloakUser,
    signInToFakeKeycloak,
} from "../../../support/functions/fakeKeycloak";
import { setupStandardFixtures } from "../../../support/functions/intercept";

// Proves the SPA's OIDC flow end to end against the fake Keycloak, with no backend at all.
describe("Authentication against fake Keycloak", () => {
    beforeEach(() => {
        setupFakeEnvironment();
        setupStandardFixtures();
        cy.intercept("POST", "**/UserFeedback/Rating*", {
            statusCode: 200,
            body: { resourcePayload: true, resultStatus: 1 },
        });
    });

    it("Signs in with BC Services Card from the login page", () => {
        cy.visit("/login?redirect=/home");

        cy.get("#BCSCBtn").should("be.visible").click();

        cy.location("pathname").should("eq", "/home");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");
        cy.get("[data-testid=health-records-card]").should("be.visible");

        cy.then(() => {
            const { authRequests, tokenRequests } = fakeKeycloakState();
            const login = authRequests.find((r) => r.prompt !== "none");
            expect(login.idpHint, "kc_idp_hint").to.eq("bcsc");
            expect(login.codeChallengeMethod, "PKCE method").to.eq("S256");
            expect(new URL(login.redirectUri).pathname, "login callback").to.eq(
                "/loginCallback"
            );
            expect(
                tokenRequests.filter(
                    (r) => r.grantType === "authorization_code"
                ),
                "code exchanges"
            )
                .to.have.length(1)
                .and.have.nested.property("[0].hasCodeVerifier", true);
        });
    });

    it("Restores an existing Keycloak session silently with check-sso", () => {
        signInToFakeKeycloak(FakeUser.Bcsc);

        cy.visit("/home");

        cy.location("pathname").should("eq", "/home");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");

        cy.reload();

        cy.location("pathname").should("eq", "/home");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");
        cy.then(() => {
            const { authRequests } = fakeKeycloakState();
            // Vue checks Keycloak again on every load; ClientApp2 keeps its tokens in
            // sessionStorage and refreshes them. Either way the user never sees a login.
            expect(authRequests, "auth requests").to.not.be.empty;
            authRequests.forEach((request) =>
                expect(request.prompt, "silent check").to.eq("none")
            );
        });
    });

    it("Sends a signed-out visitor to the login page", () => {
        cy.visit("/home");

        cy.location("pathname").should("eq", "/login");
        cy.location("search").should((search) =>
            expect(new URLSearchParams(search).get("redirect")).to.eq("/home")
        );
        cy.get("[data-testid=headerDropdownBtn]").should("not.exist");
        cy.then(() => {
            // With the session status iframe the SPA may skip the silent auth request entirely.
            const { authRequests, tokenRequests } = fakeKeycloakState();
            authRequests.forEach((request) =>
                expect(request.prompt, "silent check").to.eq("none")
            );
            expect(tokenRequests, "token requests").to.be.empty;
        });
    });

    it("Signs out of the SPA and Keycloak", () => {
        signInToFakeKeycloak(FakeUser.Bcsc);
        cy.visit("/home");
        cy.get("[data-testid=headerDropdownBtn]").should("exist");

        cy.get("[data-testid=headerDropdownBtn]").click();
        cy.get("[data-testid=logoutBtn]").click();
        cy.get("[data-testid=ratingModalSkipBtn]").click();

        cy.get("[data-testid=logout-complete-msg]").should("be.visible");
        cy.location("pathname").should("eq", "/logoutComplete");
        cy.then(() => {
            const state = fakeKeycloakState();
            expect(state.signedIn, "Keycloak session").to.eq(false);
            expect(state.logoutRequests).to.have.length(1);
            expect(state.logoutRequests[0].hasIdTokenHint).to.eq(true);
        });

        // The session is gone, so a protected page asks for login again.
        cy.visit("/home");
        cy.location("pathname").should("eq", "/login");
    });

    it("Rejects users from an identity provider other than BCSC", () => {
        setFakeKeycloakUser(FakeUser.Idir);
        cy.visit("/login?redirect=/home");

        cy.get("#IDIRBtn").should("be.visible").click();

        cy.location("pathname").should("eq", "/idirLoggedIn");
        cy.then(() => {
            const login = fakeKeycloakState().authRequests.find(
                (r) => r.prompt !== "none"
            );
            expect(login.idpHint, "kc_idp_hint").to.eq("idir");
        });
    });
});
