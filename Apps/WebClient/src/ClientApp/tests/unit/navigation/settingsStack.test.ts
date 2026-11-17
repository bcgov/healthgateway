import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { render } from "@testing-library/react-native";
import { createElement, type ReactNode } from "react";

import { SettingsPage, WayBackContext } from "@/components/site/SettingsPage";
import { Path } from "@/constants/path";
import type { OuterBack } from "@/navigation/stackBack";
import { resetStores, signInRegistered } from "@/test/fixtures";

// native only (a .ts test): web's layout is a Slot
interface FakeRoute {
    key?: string;
    name: string;
    state?: { index?: number; routes: FakeRoute[] };
}
/** What the hooks report: the navigator's state, this screen's route, and its navigation. */
let mockState: { index?: number; routes: FakeRoute[] } = { routes: [] };
let mockRouteKey = "";
const mockGoBack = jest.fn();
const mockSetOptions = jest.fn();
const mockReplace = jest.fn();
const mockScreens: { name: string; dangerouslySingular?: unknown }[] = [];
let mockWayBack: OuterBack | undefined;

jest.mock("expo-router/react-navigation", () => ({
    useNavigation: () => ({ goBack: mockGoBack, setOptions: mockSetOptions }),
    useRoute: () => ({ key: mockRouteKey }),
    useNavigationState: (select: (state: unknown) => unknown) =>
        select(mockState),
}));
jest.mock("expo-router", () => {
    // reads what the layout hands its pages, as `SettingsPage` does
    function Stack({ children }: { children?: ReactNode }) {
        const { useContext } =
            jest.requireActual<typeof import("react")>("react");
        const page = jest.requireActual<
            typeof import("@/components/site/SettingsPage")
        >("@/components/site/SettingsPage");
        mockWayBack = useContext(page.WayBackContext);
        return children ?? null;
    }
    function Screen(props: { name: string; dangerouslySingular?: unknown }) {
        mockScreens.push(props);
        return null;
    }
    Stack.Screen = Screen;
    return { Stack, useRouter: () => ({ replace: mockReplace }) };
});
jest.mock("@/components/site/useNativeStackOptions", () => ({
    useNativeStackOptions: () => () => ({}),
}));
jest.mock("@/components/site/nativeHeaderItems", () => ({
    useNativeHeaderItems: () => ({
        back: (target: OuterBack) => ({ drawnBack: target }),
    }),
}));

// eslint-disable-next-line import/first
import SettingsStackLayout from "@/app/(settings)/_layout";

const settings = { key: "settings", name: "(settings)" };

/** The root stack with the tabs beneath this stack, showing `tab` and its stack's pages. */
const overTab = (tab: string, pages: string[]) => ({
    index: 1,
    routes: [
        {
            key: "tabs",
            name: "(tabs)",
            state: {
                index: 1,
                routes: [
                    { name: "(home)" },
                    {
                        name: tab,
                        state: {
                            index: pages.length - 1,
                            routes: pages.map((name) => ({ name })),
                        },
                    },
                ],
            },
        },
        settings,
    ],
});

describe("Security and Terms of Service stack", () => {
    beforeEach(() => {
        mockScreens.length = 0;
        mockWayBack = undefined;
        mockGoBack.mockClear();
        mockSetOptions.mockClear();
        mockReplace.mockClear();
    });
    afterEach(() => resetStores());

    describe("the layout: where Back leads", () => {
        const wayBackOver = async (state: typeof mockState) => {
            mockState = state;
            mockRouteKey = "settings";
            await render(createElement(SettingsStackLayout));
            return mockWayBack as OuterBack;
        };

        it("keeps a single Security: opening it again moves the open one up instead of stacking another", async () => {
            await wayBackOver(overTab("reports", ["index"]));
            expect(mockScreens).toContainEqual(
                expect.objectContaining({
                    name: "security",
                    dangerouslySingular: true,
                })
            );
        });

        it.each([
            ["reports", ["index"], "Download"],
            ["services", ["index"], "Services"],
            ["timeline", ["index"], "Health Records"],
            ["(home)", ["home", "profile"], "Profile"],
        ])(
            "over the %s tab: back to the page there, named for it",
            async (tab, pages, label) => {
                const wayBack = await wayBackOver(overTab(tab, pages));
                expect(wayBack.label).toBe(label);
                wayBack.goBack();
                expect(mockGoBack).toHaveBeenCalledTimes(1);
                expect(mockReplace).not.toHaveBeenCalled();
            }
        );

        it("signed out, over the landing page: Log In, back to it", async () => {
            const wayBack = await wayBackOver({
                index: 1,
                routes: [
                    {
                        key: "pages",
                        name: "(pages)",
                        state: { index: 0, routes: [{ name: "index" }] },
                    },
                    settings,
                ],
            });
            expect(wayBack.label).toBe("Log In");
            wayBack.goBack();
            expect(mockGoBack).toHaveBeenCalledTimes(1);
        });

        it("with nothing beneath, signed in: straight to Home, so the landing page doesn't flash", async () => {
            signInRegistered();
            const wayBack = await wayBackOver({ index: 0, routes: [settings] });
            expect(wayBack.label).toBe("Home");
            wayBack.goBack();
            expect(mockReplace).toHaveBeenCalledWith(Path.Home);
            expect(mockGoBack).not.toHaveBeenCalled();
        });

        it("with nothing beneath, signed out: Log In, to the start", async () => {
            const wayBack = await wayBackOver({ index: 0, routes: [settings] });
            expect(wayBack.label).toBe("Log In");
            wayBack.goBack();
            expect(mockReplace).toHaveBeenCalledWith(Path.Root);
        });
    });

    describe("each page: its back button", () => {
        const wayBack: OuterBack = { label: "Download", goBack: jest.fn() };
        const titles = {
            security: "Security",
            termsOfService: "Terms of Service",
        };
        const page = async (routes: FakeRoute[], key: string) => {
            mockState = { index: routes.length - 1, routes };
            mockRouteKey = key;
            await render(
                createElement(
                    WayBackContext.Provider,
                    { value: wayBack },
                    createElement(SettingsPage, { titles })
                )
            );
        };

        it("the first page draws the way back out of the stack", async () => {
            await page([{ key: "s", name: "security" }], "s");
            expect(mockSetOptions).toHaveBeenCalledWith({ drawnBack: wayBack });
        });

        it("one above another keeps the platform's back button, named for the page beneath", async () => {
            await page(
                [
                    { key: "t", name: "termsOfService" },
                    { key: "s", name: "security" },
                ],
                "s"
            );
            expect(mockSetOptions).toHaveBeenCalledWith({
                headerBackTitle: "Terms of Service",
            });
            expect(mockSetOptions).not.toHaveBeenCalledWith({
                drawnBack: wayBack,
            });
        });
    });
});
