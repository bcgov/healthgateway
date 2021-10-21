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
        restoreAuthCookies(): void;
        checkVaccineRecordHasLoaded(): void;
    }
}
