export const deleteDownloadsFolder = () => {
    const downloadsFolder = Cypress.config("downloadsFolder");
    cy.task("deleteFolder", downloadsFolder);
};

export function convertStringToDate(stringDate) {
    return new Date(stringDate);
}

export function getMonth(date) {
    // add 1 to the returned month since they're indexed starting at 0
    return convertStringToDate(date).getMonth() + 1;
}

export function getYear(date) {
    return convertStringToDate(date).getFullYear();
}

export function getDay(date) {
    return convertStringToDate(date).getDate();
}

export function selectorShouldBeVisible(selector) {
    cy.get(selector).should("be.visible");
}

export function selectorShouldNotExists(selector) {
    cy.get(selector).should("not.exist");
}

export function getPcrTestTakenTime(number) {
    switch (number) {
        case 5:
            return "Just now";
            break;
        case 30:
            return "Within 30 minutes";
            break;
        case 120:
            return "Within 2 hours";
            break;
        case 360:
            return "Within 6 hours";
            break;
        case 1440:
            return "Within 24 hours";
            break;
        case 4320:
            return "Within 36 hours";
            break;
    }
}

export function selectOption(selector, option) {
    return cy.get(selector).should("be.visible", "be.enabled").select(option);
}
