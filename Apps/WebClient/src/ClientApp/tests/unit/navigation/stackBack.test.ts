import { describe, expect, it, jest } from "@jest/globals";

import {
    outerBack,
    routeBeneath,
    type StackNavigation,
} from "@/navigation/stackBack";

interface FakeRoute {
    key?: string;
    name: string;
    state?: { type?: string; index?: number; routes: FakeRoute[] };
}

/** A navigator: its state, and the one around it. */
function navigator(
    type: string,
    routes: FakeRoute[],
    parent?: StackNavigation,
    index = routes.length - 1
): StackNavigation & { goBack: jest.Mock } {
    return {
        getState: () => ({ type, index, routes }),
        getParent: () => parent,
        goBack: jest.fn(),
    };
}

/** Signed out: the tabs group pushed over the page outside the tabs that opened Security (`RootNavigator`). */
function signedOutOver(page: string, home: FakeRoute[]) {
    const root = navigator("stack", [
        {
            key: "pages",
            name: "(pages)",
            state: { type: "stack", index: 0, routes: [{ name: page }] },
        },
        { key: "tabs", name: "(tabs)" },
    ]);
    const tabs = navigator("tab", [{ key: "home-tab", name: "(home)" }], root);
    const homeStack = navigator("stack", home, tabs);
    return { root, homeStack };
}

describe("stack back", () => {
    describe("outerBack", () => {
        it.each([
            ["index", "Home"],
            ["login", "Log In"],
            ["logoutComplete", "Back"],
            ["not-found", "Back"],
            ["unauthorized", "Back"],
            ["idirLoggedIn", "Back"],
        ])("signed out, over %s: back to it, named %s", (page, label) => {
            const { homeStack } = signedOutOver(page, [
                { key: "s", name: "security" },
            ]);
            expect(outerBack({ key: "s" }, homeStack)?.label).toBe(label);
        });

        it("goes back in the stack that holds the page beneath", () => {
            const { root, homeStack } = signedOutOver("login", [
                { key: "s", name: "security" },
            ]);
            outerBack({ key: "s" }, homeStack)?.goBack();
            expect(root.goBack).toHaveBeenCalledTimes(1);
            expect(homeStack.goBack).not.toHaveBeenCalled();
        });

        it("none for a page above another in its own stack: the platform draws that back button", () => {
            const { homeStack } = signedOutOver("login", [
                { key: "t", name: "termsOfService" },
                { key: "s", name: "security" },
            ]);
            expect(outerBack({ key: "s" }, homeStack)).toBeUndefined();
        });

        it("none with nothing to go back to (signed in, the root keeps only the tabs)", () => {
            const root = navigator("stack", [{ key: "tabs", name: "(tabs)" }]);
            const tabs = navigator(
                "tab",
                [{ key: "home-tab", name: "(home)" }],
                root
            );
            const homeStack = navigator(
                "stack",
                [{ key: "s", name: "security" }],
                tabs
            );
            expect(outerBack({ key: "s" }, homeStack)).toBeUndefined();
        });

        it("none without a navigation or a route key", () => {
            const { homeStack } = signedOutOver("login", [
                { key: "s", name: "security" },
            ]);
            expect(outerBack({ key: "s" }, undefined)).toBeUndefined();
            expect(outerBack({}, homeStack)).toBeUndefined();
        });
    });

    describe("routeBeneath", () => {
        it("the route beneath in the page's own stack", () => {
            const { homeStack } = signedOutOver("login", [
                { key: "t", name: "termsOfService" },
                { key: "s", name: "security" },
            ]);
            expect(routeBeneath({ key: "s" }, homeStack)).toBe(
                "termsOfService"
            );
            expect(routeBeneath({ key: "t" }, homeStack)).toBeUndefined();
        });
    });
});
