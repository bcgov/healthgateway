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

/** Security or Terms of Service (`(settings)`) pushed on the root stack over `beneath` (`RootNavigator`). */
function settingsOver(beneath: FakeRoute, pages: FakeRoute[]) {
    const root = navigator("stack", [
        beneath,
        { key: "settings", name: "(settings)" },
    ]);
    const settingsStack = navigator("stack", pages, root);
    return { root, settingsStack };
}

/** Signed out, over a page outside the tabs. */
const overPage = (page: string, pages: FakeRoute[]) =>
    settingsOver(
        {
            key: "pages",
            name: "(pages)",
            state: { type: "stack", index: 0, routes: [{ name: page }] },
        },
        pages
    );

/** Signed in, over the tabs, showing `tab` with `stack` as that tab's pages. */
const overTab = (tab: string, stack: FakeRoute[]) =>
    settingsOver(
        {
            key: "tabs",
            name: "(tabs)",
            state: {
                type: "tab",
                index: 1,
                routes: [
                    { name: "(home)" },
                    {
                        name: tab,
                        state: {
                            type: "stack",
                            index: stack.length - 1,
                            routes: stack,
                        },
                    },
                ],
            },
        },
        [{ key: "s", name: "security" }]
    );

const security = [{ key: "s", name: "security" }];

describe("stack back", () => {
    describe("outerBack", () => {
        it.each([
            ["index", "Log In"],
            ["login", "Log In"],
            ["logoutComplete", "Back"],
            ["not-found", "Back"],
            ["unauthorized", "Back"],
            ["idirLoggedIn", "Back"],
        ])("signed out, over %s: back to it, named %s", (page, label) => {
            const { settingsStack } = overPage(page, security);
            expect(outerBack({ key: "s" }, settingsStack)?.label).toBe(label);
        });

        it.each([
            ["(home)", "home", "Home"],
            ["timeline", "index", "Health Records"],
            ["dependents", "index", "Dependents"],
            ["services", "index", "Services"],
            ["reports", "index", "Download"],
        ])(
            "signed in, over the %s tab's first page: back to it, named for the tab",
            (tab, page, label) => {
                const { settingsStack } = overTab(tab, [{ name: page }]);
                expect(outerBack({ key: "s" }, settingsStack)?.label).toBe(
                    label
                );
            }
        );

        it.each([
            ["(home)", "profile", "Profile"],
            ["(home)", "otherRecordSources", "Other record sources"],
            ["dependents", "[id]/timeline", "Health Records"],
        ])(
            "signed in, over a page pushed on the %s tab: back to it, named %s",
            (tab, page, label) => {
                const { settingsStack } = overTab(tab, [
                    { name: tab === "(home)" ? "home" : "index" },
                    { name: page },
                ]);
                expect(outerBack({ key: "s" }, settingsStack)?.label).toBe(
                    label
                );
            }
        );

        it("over a page on a tab's stack with no name of its own: the tab's name", () => {
            const { settingsStack } = overTab("(home)", [
                { name: "home" },
                { name: "somethingNew" },
            ]);
            expect(outerBack({ key: "s" }, settingsStack)?.label).toBe("Home");
        });

        it("over the tabs before they hold any state: Back", () => {
            const { settingsStack } = settingsOver(
                { key: "tabs", name: "(tabs)" },
                security
            );
            expect(outerBack({ key: "s" }, settingsStack)?.label).toBe("Back");
        });

        it("goes back in the stack that holds the page beneath", () => {
            const { root, settingsStack } = overTab("timeline", [
                { name: "index" },
            ]);
            outerBack({ key: "s" }, settingsStack)?.goBack();
            expect(root.goBack).toHaveBeenCalledTimes(1);
            expect(settingsStack.goBack).not.toHaveBeenCalled();
        });

        it("none for a page above another in its own stack: the platform draws that back button", () => {
            const { settingsStack } = overPage("login", [
                { key: "t", name: "termsOfService" },
                { key: "s", name: "security" },
            ]);
            expect(outerBack({ key: "s" }, settingsStack)).toBeUndefined();
        });

        it("none with nothing to go back to (a link that starts the app on Security)", () => {
            const root = navigator("stack", [
                { key: "settings", name: "(settings)" },
            ]);
            const settingsStack = navigator("stack", security, root);
            expect(outerBack({ key: "s" }, settingsStack)).toBeUndefined();
        });

        it("none without a navigation or a route key", () => {
            const { settingsStack } = overPage("login", security);
            expect(outerBack({ key: "s" }, undefined)).toBeUndefined();
            expect(outerBack({}, settingsStack)).toBeUndefined();
        });
    });

    describe("routeBeneath", () => {
        it("the route beneath in the page's own stack", () => {
            const { settingsStack } = overPage("login", [
                { key: "t", name: "termsOfService" },
                { key: "s", name: "security" },
            ]);
            expect(routeBeneath({ key: "s" }, settingsStack)).toBe(
                "termsOfService"
            );
            expect(routeBeneath({ key: "t" }, settingsStack)).toBeUndefined();
        });
    });
});
