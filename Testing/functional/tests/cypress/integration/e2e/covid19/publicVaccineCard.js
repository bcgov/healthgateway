const dateOfBirthInput = "[data-testid=dateOfBirthInput] input";
const dateOfVaccineInput = "[data-testid=dateOfVaccineInput] input";

const vaccineCardUrl = "/vaccinecard";

const fullyVaccinatedPhn = "9735361219";
const fullyVaccinatedDobYear = "1994";
const fullyVaccinatedDobMonth = "JUN";
const fullyVaccinatedDobDay = "09";
const fullyVaccinatedDovYear = "2021";
const fullyVaccinatedDovMonth = "JAN";
const fullyVaccinatedDovDay = "20";

function enterVaccineCardPHN(phn) {
    cy.get("[data-testid=phnInput] input").clear().type(phn);
}

function clickVaccineCardEnterButton() {
    cy.get("[data-testid=btnEnter]").should("be.enabled", "be.visible").click();
}

describe("Public Vaccine Card Result", () => {
    it("Partially Vaccinated", () => {
        const phn = "9735353315";
        const dobYear = "1967";
        const dobMonth = "JUN";
        const dobDay = "02";
        const dovYear = "2021";
        const dovMonth = "JUL";
        const dovDay = "04";

        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                    showFederalProofOfVaccination: true,
                },
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });

        cy.logout();
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(phn);

        cy.get(dateOfBirthInput).type(`${dobYear}-${dobMonth}-${dobDay}`, {
            force: true,
        });
        cy.get(dateOfVaccineInput).type(`${dovYear}-${dovMonth}-${dovDay}`, {
            force: true,
        });

        clickVaccineCardEnterButton();

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusPartiallyVaccinated]").should("be.visible");
    });

    it("Fully Vaccinated", () => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                    showFederalProofOfVaccination: true,
                },
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dateOfBirthInput).type(
            `${fullyVaccinatedDobYear}-${fullyVaccinatedDobMonth}-${fullyVaccinatedDobDay}`
        );
        cy.get(dateOfVaccineInput).type(
            `${fullyVaccinatedDovYear}-${fullyVaccinatedDovMonth}-${fullyVaccinatedDovDay}`
        );

        clickVaccineCardEnterButton();

        cy.get("[data-testid=formTitleVaccineCard]").should("be.visible");
        cy.get("[data-testid=statusVaccinated]").should("be.visible");
    });
});

describe("Public Vaccine Card Downloads", () => {
    beforeEach(() => {
        cy.configureSettings({
            covid19: {
                publicCovid19: {
                    vaccineCardEnabled: true,
                    showFederalProofOfVaccination: true,
                },
            },
            datasets: [
                {
                    name: "immunization",
                    enabled: true,
                },
            ],
        });
        cy.logout();
        cy.visit(vaccineCardUrl);

        enterVaccineCardPHN(fullyVaccinatedPhn);
        cy.get(dateOfBirthInput).type(
            `${fullyVaccinatedDobYear}-${fullyVaccinatedDobMonth}-${fullyVaccinatedDobDay}`
        );
        cy.get(dateOfVaccineInput).type(
            `${fullyVaccinatedDovYear}-${fullyVaccinatedDovMonth}-${fullyVaccinatedDovDay}`
        );

        clickVaccineCardEnterButton();
    });

    it("Save Federal Proof of Vaccination PDF", () => {
        cy.get("[data-testid=save-dropdown-btn]")
            .should("be.visible", "be.enabled")
            .click();
        cy.get("[data-testid=save-as-pdf-dropdown-item]")
            .should("be.visible")
            .click();

        cy.get("[data-testid=generic-message-modal]").should("be.visible");
        cy.get("[data-testid=generic-message-submit-btn]").click();

        cy.verifyDownload("VaccineProof.pdf");
    });
});
