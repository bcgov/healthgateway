import { afterEach, beforeEach, describe, expect, it } from "@jest/globals";
import type {
    NativeStackHeaderItem,
    NativeStackHeaderItemMenu,
    NativeStackHeaderItemMenuAction,
    NativeStackHeaderItemMenuSubmenu,
    NativeStackNavigationOptions,
} from "expo-router/native-stack";

// the Jest native project is iOS: this is `nativeHeaderItems.ios.tsx`
import { useNativeHeaderItems } from "@/components/site/nativeHeaderItems";
import { useNativeStackOptions } from "@/components/site/useNativeStackOptions";
import { Path } from "@/constants/path";
import { useConfigStore } from "@/stores/config.store";
import { usePageRefreshStore } from "@/stores/pageRefresh.store";
import { resetRouter } from "@/test/expoRouterMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { renderHook } from "@/test/ui";

// native only (a .ts test): web has no native navigation bar

const rightItems = (options: NativeStackNavigationOptions) =>
    options.unstable_headerRightItems?.({}) ?? [];

const menuOf = (items: NativeStackHeaderItem[]) =>
    items.find((i) => i.type === "menu") as NativeStackHeaderItemMenu;

/** The UIMenu's inline sections, each its items' labels. */
const sectionLabels = (menu: NativeStackHeaderItemMenu) =>
    menu.menu.items.map((section) =>
        (section as NativeStackHeaderItemMenuSubmenu).items.map((i) => i.label)
    );

const find = (menu: NativeStackHeaderItemMenu, label: string) =>
    menu.menu.items
        .flatMap((s) => (s as NativeStackHeaderItemMenuSubmenu).items)
        .find((i) => i.label === label);

describe("native navigation bar (iOS)", () => {
    beforeEach(() => {
        resetRouter(Path.Home);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
    });
    afterEach(() => {
        updateStores(() =>
            usePageRefreshStore.setState({
                refresh: undefined,
                refreshing: false,
            })
        );
        resetStores();
    });

    describe("items", () => {
        it("signed out: a gear opening the settings menu", async () => {
            const { result } = await renderHook(() => useNativeHeaderItems());
            const menu = menuOf(rightItems(result.current.menu));
            expect(menu.accessibilityLabel).toBe("Settings");
            expect(menu.icon).toEqual({ type: "sfSymbol", name: "gearshape" });
            expect(menu.menu.title).toBeUndefined();
            expect(sectionLabels(menu)[0]).toEqual([
                "Dark Mode",
                "Security",
                "Support",
            ]);
            for (const section of menu.menu.items)
                expect(section).toEqual(
                    expect.objectContaining({ type: "submenu", inline: true })
                );
        });

        it("signed in: the initials, opening the account menu titled with the name", async () => {
            signInRegistered();
            const { result } = await renderHook(() => useNativeHeaderItems());
            const menu = menuOf(rightItems(result.current.menu));
            expect(menu.label).toBe("DG");
            expect(menu.icon).toBeUndefined();
            expect(menu.accessibilityLabel).toBe("Account menu");
            expect(menu.menu.title).toBe("Dr Gateway");
            expect(sectionLabels(menu).slice(0, 3)).toEqual([
                ["Profile", "Dark Mode", "Security"],
                ["Support", "Feedback"],
                ["Log Out"],
            ]);
            expect(find(menu, "Log Out")).toEqual(
                expect.objectContaining({ destructive: true })
            );
        });

        it("Dark Mode is a one-choice submenu with the current appearance checked", async () => {
            const { result } = await renderHook(() => useNativeHeaderItems());
            const darkMode = find(
                menuOf(rightItems(result.current.menu)),
                "Dark Mode"
            ) as NativeStackHeaderItemMenuSubmenu;
            expect(darkMode.multiselectable).toBe(false);
            const choices = darkMode.items as NativeStackHeaderItemMenuAction[];
            expect(choices.map((c) => [c.label, c.state])).toEqual([
                ["On", "off"],
                ["Off", "on"],
                ["System", "off"],
            ]);
        });

        it("Refresh sits left of the menu on a page with pull to refresh, waiting while one runs", async () => {
            const refresh = () => undefined;
            usePageRefreshStore.setState({ refresh, refreshing: true });
            const { result } = await renderHook(() => useNativeHeaderItems());
            const items = rightItems(result.current.menu);
            expect(items.map((i) => i.type)).toEqual(["button", "menu"]);
            expect(items[0]).toEqual(
                expect.objectContaining({
                    accessibilityLabel: "Refresh",
                    icon: { type: "sfSymbol", name: "arrow.clockwise" },
                    disabled: true,
                    onPress: refresh,
                })
            );
        });

        it("no Refresh on a page without pull to refresh", async () => {
            const { result } = await renderHook(() => useNativeHeaderItems());
            expect(rightItems(result.current.menu).map((i) => i.type)).toEqual([
                "menu",
            ]);
        });

        it("the logo sits on the bar without the shared glass background", async () => {
            const { result } = await renderHook(() => useNativeHeaderItems());
            expect(result.current.logo.unstable_headerLeftItems?.({})).toEqual([
                expect.objectContaining({
                    type: "custom",
                    hidesSharedBackground: true,
                }),
            ]);
        });
    });

    describe("stack options", () => {
        const options = async (
            name: string,
            args: Parameters<typeof useNativeStackOptions>[0]
        ) => {
            const { result } = await renderHook(() =>
                useNativeStackOptions(args)
            );
            return result.current({ route: { name } });
        };

        it("a stack's first page: the logo, no title, no back, the menu", async () => {
            const o = await options("home", { roots: ["home"] });
            expect(o.headerShown).toBe(true);
            expect(o.unstable_headerLeftItems).toBeDefined();
            expect(o.headerBackVisible).toBe(false);
            expect(o.title).toBe("");
            expect(rightItems(o)).not.toHaveLength(0);
        });

        it("a page above it: back to the first page, its own title, the menu", async () => {
            const o = await options("profile", {
                roots: ["home"],
                titles: { profile: "Profile" },
                backTitle: "Home",
            });
            expect(o.unstable_headerLeftItems).toBeUndefined();
            expect(o.title).toBe("Profile");
            expect(o.headerBackTitle).toBe("Home");
            expect(rightItems(o)).not.toHaveLength(0);
        });

        it("a page the menu opens has no menu: back is the way out", async () => {
            const o = await options("profile", {
                roots: ["home"],
                withoutMenu: ["profile"],
            });
            expect(o.unstable_headerRightItems).toBeUndefined();
            expect(o.title).toBe("");
        });

        it("without roots every page is a first page (the pages outside the tabs)", async () => {
            const o = await options("registration", {});
            expect(o.unstable_headerLeftItems).toBeDefined();
        });

        it("a hidden route has no bar", async () => {
            const o = await options("loginCallback", {
                hidden: ["loginCallback"],
            });
            expect(o).toEqual({ headerShown: false });
        });

        it("the bar sits below the shell's banners without the status bar's height again (Android)", async () => {
            const o = await options("home", { roots: ["home"] });
            expect(o.unstable_nativeProps?.headerConfig).toEqual(
                expect.objectContaining({ disableTopInsetApplication: true })
            );
        });
    });
});
