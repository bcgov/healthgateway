import { afterEach, describe, expect, it } from "@jest/globals";
import { act } from "react";
import { Platform, Text } from "react-native";

import { LazyTab } from "@/components/site/LazyTab";
import { Loading } from "@/components/site/Loading";
import { useLoadingStore } from "@/stores/loading.store";
import { useThemeStore } from "@/stores/theme.store";
import { routerState } from "@/test/expoRouterMock";
import { render, screen } from "@/test/ui";

const page = (
    <LazyTab>
        <Text>tab content</Text>
    </LazyTab>
);

/** Subscribes to a store so that a change re-renders it, as focus changes do in the app. */
function Harness() {
    useThemeStore((s) => s.preference);
    return (
        <LazyTab>
            <Text>tab content</Text>
        </LazyTab>
    );
}

const rerenderWith = async (focused: boolean) => {
    routerState.focused = focused;
    await act(async () => {
        await useThemeStore
            .getState()
            .setPreference(
                useThemeStore.getState().preference === "light"
                    ? "dark"
                    : "light"
            );
    });
};

describe("LazyTab", () => {
    afterEach(() => {
        routerState.focused = true;
    });

    it("renders the current tab", async () => {
        await render(page);
        expect(screen.getByText("tab content")).toBeTruthy();
    });

    if (Platform.OS === "web") {
        it("is a pass-through on web", async () => {
            routerState.focused = false;
            await render(page);
            expect(screen.getByText("tab content")).toBeTruthy();
        });
        return;
    }

    it("holds a background tab until it is opened, then keeps it", async () => {
        routerState.focused = false;
        await render(<Harness />);
        expect(screen.queryByText("tab content")).toBeNull();

        await rerenderWith(true);
        expect(screen.getByText("tab content")).toBeTruthy();

        await rerenderWith(false);
        expect(screen.getByText("tab content")).toBeTruthy();
    });

    it("a mounted tab left in the background shows no loading overlay", async () => {
        routerState.focused = false;
        await render(<Loading isLoading />);
        expect(Object.keys(useLoadingStore.getState().requests)).toHaveLength(
            0
        );
    });
});
