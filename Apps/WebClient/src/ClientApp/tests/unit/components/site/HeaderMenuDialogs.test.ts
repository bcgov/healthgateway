import { afterEach, describe, expect, it } from "@jest/globals";
import { createElement } from "react";

import { HeaderMenuDialogs } from "@/components/site/HeaderMenuDialogs";
import { useAuthStore } from "@/stores/auth.store";
import { useHeaderMenuStore } from "@/stores/headerMenu.store";
import { resetStores, signInRegistered, updateStores } from "@/test/fixtures";
import { render, screen } from "@/test/ui";

// native only (a .ts test): the web client's header keeps its own dialogs

describe("HeaderMenuDialogs", () => {
    afterEach(() => {
        updateStores(() =>
            useHeaderMenuStore.setState({ dialog: undefined, bellSeen: false })
        );
        resetStores();
    });

    it("shows the dialog the menu opened", async () => {
        signInRegistered();
        useHeaderMenuStore.setState({ dialog: "feedback" });
        await render(createElement(HeaderMenuDialogs));
        expect(screen.getByTestId("feedback-container")).toBeTruthy();
    });

    it("closes and drops the dialogs when the session ends, so nothing carries into the next sign-in", async () => {
        signInRegistered();
        useHeaderMenuStore.setState({ dialog: "feedback", bellSeen: true });
        await render(createElement(HeaderMenuDialogs));
        updateStores(() => useAuthStore.setState({ isAuthenticated: false }));
        expect(screen.queryByTestId("feedback-container")).toBeNull();
        expect(useHeaderMenuStore.getState()).toEqual(
            expect.objectContaining({ dialog: undefined, bellSeen: false })
        );
    });
});
