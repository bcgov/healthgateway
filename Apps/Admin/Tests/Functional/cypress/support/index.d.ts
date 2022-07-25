declare namespace Cypress {
    interface Chainable {
        logout(): void;
        login(
            username: string,
            password: string,
            path: string,
            authMethod?: string
        ): void;
        readConfig(): Chainable<any>;
        /**
         * Select an `<option>` with specific text, value, or index within a `<select>`.
         * This overwrite ensures the `<select>` element is visible and enabled.
         *
         * @see https://on.cypress.io/select
         */
        select(
            valueOrTextOrIndex: string | number | Array<string | number>,
            options?: Partial<SelectOptions>
        ): Chainable<any>;
    }
}
