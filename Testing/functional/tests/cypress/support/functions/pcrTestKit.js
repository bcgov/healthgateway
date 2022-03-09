export function clickManualRegistrationButton() {
    cy.get("[data-testid=btn-manual]")
        .should("be.enabled", "be.visible")
        .click();
}

export function clickRegisterKitButton() {
    cy.get("[data-testid=btn-register-kit]")
        .should("be.enabled", "be.visible")
        .click();
}

export function getPcrTestTakenTime(minutes) {
    switch (minutes) {
        case 5:
            return "Just now";
        case 30:
            return "Within 30 minutes";
        case 120:
            return "Within 2 hours";
        case 360:
            return "Within 6 hours";
        case 1440:
            return "Within 24 hours";
        case 4320:
            return "Within 36 hours";
    }
}
