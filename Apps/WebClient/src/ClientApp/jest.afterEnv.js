/* global beforeEach, afterEach */
// A React act() warning means a state update escaped the test's control (a promise or timer finishing after
// the assertions). Fail the test that caused it instead of printing it, so these cannot pile up unnoticed.
const { format } = require("util");

const actWarnings = [];
const originalError = console.error;

beforeEach(() => {
    actWarnings.length = 0;
    console.error = (...args) => {
        // React passes the component name as a %s argument
        const text = format(...args);
        if (text.includes("not wrapped in act")) {
            actWarnings.push(text.split("\n")[0]);
            return;
        }
        originalError(...args);
    };
});

afterEach(() => {
    console.error = originalError;
    if (actWarnings.length > 0) {
        const list = [...new Set(actWarnings)].join("\n  ");
        actWarnings.length = 0;
        throw new Error(
            `React act() warning(s) during this test:\n  ${list}\nAwait the helpers in @/test/ui, use advanceTimers for timers, and updateStores for store changes.`
        );
    }
});
