export function triggerEmptyValidation(vuetifySelector) {
    cy.get(vuetifySelector + " input")
        .should("be.enabled")
        .clear()
        .blur();
    cy.get(vuetifySelector).should("have.class", "v-input--error");
}
