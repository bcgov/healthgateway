declare namespace Cypress {
    interface Chainable {
        login(
            username: string,
            password: string,
            authMethod?: string,
            path?: string
        ): void;
        getTokens(username: string, password: string): void;
        readConfig(): Chainable<any>;
        checkTimelineHasLoaded(): void;
        enableModules(modules: string[]): Chainable<any>;
        setupDownloads(): void;
        deleteDownloadsFolder(): void;
        restoreAuthCookies(): void;
        // /**
        //  * Select an `<option>` with specific text, value, or index within a `<select>`.
        //  * This overwrite ensures the `<select>` element is visible and enabled.
        //  *
        //  * @see https://on.cypress.io/select
        //  */
        // select(
        //     valueOrTextOrIndex: string | number | Array<string | number>,
        //     options?: Partial<SelectOptions>
        // ): Chainable<any>;
        /**
         * Asserts that a `<select>` element contains an `<option>` with a particular value.
         */
        shouldContain(value: string): Chainable<any>;
        /**
         * Asserts that a `<select>` element does not contain an `<option>` with a particular value.
         */
        shouldNotContain(value: string): Chainable<any>;
        /**
         * Populates the values in an hg-date-dropdown component from a given date string.
         *
         * @param {string} yearSelector The selector for the year `<select>`.
         * @param {string} monthSelector The selector for the month `<select>`.
         * @param {string} daySelector The selector for the day `<select>`.
         * @param {string} dateString A date string that can be converted into a Date object.
         */
        populateDateDropdowns(
            yearSelector: string,
            monthSelector: string,
            daySelector: string,
            dateString: string
        ): void;
    }
}
