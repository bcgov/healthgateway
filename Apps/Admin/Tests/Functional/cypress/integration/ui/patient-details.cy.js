import { performSearch } from "../../utilities/supportUtilities";
import { getTableRows } from "../../utilities/sharedUtilities";

const hdid = "P6FFO433A5WPMVTGM7T4ZVWBKCSVNAYGTWTU3J2LWMGUMERKI72A";
const hdidPatientNotFound =
    "RD33Y2LJEUZCY2TCMOIECUTKS3E62MEQ62CSUL6Q553IHHBI3AWZ";
const hdidPatientDeceased =
    "MPOLGP66AV4PPDB6ZMYEWQ63WKRYPM4EPDW5MSXA2LA65EQOEMCQ";
const hdidPatientNotUser =
    "C54JQKXANHJK7TIYBRCHJBOKXOIXASNQE76WSRCTOPKOXRMI5OAA";

describe("Patient details", () => {
    beforeEach(() => {
        // HDID with results
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdid}&queryType=Hdid`,
            {
                fixture: "SupportService/users-hdid.json",
            }
        );

        // HDID not found
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdidPatientNotFound}&queryType=Hdid`,
            {
                fixture: "SupportService/users-hdid-not-found.json",
            }
        );

        // HDID deceased
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdidPatientDeceased}&queryType=Hdid`,
            {
                fixture: "SupportService/users-hdid-deceased.json",
            }
        );

        // HDID not a user
        cy.intercept(
            "GET",
            `**/Support/Users?queryString=${hdidPatientNotUser}&queryType=Hdid`,
            {
                fixture: "SupportService/users-hdid-not-user.json",
            }
        );

        // Message Verifications
        cy.intercept("GET", `**/Support/Verifications?hdid=${hdid}`, {
            fixture: "SupportService/messaging-verification.json",
        });

        cy.login(
            Cypress.env("keycloak_username"),
            Cypress.env("keycloak_password"),
            "/support"
        );
    });

    it("Verify patient details", () => {
        performSearch("HDID", hdid);

        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdid);
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");
        cy.get("[data-testid=profile-created-datetime]").should("be.visible");
        cy.get("[data-testid=profile-last-login-datetime]").should(
            "be.visible"
        );
        getTableRows("[data-testid=messaging-verification-table]").should(
            "have.length",
            2
        );
    });

    it("Verify patient not found", () => {
        performSearch("HDID", hdidPatientNotFound);

        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdidPatientNotFound);
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        cy.get("[data-testid=profile-created-datetime]").should("not.exist");
        cy.get("[data-testid=profile-last-login-datetime]").should("not.exist");
        cy.get("[data-testid=messaging-verification-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=no-hg-profile]").should("be.visible");

        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient not found");
    });

    it("Verify patient deceased", () => {
        performSearch("HDID", hdidPatientDeceased);

        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdidPatientDeceased);
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        cy.get("[data-testid=profile-created-datetime]").should("not.exist");
        cy.get("[data-testid=profile-last-login-datetime]").should("not.exist");
        cy.get("[data-testid=messaging-verification-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=no-hg-profile]").should("be.visible");

        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient is deceased");
    });

    it("Verify patient not a user", () => {
        performSearch("HDID", hdidPatientNotUser);

        cy.get("[data-testid=user-table]")
            .find("tbody tr.mud-table-row")
            .first()
            .click();

        cy.get("[data-testid=patient-name]").should("be.visible");
        cy.get("[data-testid=patient-dob]").should("be.visible");
        cy.get("[data-testid=patient-phn]").should("be.visible");
        cy.get("[data-testid=patient-hdid]")
            .should("be.visible")
            .contains(hdidPatientNotUser);
        cy.get("[data-testid=patient-physical-address]").should("be.visible");
        cy.get("[data-testid=patient-mailing-address]").should("be.visible");

        cy.get("[data-testid=profile-created-datetime]").should("not.exist");
        cy.get("[data-testid=profile-last-login-datetime]").should("not.exist");
        cy.get("[data-testid=messaging-verification-table]").should(
            "not.exist"
        );
        cy.get("[data-testid=no-hg-profile]").should("be.visible");

        cy.get("[data-testid=user-banner-feedback-status-warning-message]")
            .should("be.visible")
            .contains("Patient is not a user");
    });
});
