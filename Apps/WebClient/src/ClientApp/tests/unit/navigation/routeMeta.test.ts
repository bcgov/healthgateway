import { describe, expect, it } from "@jest/globals";

import { Path } from "@/constants/path";
import {
    getRouteMeta,
    routeFeaturesEnabled,
    routeMeta,
} from "@/navigation/routeMeta";
import { UserState } from "@/navigation/userState";
import { configFixture } from "@/test/fixtures";

const toggles = configFixture.webClient.featureToggleConfiguration;

describe("getRouteMeta", () => {
    it("matches exact paths case-insensitively and ignores trailing slashes", () => {
        expect(getRouteMeta("/home")).toBe(routeMeta[Path.Home]);
        expect(getRouteMeta("/HOME/")).toBe(routeMeta[Path.Home]);
        expect(getRouteMeta("/")).toBe(routeMeta[Path.Root]);
    });

    it("matches a dynamic child by its longest parent prefix", () => {
        expect(getRouteMeta("/validateEmail/abc123")).toBe(
            routeMeta[Path.ValidateEmail]
        );
        expect(getRouteMeta("/profile/x")).toBe(routeMeta[Path.Profile]);
    });

    it("returns undefined for unknown routes", () => {
        expect(getRouteMeta("/nope")).toBeUndefined();
    });

    it("keeps every Path covered", () => {
        for (const path of Object.values(Path)) {
            expect(getRouteMeta(path)).toBeDefined();
        }
    });

    it("gates Dependents, Services and Other record sources on their feature toggles", () => {
        expect(
            routeMeta[Path.Dependents].requiredFeaturesEnabled?.(toggles)
        ).toBe(true);
        expect(
            routeMeta[Path.Services].requiredFeaturesEnabled?.(toggles)
        ).toBe(true);
        expect(
            routeMeta[Path.OtherRecordSources].requiredFeaturesEnabled?.(
                toggles
            )
        ).toBe(true);
        const off = {
            ...toggles,
            dependents: { ...toggles.dependents, enabled: false },
            services: { ...toggles.services, enabled: false },
            homepage: {
                ...toggles.homepage,
                otherRecordSources: { enabled: false, sources: [] },
            },
        };
        expect(routeMeta[Path.Dependents].requiredFeaturesEnabled?.(off)).toBe(
            false
        );
        expect(routeMeta[Path.Services].requiredFeaturesEnabled?.(off)).toBe(
            false
        );
        expect(
            routeMeta[Path.OtherRecordSources].requiredFeaturesEnabled?.(off)
        ).toBe(false);
    });

    it("lets a pending-deletion user reach Profile but not Home", () => {
        expect(routeMeta[Path.Profile].validStates).toContain(
            UserState.pendingDeletion
        );
        expect(routeMeta[Path.Home].validStates).not.toContain(
            UserState.pendingDeletion
        );
    });

    it("a dependent's timeline also needs the timeline feature (router/index.ts)", () => {
        const meta = getRouteMeta("/dependents/HDID/timeline");
        const on = { dependents: { enabled: true, timelineEnabled: true } };
        const off = { dependents: { enabled: true, timelineEnabled: false } };
        expect(meta?.requiredFeaturesEnabled?.(on as never)).toBe(true);
        expect(meta?.requiredFeaturesEnabled?.(off as never)).toBe(false);
        expect(getRouteMeta("/dependents/HDID")).toBe(meta);
        expect(
            getRouteMeta("/dependents")?.requiredFeaturesEnabled?.(off as never)
        ).toBe(true);
    });
});

describe("routeFeaturesEnabled", () => {
    const dependentsOff = {
        ...toggles,
        dependents: { ...toggles.dependents, enabled: false },
    };

    it("passes routes without a feature requirement, and unknown ones", () => {
        expect(routeFeaturesEnabled(Path.Timeline, dependentsOff)).toBe(true);
        expect(routeFeaturesEnabled("/does-not-exist", undefined)).toBe(true);
    });

    it("follows a gated route's toggle, dynamic children included", () => {
        expect(routeFeaturesEnabled(Path.Dependents, toggles)).toBe(true);
        expect(routeFeaturesEnabled(Path.Dependents, dependentsOff)).toBe(
            false
        );
        expect(
            routeFeaturesEnabled("/dependents/HDID/timeline", dependentsOff)
        ).toBe(false);
    });

    it("fails a gated route until the configuration is loaded", () => {
        expect(routeFeaturesEnabled(Path.Services, undefined)).toBe(false);
    });
});
