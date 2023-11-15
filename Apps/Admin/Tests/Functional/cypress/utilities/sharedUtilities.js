export function getTableRows(tableSelector) {
    cy.get(tableSelector).should("be.visible");
    return cy.get(`${tableSelector} tbody`).find("tr.mud-table-row");
}

export function getTodayPlusDaysDate(days) {
    let newDay = new Date(Date.now());
    cy.log(`New day: ${newDay}`);
    newDay.setDate(newDay.getDate() + days);
    cy.log(`New day plus/minus ${days}: ${newDay}`);

    // Convert the date and time to a localized string
    const localizedDateString = newDay.toLocaleString("en-US", {
        timeZone: "America/Vancouver",
    });

    // Parse the localized string to a Date object
    const parsedDate = new Date(localizedDateString);
    cy.log(`Parsed date: ${parsedDate}`);

    // Get the components: year, month, and day
    const year = parsedDate.getFullYear();
    const month = String(parsedDate.getMonth() + 1).padStart(2, "0");
    const day = String(parsedDate.getDate()).padStart(2, "0");

    // Format the date to "yyyy-mm-dd" format
    const dateString = `${year}-${month}-${day}`;
    cy.log(`Today plus ${days} => ${dateString}`);
    return dateString;
}

export function selectTab(tabsSelector, tabText) {
    cy.get(tabsSelector).contains(".mud-tab", tabText).click();
}
