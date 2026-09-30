import { localDevUri } from "../constants";
import { startLoginDiagnostics } from "./loginDiagnostics";

// Per-test settings are deliberately kept outside authentication storage.
let configuredSettings;

export function resetTestSettings() {
    configuredSettings = undefined;
}

export function resolveTestSettings() {
    return configuredSettings
        ? cy.wrap(configuredSettings, { log: false })
        : cy.readConfig();
}

function setBooleanProperties(object, enabled) {
    const properties = Object.keys(object);
    for (const property of properties) {
        const value = object[property];
        if (typeof value === "object" && value !== null) {
            setBooleanProperties(value, enabled);
        } else if (typeof value === "boolean") {
            object[property] = enabled;
        }
    }
}

function populateFallbackValues(baseArray, fallbackArray, idProperty = "name") {
    if (!baseArray) {
        return;
    }

    for (const f of fallbackArray) {
        if (!baseArray.some((b) => b[idProperty] === f[idProperty])) {
            baseArray.push(f);
        }
    }
}

function overrideProperties(baseObject, overrideObject) {
    const properties = Object.keys(overrideObject ?? {});
    for (const property of properties) {
        if (
            property === "__proto__" ||
            property === "constructor" ||
            property === "prototype"
        ) {
            throw new Error(`Can't override unsafe property '${property}'`);
        }

        if (!Object.hasOwn(baseObject, property)) {
            throw new Error(`Can't override unknown property '${property}'`);
        }

        const value = baseObject[property];

        if (value === undefined) {
            throw new Error(`Can't override unknown property '${property}'`);
        }

        if (
            typeof value === "object" &&
            value !== null &&
            !Array.isArray(value)
        ) {
            overrideProperties(value, overrideObject[property]);
        } else {
            baseObject[property] = overrideObject[property];
        }
    }
}

export function configureTestSettings(overriddenFeatures) {
    return cy
        .readConfig()
        .as("config")
        .then((config) => {
            const features = config.webClient.featureToggleConfiguration;

            // default all boolean settings to false (except dependent datasets)
            setBooleanProperties(features, false);
            setBooleanProperties(features.dependents.datasets, true);

            // ensure non-overridden datasets and services are populated with default values
            populateFallbackValues(
                overriddenFeatures.datasets,
                features.datasets
            );
            populateFallbackValues(
                overriddenFeatures.dependents?.datasets,
                features.dependents.datasets
            );
            populateFallbackValues(
                overriddenFeatures.services?.services,
                features.services.services
            );

            // apply overrides
            overrideProperties(features, overriddenFeatures);

            // intercept configuration calls to return the modified configuration
            cy.intercept("GET", "**/configuration", {
                statusCode: 200,
                body: config,
            });

            configuredSettings = config;
        });
}

export function readEnvironmentConfig() {
    cy.log(`Reading Environment Configuration`);
    let baseWebClientUrl = Cypress.config("baseUrl");
    if (baseWebClientUrl == localDevUri) {
        baseWebClientUrl = Cypress.env("baseWebClientUrl");
    }

    const diagnostics = startLoginDiagnostics();
    let recordResponse;
    return cy
        .then(() => {
            recordResponse = diagnostics.configurationRequest(
                `${baseWebClientUrl}/configuration`
            );
        })
        .request({
            url: `${baseWebClientUrl}/configuration`,
            failOnStatusCode: false,
        })
        .then((response) => {
            recordResponse(response);
            return response;
        })
        .should((response) => {
            expect(response.status).to.eq(200);
        })
        .its("body");
}
