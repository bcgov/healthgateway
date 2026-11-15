import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import {
    MENU_DISMISS_MS,
    useHeaderMenu,
} from "@/components/site/useHeaderMenu";
import { Path } from "@/constants/path";
import { Action, Text, Type } from "@/constants/tracking";
import type {
    HeaderMenu,
    HeaderMenuAction,
    HeaderMenuEntry,
} from "@/navigation/headerMenu";
import { services } from "@/services/container";
import { endSessionGeneration } from "@/services/sessionFence";
import { useConfigStore } from "@/stores/config.store";
import { useHeaderMenuStore } from "@/stores/headerMenu.store";
import { useThemeStore } from "@/stores/theme.store";
import { useUserStore } from "@/stores/user.store";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    bcscUserInfo,
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { advanceTimers, inAct, renderHook } from "@/test/ui";

// native only (a .ts test): the web client keeps its own header menus

/** Each section's entries by key. */
const keys = (menu: HeaderMenu) =>
    menu.sections.map((section) => section.map((entry) => entry.key));

const entry = (menu: HeaderMenu, key: string): HeaderMenuEntry => {
    for (const e of menu.sections.flat()) {
        if (e.key === key) return e;
        if (e.type === "submenu") {
            const inner = e.items.find((i) => i.key === key);
            if (inner) return inner;
        }
    }
    throw new Error(`no ${key}`);
};

const action = (menu: HeaderMenu, key: string) =>
    entry(menu, key) as HeaderMenuAction;

describe("useHeaderMenu", () => {
    beforeEach(() => {
        resetRouter(Path.Home);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        jest.useRealTimers();
        resetStores();
        useHeaderMenuStore.setState({ dialog: undefined, bellSeen: false });
        jest.restoreAllMocks();
    });

    describe("signed out", () => {
        it("is the settings menu: Dark Mode, Security and Support, then the version", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            const { menu, bell } = result.current;
            expect(menu.kind).toBe("settings");
            expect(menu.buttonLabel).toBe("Settings");
            expect(menu.title).toBeUndefined();
            expect(bell).toBeUndefined();
            expect(keys(menu)[0]).toEqual([
                "menu-btn-dark-mode-link",
                "menu-btn-security-link",
                "menu-btn-support-link",
            ]);
            const version = menu.sections[1]?.[0] as
                HeaderMenuAction | undefined;
            if (version) {
                expect(version.key).toBe("appVersion");
                expect(version.disabled).toBe(true);
            }
        });

        it("lists every footer link under Support, the email last", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            const support = entry(result.current.menu, "menu-btn-support-link");
            expect(support.type === "submenu" && support.items).toEqual([
                expect.objectContaining({
                    label: "Support Guide",
                    glyph: "external",
                }),
                expect.objectContaining({ label: "Release Notes" }),
                expect.objectContaining({
                    label: "Terms of Service",
                    glyph: "document",
                }),
                expect.objectContaining({ label: "About Us" }),
                expect.objectContaining({
                    key: "footer-email-link",
                    glyph: "email",
                }),
            ]);
        });

        it("opens Security", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "menu-btn-security-link").onPress?.();
            expect(router.push).toHaveBeenCalledWith(Path.Security);
        });

        it("checks the appearance in use and switches to the one picked", async () => {
            useThemeStore.setState({ preference: "system" });
            const { result } = await renderHook(() => useHeaderMenu());
            const menu = result.current.menu;
            expect(action(menu, "dark-mode-system").checked).toBe(true);
            expect(action(menu, "dark-mode-on").checked).toBe(false);
            expect(action(menu, "dark-mode-system").subtitle).toMatch(
                /device's system settings/
            );
            await inAct(() => action(menu, "dark-mode-on").onPress?.());
            expect(useThemeStore.getState().preference).toBe("dark");
        });
    });

    describe("signed in", () => {
        beforeEach(() => signInRegistered());

        it("is the account menu under the initials, titled with the name", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            const { menu } = result.current;
            expect(menu.kind).toBe("account");
            expect(menu.buttonLabel).toBe("Account menu");
            expect(menu.initials).toBe("DG");
            expect(menu.title).toBe("Dr Gateway");
            expect(keys(menu).slice(0, 3)).toEqual([
                [
                    "profileBtn",
                    "menu-btn-dark-mode-link",
                    "menu-btn-security-link",
                ],
                ["menu-btn-support-link", "menu-btn-feedback-link"],
                ["logoutBtn"],
            ]);
            expect(action(menu, "logoutBtn").destructive).toBe(true);
        });

        it("opens Profile, with its Snowplow event", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "profileBtn").onPress?.();
            expect(services.tracking.trackEvent).toHaveBeenCalledWith(
                expect.objectContaining({
                    action: Action.ButtonClick,
                    text: Text.Profile,
                    type: Type.Profile,
                })
            );
            expect(router.push).toHaveBeenCalledWith(Path.Profile);
        });

        it("stays on Profile when it is already open", async () => {
            resetRouter(Path.Profile);
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "profileBtn").onPress?.();
            expect(router.push).not.toHaveBeenCalled();
        });

        it("opens Feedback once the menu has gone", async () => {
            jest.useFakeTimers();
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "menu-btn-feedback-link").onPress?.();
            expect(useHeaderMenuStore.getState().dialog).toBeUndefined();
            await advanceTimers(MENU_DISMISS_MS);
            expect(useHeaderMenuStore.getState().dialog).toBe("feedback");
        });

        it("asks for a rating before logging out", async () => {
            jest.useFakeTimers();
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "logoutBtn").onPress?.();
            await advanceTimers(MENU_DISMISS_MS);
            expect(useHeaderMenuStore.getState().dialog).toBe("rating");
            expect(router.push).not.toHaveBeenCalled();
        });

        it("drops a dialog still waiting for the menu to go when the session ends meanwhile", async () => {
            jest.useFakeTimers();
            const { result } = await renderHook(() => useHeaderMenu());
            action(result.current.menu, "logoutBtn").onPress?.();
            action(result.current.menu, "menu-btn-feedback-link").onPress?.();
            endSessionGeneration();
            await advanceTimers(MENU_DISMISS_MS);
            expect(useHeaderMenuStore.getState().dialog).toBeUndefined();
        });

        it("leaves out Profile and Feedback while the patient cannot be found", async () => {
            updateStores(() =>
                useUserStore.setState({ patientRetrievalFailed: true })
            );
            const { result } = await renderHook(() => useHeaderMenu());
            expect(keys(result.current.menu).slice(0, 3)).toEqual([
                ["menu-btn-dark-mode-link", "menu-btn-security-link"],
                ["menu-btn-support-link"],
                ["logoutBtn"],
            ]);
        });
    });

    describe("signed in with another identity provider", () => {
        beforeEach(() => {
            signInRegistered();
            updateStores(() =>
                useUserStore
                    .getState()
                    .setOidcUserInfo({ ...bcscUserInfo, idp: "IDIR" })
            );
        });

        it("has no Profile, and logs out without asking for a rating", async () => {
            const { result } = await renderHook(() => useHeaderMenu());
            const { menu } = result.current;
            expect(keys(menu)[0]).not.toContain("profileBtn");
            action(menu, "logoutBtn").onPress?.();
            expect(services.tracking.trackEvent).toHaveBeenCalledWith(
                expect.objectContaining({
                    text: Text.Logout,
                    type: Type.Logout,
                })
            );
            expect(router.push).toHaveBeenCalledWith(Path.Logout);
            expect(useHeaderMenuStore.getState().dialog).toBeUndefined();
        });
    });
});
