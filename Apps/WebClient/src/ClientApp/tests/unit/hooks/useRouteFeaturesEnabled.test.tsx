import { afterEach, describe, expect, it } from "@jest/globals";

import { Path } from "@/constants/path";
import { useRouteFeaturesEnabled } from "@/hooks/useRouteFeaturesEnabled";
import type { FeatureToggleConfiguration } from "@/models/configData";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import { configFixture, resetStores, updateStores } from "@/test/fixtures";
import { renderHook } from "@/test/ui";

const toggles = configFixture.webClient.featureToggleConfiguration;

function loadConfig(
    dependents: Partial<FeatureToggleConfiguration["dependents"]>
) {
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...toggles,
                        dependents: { ...toggles.dependents, ...dependents },
                    },
                },
            },
            status: "loaded",
        })
    );
}

const enabledAt = async (path: string) => {
    resetRouter(path);
    const { result } = await renderHook(() => useRouteFeaturesEnabled());
    return result.current;
};

describe("useRouteFeaturesEnabled", () => {
    afterEach(() => {
        routerState.focused = true;
        resetRouter();
        resetStores();
    });

    it("allows a gated route whose feature is on", async () => {
        loadConfig({ enabled: true });
        expect(await enabledAt(Path.Dependents)).toBe(true);
    });

    it("holds back a gated route whose feature is off", async () => {
        loadConfig({ enabled: false });
        expect(await enabledAt(Path.Dependents)).toBe(false);
        expect(await enabledAt("/dependents/HDID/timeline")).toBe(false);
    });

    it("judges the dependent timeline by its own feature", async () => {
        loadConfig({ enabled: true, timelineEnabled: false });
        expect(await enabledAt(Path.Dependents)).toBe(true);
        expect(await enabledAt("/dependents/HDID/timeline")).toBe(false);
    });

    it("holds back a gated route until the configuration is loaded", async () => {
        expect(await enabledAt(Path.Services)).toBe(false);
        expect(await enabledAt(Path.Timeline)).toBe(true);
    });

    it("leaves a screen in the background alone: the path is the focused route's", async () => {
        loadConfig({ enabled: false });
        routerState.focused = false;
        expect(await enabledAt(Path.Dependents)).toBe(true);
    });
});
