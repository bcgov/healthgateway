import { afterEach, describe, expect, it } from "@jest/globals";
import { useEffect } from "react";
import { Text } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { Path } from "@/constants/path";
import { useConfigStore } from "@/stores/config.store";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import { configFixture, resetStores, updateStores } from "@/test/fixtures";
import { render, screen } from "@/test/ui";

const toggles = configFixture.webClient.featureToggleConfiguration;

function loadConfig(dependentsEnabled: boolean) {
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...toggles,
                        dependents: {
                            ...toggles.dependents,
                            enabled: dependentsEnabled,
                        },
                    },
                },
            },
            status: "loaded",
        })
    );
}

/** Stands in for a view that asks for its data when it mounts, as DependentsView does. */
let requests: string[] = [];
function FetchingPage() {
    useEffect(() => {
        requests.push("dependents");
    }, []);
    return <Text>dependents page</Text>;
}

const page = (
    <PageScroll>
        <FetchingPage />
    </PageScroll>
);

describe("PageScroll route feature gate", () => {
    afterEach(() => {
        requests = [];
        routerState.focused = true;
        resetRouter();
        resetStores();
    });

    it("shows the page when the route's feature is on", async () => {
        loadConfig(true);
        resetRouter(Path.Dependents);
        await render(page);
        expect(screen.getByText("dependents page")).toBeTruthy();
        expect(requests).toEqual(["dependents"]);
    });

    it("holds the page back when the route's feature is off, so it never asks for its data", async () => {
        loadConfig(false);
        resetRouter(Path.Dependents);
        await render(page);
        expect(screen.queryByText("dependents page")).toBeNull();
        expect(requests).toEqual([]);
    });

    it("shows pages on routes without a feature requirement", async () => {
        loadConfig(false);
        resetRouter(Path.Timeline);
        await render(page);
        expect(screen.getByText("dependents page")).toBeTruthy();
    });

    it("keeps a background screen's content whatever the focused route is", async () => {
        loadConfig(false);
        resetRouter(Path.Dependents);
        routerState.focused = false;
        await render(page);
        expect(screen.getByText("dependents page")).toBeTruthy();
    });
});
