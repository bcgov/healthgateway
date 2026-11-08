import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import {
    useColorScheme,
    useThemeColors,
    useThemeVars,
    useWebThemeRoot,
} from "@/hooks/useTheme";
import { useThemeStore } from "@/stores/theme.store";
import { updateStores } from "@/test/fixtures";
import { renderHook } from "@/test/ui";
import { palettes, toCssVars } from "@/theme";

// The OS appearance as react-native(-web)'s useColorScheme reports it. Native imports it from "react-native";
// on web babel-preset-expo rewrites that import to react-native-web's per-export module, so mock both.
const mockOsScheme: { value: "light" | "dark" | null } = { value: "light" };
jest.mock("react-native", () => {
    const actual =
        jest.requireActual<typeof import("react-native")>("react-native");
    return new Proxy(actual, {
        get: (target, key) =>
            key === "useColorScheme"
                ? () => mockOsScheme.value
                : (Reflect.get(target, key) as unknown),
    });
});
jest.mock("react-native-web/dist/exports/useColorScheme", () => ({
    __esModule: true,
    default: () => mockOsScheme.value,
}));

const setPreference = (preference: "light" | "dark" | "system") =>
    updateStores(() => useThemeStore.setState({ preference }));

describe("useTheme", () => {
    afterEach(() => {
        setPreference("light");
        mockOsScheme.value = "light";
        jest.restoreAllMocks();
        if (typeof document !== "undefined") {
            document.documentElement.removeAttribute("style");
            document.documentElement.className = "";
        }
    });

    it("uses the stored light or dark preference regardless of the OS", async () => {
        mockOsScheme.value = "dark";
        setPreference("light");
        const hook = await renderHook(() => useColorScheme());
        expect(hook.result.current).toBe("light");
        setPreference("dark");
        await hook.rerender();
        expect(hook.result.current).toBe("dark");
    });

    it.each([
        ["dark", "dark"],
        ["light", "light"],
        [null, "light"],
    ] as const)(
        "system preference follows the OS (%s → %s)",
        async (os, expected) => {
            mockOsScheme.value = os;
            setPreference("system");
            const hook = await renderHook(() => useColorScheme());
            expect(hook.result.current).toBe(expected);
        }
    );

    it("gives the palette and the CSS-variable style for the active scheme", async () => {
        setPreference("dark");
        const colors = await renderHook(() => useThemeColors());
        expect(colors.result.current).toBe(palettes.dark);
        const darkVars = (await renderHook(() => useThemeVars())).result
            .current;
        setPreference("light");
        const lightVars = (await renderHook(() => useThemeVars())).result
            .current;
        // one prebuilt style object per scheme (NativeWind vars() is opaque in Jest, so compare identity)
        expect(lightVars).not.toBe(darkVars);
        expect((await renderHook(() => useThemeVars())).result.current).toBe(
            lightVars
        );
    });

    it("web: puts the palette variables, color-scheme and the dark class on <html>; native: does nothing", async () => {
        await renderHook(() => useWebThemeRoot("dark"));
        if (Platform.OS !== "web") {
            expect(
                typeof document === "undefined" ||
                    document.documentElement.style.length === 0
            ).toBe(true);
            return;
        }
        const root = document.documentElement;
        const [name, value] = Object.entries(toCssVars(palettes.dark))[0];
        expect(root.style.getPropertyValue(name)).toBe(value);
        expect(root.style.colorScheme).toBe("dark");
        expect(root.classList.contains("dark")).toBe(true);
        await renderHook(() => useWebThemeRoot("light"));
        expect(root.style.getPropertyValue(name)).toBe(
            toCssVars(palettes.light)[name]
        );
        expect(root.classList.contains("dark")).toBe(false);
    });
});
