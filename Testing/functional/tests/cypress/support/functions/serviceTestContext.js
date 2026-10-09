// Create one context per spec. Network setup belongs inside each test so a
// failed request does not skip the remaining tests through a failed hook.
export function createServiceTestContext({ clientCredentials = false } = {}) {
    let configuration;
    let tokens;
    let expiresAt = 0;

    function getConfig() {
        return cy.then(() => {
            if (configuration) {
                return Cypress._.cloneDeep(configuration);
            }
            return cy.readConfig().then((result) => {
                configuration = Cypress._.cloneDeep(result);
                return Cypress._.cloneDeep(configuration);
            });
        });
    }

    function getTokens() {
        return cy.then(() => {
            if (tokens && Date.now() < expiresAt) {
                return cy.wrap(tokens, { log: false });
            }
            tokens = undefined;
            expiresAt = 0;
            return getConfig().then(() => {
                // Count lifetime from before authentication to avoid overstating
                // validity when the authentication requests themselves are slow.
                const requestedAt = Date.now();
                const request = clientCredentials
                    ? cy.getTokens()
                    : cy
                          .env(["keycloak.password"])
                          .then(({ "keycloak.password": password }) =>
                              cy.getTokens(
                                  Cypress.expose("keycloak.username"),
                                  password
                              )
                          );
                return request.then((result) => {
                    if (
                        !result ||
                        typeof result.access_token !== "string" ||
                        !result.access_token
                    ) {
                        throw new Error(
                            "Authentication did not return an access token"
                        );
                    }
                    const lifetime = Number(result.expires_in);
                    // Unknown or short lifetimes are usable for this test only.
                    if (Number.isFinite(lifetime) && lifetime > 60) {
                        tokens = result;
                        expiresAt = requestedAt + (lifetime - 60) * 1000;
                    }
                    return cy.wrap(result, { log: false });
                });
            });
        });
    }

    return { getConfig, getTokens };
}
