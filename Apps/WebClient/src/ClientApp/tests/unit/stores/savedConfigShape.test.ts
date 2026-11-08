import { describe, expect, it } from "@jest/globals";

import { services } from "@/services/container";
import {
    isUsableConfig,
    selectIdleTimeout,
    selectIsOffline,
    selectUpdateRequired,
} from "@/stores/config.store";
import { configFixture, fakeAuth } from "@/test/fixtures";

const withWeb = (web: object) => ({
    ...configFixture,
    webClient: { ...configFixture.webClient, ...web },
});
const withToggles = (toggles: object) =>
    withWeb({
        featureToggleConfiguration: {
            ...configFixture.webClient.featureToggleConfiguration,
            ...toggles,
        },
    });

describe("what counts as a usable saved configuration", () => {
    it("the configuration the app runs on", () => {
        expect(isUsableConfig(configFixture)).toBe(true);
    });

    it.each([
        ["no maintenance window", { offlineMode: undefined }],
        ["a null one", { offlineMode: null }],
        [
            "an open-ended one",
            {
                offlineMode: {
                    startDateTime: "2026-01-01T00:00:00Z",
                    whitelist: [],
                },
            },
        ],
    ])("accepts %s", (_name, web) => {
        expect(isUsableConfig(withWeb(web))).toBe(true);
    });

    it.each([
        ["a window that is not an object", withWeb({ offlineMode: "soon" })],
        [
            "a window with no whitelist",
            withWeb({
                offlineMode: {
                    startDateTime: "2026-01-01T00:00:00Z",
                    whitelist: null,
                },
            }),
        ],
        ["a window with no start", withWeb({ offlineMode: { whitelist: [] } })],
        [
            "a window with a bad end",
            withWeb({
                offlineMode: {
                    startDateTime: "2026-01-01T00:00:00Z",
                    whitelist: [],
                    endDateTime: 5,
                },
            }),
        ],
        ["no timeouts", withWeb({ timeouts: undefined })],
        ["no toggles", withWeb({ featureToggleConfiguration: undefined })],
        ["datasets not a list", withToggles({ datasets: {} })],
        ["no dependents toggle", withToggles({ dependents: undefined })],
        ["no timeline toggle", withToggles({ timeline: undefined })],
        ["no services toggle", withToggles({ services: undefined })],
        [
            "services without a list",
            withToggles({ services: { enabled: true } }),
        ],
    ])("rejects %s", (_name, config) => {
        expect(isUsableConfig(config)).toBe(false);
    });

    it("no notification centre toggle, which a service reads as it is created", () => {
        expect(
            isUsableConfig(withToggles({ notificationCentre: undefined }))
        ).toBe(false);
    });

    /** Every field, removed and then nulled, one at a time: whatever still counts as usable has to start up. */
    const paths = (value: unknown, path: string[] = []): string[][] =>
        typeof value === "object" && value !== null && !Array.isArray(value)
            ? Object.entries(value).flatMap(([key, child]) => [
                  [...path, key],
                  ...paths(child, [...path, key]),
              ])
            : [];
    const without = (path: string[], replacement?: null) => {
        const copy = JSON.parse(JSON.stringify(configFixture)) as Record<
            string,
            unknown
        >;
        let node = copy;
        for (const key of path.slice(0, -1))
            node = node[key] as Record<string, unknown>;
        if (replacement === null) node[path[path.length - 1]] = null;
        else delete node[path[path.length - 1]];
        return copy;
    };
    const startsUp = (config: never) => {
        services.initialize(config, fakeAuth());
        const state = { config } as never;
        selectIsOffline(state);
        selectIdleTimeout(state);
        selectUpdateRequired(state);
    };

    it.each(
        paths(configFixture).map((path) => [path.join("."), path] as const)
    )("without %s, it is rejected or still starts up", (_name, path) => {
        for (const config of [without(path), without(path, null)]) {
            if (isUsableConfig(config))
                expect(() => startsUp(config as never)).not.toThrow();
        }
    });

    it("selecting offline survives a window that has no whitelist", () => {
        const config = withWeb({
            offlineMode: { startDateTime: "2000-01-01T00:00:00Z" },
        });
        expect(() => selectIsOffline({ config } as never)).not.toThrow();
    });
});
