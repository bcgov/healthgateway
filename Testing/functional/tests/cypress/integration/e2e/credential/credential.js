const { AuthMethod } = require("../../../support/constants");

describe("Credentials (Disabled)", () => {
    beforeEach(() => {
        cy.enableModules(["Patient", "Immunization"]);
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak
        );
    });

    it("Verify No Credentials Link in Sidebar", () => {
        cy.get("[data-testid=sidebar]").should("be.visible");
        cy.get("[data-testid=menuBtnCredentialsLink]").should("not.be.visible");
    });

    it("Verify Credentials Page HTTP 401", () => {
        cy.visit("/credentials");
        cy.contains("h1", "401").should("be.visible");
    });
});

describe("Credentials (No Connection Established)", () => {
    beforeEach(() => {
        cy.enableModules(["Patient", "Immunization", "Credential"]);
        cy.intercept(
            "GET",
            "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Connection",
            {
                fixture: "WebClientService/wallet/connectionUndefined.json",
            }
        );
        cy.intercept(
            "POST",
            "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Connection",
            {
                fixture: "WebClientService/wallet/connectionPending.json",
            }
        );
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/credentials"
        );
    });

    it("Verify Credentials Link in Sidebar", () => {
        cy.get("[data-testid=menuBtnCredentialsLink]").should("be.visible");
    });

    it("Verify Credentials Page Loads", () => {
        cy.get("[data-testid=credentialInstructions]").should("be.visible");
    });

    it("Verify Immunizations Are Required for Credentials", () => {
        cy.intercept("GET", "**/v1/api/Immunization/*", {
            fixture: "ImmunizationService/immunizationNoRecords.json",
        });
        cy.visit("/credentials");
        cy.get("[data-testid=credentialsStartButton]")
            .should("be.visible")
            .should("not.be.enabled");
    });

    it("Verify Create Wallet Connection", () => {
        cy.get("[data-testid=credentialInstructions]").should("be.visible");
        cy.get("[data-testid=credentialsStartButton]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=createConnectionButton]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=credentialManagement]").should("be.visible");
        cy.get("[data-testid=mobileConnectButton]").should("be.visible");
        cy.get("[data-testid=qrCodeImage]").should("be.visible");
    });
});

describe("Credentials (Connection Established)", () => {
    beforeEach(() => {
        cy.enableModules(["Patient", "Immunization", "Credential"]);
        cy.intercept(
            "GET",
            "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Connection",
            {
                fixture: "WebClientService/wallet/connectionNoCredentials.json",
            }
        );
        cy.intercept(
            "POST",
            "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Credentials",
            {
                fixture: "WebClientService/wallet/credentialCreated.json",
            }
        );
        cy.intercept(
            "DELETE",
            "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Connection",
            {
                fixture: "WebClientService/wallet/connectionDisconnected.json",
            }
        );
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/credentials"
        );
    });

    // it("Verify Create Credential", () => {
    //     cy.get("[data-testid=credentialCard]")
    //         .first()
    //         .within(() => {
    //             cy.get("[data-testid=addCredentialButton]")
    //                 .should("be.visible")
    //                 .should("be.enabled")
    //                 .click();
    //             cy.get("[data-testid=credentialStatus]")
    //                 .should("be.visible")
    //                 .should("have.text", "Created");
    //         });
    // });

    it("Verify Disconnect Connection", () => {
        cy.log("Verifying connection menu is enabled");
        cy.get("[data-testid=connectionMenuBtn] > a")
            .should("be.visible")
            .should("not.have.class", "disabled")
            .click();

        cy.log("Verifying disconnect option is enabled in connection menu");
        cy.get("[data-testid=disconnectMenuBtn]")
            .should("be.visible")
            .should("not.have.class", "disabled");

        cy.log("Verifying disconnect modal opens");
        cy.get("[data-testid=disconnectMenuBtn]").click();
        cy.get("[data-testid=deleteConfirmationModal] .modal").should(
            "be.visible"
        );

        cy.log("Verifying disconnect modal can be cancelled");
        cy.get("[data-testid=cancelDeleteBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=credentialsInWallet]").should("be.visible");

        cy.log("Verifying connection can be disconnected");
        cy.get("[data-testid=connectionMenuBtn]").click();
        cy.get("[data-testid=disconnectMenuBtn]").click();
        cy.get("[data-testid=confirmDeleteBtn]")
            .should("be.visible")
            .should("be.enabled")
            .click();
        cy.get("[data-testid=credentialsInWallet]").should("not.exist");
    });
});

// describe("Credentials (1 Credential In Wallet)", () => {
//     beforeEach(() => {
//         cy.enableModules(["Patient", "Immunization", "Credential"]);
//         cy.intercept(
//             "GET",
//             "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Connection",
//             {
//                 fixture: "WebClientService/wallet/connectionOneCredential.json",
//             }
//         );
//         cy.intercept(
//             "POST",
//             "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Credentials",
//             {
//                 fixture: "WebClientService/wallet/credentialCreated.json",
//             }
//         );
//         cy.intercept(
//             "DELETE",
//             "/v1/api/Wallet/P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A/Credential",
//             {
//                 fixture: "WebClientService/wallet/credentialRevoked.json",
//             }
//         );
//         cy.login(
//             Cypress.env("keycloak.username"),
//             Cypress.env("keycloak.password"),
//             AuthMethod.KeyCloak,
//             "/credentials"
//         );
//     });

//     it("Verify Revoke Credential", () => {
//         cy.get("[data-testid=credentialsInWallet]").should("have.text", "1");
//         cy.get("[data-testid=credentialCard]")
//             .first()
//             .within(() => {
//                 cy.get("[data-testid=credentialMenuBtn] > a")
//                     .should("be.visible")
//                     .should("not.have.class", "disabled")
//                     .click();
//                 cy.get("[data-testid=revokeCredentialMenuBtn]")
//                     .should("be.visible")
//                     .should("not.have.class", "disabled")
//                     .click();
//                 cy.get("[data-testid=addCredentialButton]")
//                     .should("be.visible")
//                     .should("be.enabled");
//             });
//         cy.get("[data-testid=credentialsInWallet]").should("have.text", "0");
//     });

//     it("Verify Reissue Credential", () => {
//         cy.get("[data-testid=credentialsInWallet]").should("have.text", "1");
//         cy.get("[data-testid=credentialCard]")
//             .first()
//             .within(() => {
//                 cy.get("[data-testid=credentialMenuBtn] > a")
//                     .should("be.visible")
//                     .should("not.have.class", "disabled")
//                     .click();
//                 cy.get("[data-testid=reissueCredentialMenuBtn]")
//                     .should("be.visible")
//                     .should("not.have.class", "disabled")
//                     .click();
//                 cy.get("[data-testid=credentialStatus]")
//                     .should("be.visible")
//                     .should("have.text", "Created");
//             });
//         cy.get("[data-testid=credentialsInWallet]").should("have.text", "0");
//     });
// });
