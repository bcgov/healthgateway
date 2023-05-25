const { AuthMethod } = require("../../../../support/constants");

function selectCardByDate(date) {
    cy.contains("[data-testid=entryCardDate]", date)
        .first()
        .scrollIntoView()
        .should("be.visible")
        .parents("[data-testid=timelineCard]")
        .click();
}

describe("Medication", () => {
    beforeEach(() => {
        cy.configureSettings({
            datasets: [
                {
                    name: "medication",
                    enabled: true,
                },
            ],
        });
        cy.intercept("GET", "**/MedicationStatement/*", {
            fixture: "MedicationService/medicationStatement.json",
        });
        cy.viewport("iphone-6");
        cy.login(
            Cypress.env("keycloak.username"),
            Cypress.env("keycloak.password"),
            AuthMethod.KeyCloak,
            "/timeline"
        );
    });

    it("Validate Card Details for Mobile", () => {
        selectCardByDate("2021-Feb-28");
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.contains(
                "[data-testid=entryCardDetailsTitle]",
                "MINT-ZOPICLONE"
            ).should("be.visible");
            cy.contains(
                "[data-testid=entryCardDetailsSubtitle]",
                "ZOPICLONE"
            ).should("be.visible");

            cy.contains(
                "[data-testid=medication-practitioner]",
                "PLJDDRV"
            ).should("be.visible");

            cy.contains("[data-testid=medication-quantity]", "30").should(
                "be.visible"
            );
            cy.contains("[data-testid=medication-strength]", "7.5").should(
                "be.visible"
            );
            cy.contains("[data-testid=medication-form]", "TABLET").should(
                "be.visible"
            );
            cy.contains(
                "[data-testid=medication-manufacturer]",
                "MINT PHARMACEUTICALS INC"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-din-pin]",
                "DIN: 2391724"
            ).should("be.visible");

            cy.contains(
                "[data-testid=medication-pharmacy-name]",
                "VQJWMQUFRYKPT"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "ZBFKMFYUDPJAFJUPPFXNEQ"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "MAPLE RIDGE"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-phone]",
                "(604) 584-2313"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-fax]",
                "(604) 909-2387"
            ).should("be.visible");

            cy.get("[data-testid=pharmacist-outcome]").should("not.exist");
            cy.get("[data-testid=pharmacist-prescription-provided]").should(
                "not.exist"
            );
            cy.get("[data-testid=pharmacist-redirected-to-provider]").should(
                "not.exist"
            );

            cy.contains(
                "[data-testid=medication-directions]",
                "TAKE 2 TABLETS AT BEDTIME"
            ).should("be.visible");
        });
        cy.get("[data-testid=backBtn]").should("be.visible").click();

        selectCardByDate("2023-May-10");
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.contains(
                "[data-testid=entryCardDetailsTitle]",
                "Pharmacist Assessment"
            ).should("be.visible");
            cy.contains(
                "[data-testid=entryCardDetailsSubtitle]",
                "Minor Ailment - Contraception"
            ).should("be.visible");

            cy.contains("[data-testid=medication-practitioner]", "COBB").should(
                "be.visible"
            );

            cy.get("[data-testid=medication-quantity]").should("not.exist");
            cy.get("[data-testid=medication-strength]").should("not.exist");
            cy.get("[data-testid=medication-form]").should("not.exist");
            cy.get("[data-testid=medication-manufacturer]").should("not.exist");
            cy.contains(
                "[data-testid=medication-din-pin]",
                "PIN: 98890016"
            ).should("be.visible");

            cy.contains(
                "[data-testid=medication-pharmacy-name]",
                "PHARMACY L31"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "2620 QUADRA ST"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "VICTORIA"
            ).should("be.visible");
            cy.get("[data-testid=medication-pharmacy-phone]").should(
                "not.exist"
            );
            cy.get("[data-testid=medication-pharmacy-fax]").should("not.exist");

            cy.get("[data-testid=pharmacist-outcome]").should("be.visible");
            cy.contains(
                "[data-testid=pharmacist-prescription-provided]",
                "Prescription not provided"
            ).should("be.visible");
            cy.get("[data-testid=pharmacist-redirected-to-provider]").should(
                "be.visible"
            );

            cy.get("[data-testid=medication-directions]").should("not.exist");
        });
        cy.get("[data-testid=backBtn]").should("be.visible").click();

        selectCardByDate("2023-Feb-13");
        cy.get("[data-testid=entryDetailsCard]").within(() => {
            cy.contains(
                "[data-testid=entryCardDetailsTitle]",
                "Pharmacist Assessment"
            ).should("be.visible");
            cy.contains(
                "[data-testid=entryCardDetailsSubtitle]",
                "Minor Ailment - Acne"
            ).should("be.visible");

            cy.contains("[data-testid=medication-practitioner]", "CORN").should(
                "be.visible"
            );

            cy.get("[data-testid=medication-quantity]").should("not.exist");
            cy.get("[data-testid=medication-strength]").should("not.exist");
            cy.get("[data-testid=medication-form]").should("not.exist");
            cy.get("[data-testid=medication-manufacturer]").should("not.exist");
            cy.contains(
                "[data-testid=medication-din-pin]",
                "PIN: 98890001"
            ).should("be.visible");

            cy.contains(
                "[data-testid=medication-pharmacy-name]",
                "PHARMACY A42"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "TILLICUM MALL"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "144/3170 TILLICUM RD"
            ).should("be.visible");
            cy.contains(
                "[data-testid=medication-pharmacy-address]",
                "VICTORIA"
            ).should("be.visible");
            cy.get("[data-testid=medication-pharmacy-phone]").should(
                "not.exist"
            );
            cy.get("[data-testid=medication-pharmacy-fax]").should("not.exist");

            cy.get("[data-testid=pharmacist-outcome]").should("be.visible");
            cy.contains(
                "[data-testid=pharmacist-prescription-provided]",
                "Prescription provided"
            ).should("be.visible");
            cy.get("[data-testid=pharmacist-redirected-to-provider]").should(
                "not.exist"
            );

            cy.get("[data-testid=medication-directions]").should("not.exist");
        });
    });
});
