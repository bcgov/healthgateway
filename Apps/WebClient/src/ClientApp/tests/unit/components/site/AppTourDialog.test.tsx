import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { AppTourDialog } from "@/components/site/AppTourDialog";
import { Path } from "@/constants/path";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import { updateStores } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("AppTourDialog", () => {
    if (Platform.OS !== "web") {
        it("is web only", () => undefined);
        return;
    }

    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter(Path.Home);
        globalThis.history.replaceState(null, "", "/home?registration=success");
    });
    afterEach(() => {
        routerState.params = {};
        globalThis.history.replaceState(null, "", "/");
    });

    it("stays closed without the registration query", async () => {
        await render(<AppTourDialog />);
        expect(screen.queryByTestId("app-tour-modal")).toBeNull();
        expect(globalThis.location.search).toBe("?registration=success");
    });

    it("opens after registration, clears the query and has a Got it! button", async () => {
        routerState.params = { registration: "success" };
        await render(<AppTourDialog />);
        expect(screen.getByText("Thank you for registering!")).toBeTruthy();
        expect(globalThis.location.search).toBe("");
        // closing is checked by Cypress: react-native-web keeps a modal until its animation ends, which jsdom never does
        await press(screen.getByTestId("app-tour-done"));
    });

    it("closes on Escape, as the web client's @keydown.esc does", async () => {
        routerState.params = { registration: "success" };
        await render(<AppTourDialog />);
        // react-native-web makes a modal active (so Escape reaches it) when its fade-in ends, and removes it when its
        // fade-out ends; jsdom never animates, so end the animation by hand on the modal's ancestors
        const endAnimation = () =>
            updateStores(() => {
                for (
                    let el =
                        document.querySelector("[aria-modal]")?.parentElement;
                    el && el !== document.body;
                    el = el.parentElement
                )
                    el.dispatchEvent(
                        new Event("animationend", { bubbles: true })
                    );
            });
        endAnimation();
        expect(document.querySelector('[role="dialog"]')).not.toBeNull();
        updateStores(() =>
            document.dispatchEvent(
                new KeyboardEvent("keyup", { key: "Escape" })
            )
        );
        endAnimation();
        expect(screen.queryByTestId("app-tour-modal")).toBeNull();
    });
});
