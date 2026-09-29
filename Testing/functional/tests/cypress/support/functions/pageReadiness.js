import { waitForInitialDataLoad } from "./intercept";

// Preserve existing data readiness until individual scenarios declare their needs.
export function waitForScenarioData(username, settings, path) {
    return waitForInitialDataLoad(username, settings, path);
}
