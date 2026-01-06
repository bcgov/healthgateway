function getConfig() {
    return cy.readConfig().then((config) => {
        cy.log("Obtaining Keycloak URI and Realm from configuration.");
        const url = new URL(config.openIdConnect.authority);
        const baseURI = url.protocol + "//" + url.hostname;
        const realm = url.pathname.split("/").pop();
        const client = Cypress.env("keycloak_admin_client");
        const secret = Cypress.env("keycloak_admin_secret");
        if (typeof client !== "string" || !client) {
            throw new Error(
                "Configuration: keycloak_admin_client is missing or empty"
            );
        }
        if (typeof secret !== "string" || !secret) {
            throw new Error(
                "Configuration: keycloak_admin_secret is missing or empty"
            );
        }
        const keycloakConfig = {
            baseUri: baseURI,
            realm: realm,
            client: client,
            secret: secret,
        };

        return cy.wrap(keycloakConfig);
    });
}

function authenticate(config) {
    cy.log("Authenticating to Keycloak Admin API.");
    const tokenUri = `${config.baseUri}/auth/realms/${config.realm}/protocol/openid-connect/token`;
    return cy
        .request({
            method: "POST",
            url: tokenUri,
            followRedirect: false,
            form: true,
            body: {
                client_id: config.client,
                client_secret: config.secret,
                grant_type: "client_credentials",
            },
        })
        .then((response) => {
            expect(response.status).to.eq(200);
            expect(response.body).to.have.property("access_token");
            return response.body;
        });
}

export function removeUserIfExists(username) {
    getConfig().then((config) => {
        const adminUri = `${config.baseUri}/auth/admin/realms/${config.realm}`;
        username = `${username}@idir`;
        authenticate(config).then((auth) => {
            cy.log(`Querying user ${username} in Keycloak.`);
            cy.request({
                method: "GET",
                url: `${adminUri}/users?briefRepresentation=true&username=${username}&exact=true`,
                followRedirect: false,
                headers: {
                    Authorization: `Bearer ${auth.access_token}`,
                },
            }).then((response) => {
                expect(response.status).to.eq(200);
                cy.log(`Inspecting query response for user ${username}.`);
                let user = response.body[0];
                if (user && Object.keys(user).length > 0) {
                    cy.log(
                        `User ${username} found with id ${user.id}. Deleting...`
                    );
                    cy.request({
                        method: "DELETE",
                        url: `${adminUri}/users/${user.id}`,
                        followRedirect: false,
                        headers: {
                            Authorization: `Bearer ${auth.access_token}`,
                        },
                    }).then((response) => {
                        expect(response.status).to.eq(204);
                        cy.log(`User ${username} deleted.`);
                    });
                } else {
                    cy.log(`User ${username} not found.`);
                }
            });
        });
    });
}
