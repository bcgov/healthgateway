import { afterEach, describe, expect, it } from "@jest/globals";
import { Platform, Pressable } from "react-native";

import { AppText } from "@/components/common/AppText";
import { LoadingOverlay, LoadingScope } from "@/components/site/LoadingOverlay";
import { useLoadingStore } from "@/stores/loading.store";
import { updateStores } from "@/test/fixtures";
import { attrOf, inAct, render, screen } from "@/test/ui";

const page = (
    <>
        <LoadingScope>
            <AppText>Page</AppText>
            <Pressable
                testID="btn"
                accessibilityRole="button"
                onPress={() => undefined}
            >
                <AppText>Go</AppText>
            </Pressable>
        </LoadingScope>
        <LoadingOverlay />
    </>
);

describe("LoadingOverlay", () => {
    afterEach(() =>
        updateStores(() => useLoadingStore.setState({ requests: {} }))
    );

    it("is a status region saying what is loading, or Loading, while a request is registered", async () => {
        await render(page);
        expect(screen.queryByTestId("loadingOverlay")).toBeNull();
        updateStores(() => useLoadingStore.getState().show("a", "Fetching"));
        expect(screen.getByText("Fetching")).toBeTruthy();
        if (Platform.OS === "web")
            expect(attrOf(screen.getByTestId("loadingStatus"), "role")).toBe(
                "status"
            );
        updateStores(() => useLoadingStore.getState().show("b"));
        updateStores(() => useLoadingStore.getState().hide("a"));
        expect(screen.getByText("Loading")).toBeTruthy();
        updateStores(() => useLoadingStore.getState().hide("b"));
        expect(screen.queryByTestId("loadingOverlay")).toBeNull();
    });

    it("web: the page under it is inert and busy while loading, and the focus comes back afterwards", async () => {
        if (Platform.OS !== "web") return;
        await render(page);
        const button = screen.getByTestId("btn") as unknown as HTMLElement;
        const scope = button.closest("[aria-busy]") as HTMLElement;
        expect(scope.getAttribute("aria-busy")).toBe("false");
        expect(scope.hasAttribute("inert")).toBe(false);
        await inAct(() => button.focus());
        expect(document.activeElement).toBe(button);
        updateStores(() => useLoadingStore.getState().show("a"));
        expect(scope.getAttribute("aria-busy")).toBe("true");
        expect(scope.hasAttribute("inert")).toBe(true);
        // a browser drops the focus from an inert subtree; jsdom does not, so do it here
        await inAct(() => button.blur());
        updateStores(() => useLoadingStore.getState().hide("a"));
        expect(scope.hasAttribute("inert")).toBe(false);
        expect(scope.getAttribute("aria-busy")).toBe("false");
        expect(document.activeElement).toBe(button);
    });
});
