import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { render } from "@testing-library/react-native";
import { createElement, type ReactNode } from "react";

import { resetStores, signInRegistered } from "@/test/fixtures";

// native only (a .ts test): web's Home layout is a Slot
const screens: { name: string; dangerouslySingular?: unknown }[] = [];
jest.mock("expo-router", () => {
    const Stack = ({ children }: { children?: ReactNode }) => children ?? null;
    function Screen(props: { name: string; dangerouslySingular?: unknown }) {
        screens.push(props);
        return null;
    }
    Stack.Screen = Screen;
    return { Stack };
});
// the navigation bar's options are their own test: here, only what the Home stack asks of them
const stackOptions: { withoutMenu?: readonly string[] }[] = [];
jest.mock("@/components/site/useNativeStackOptions", () => ({
    useNativeStackOptions: (args: { withoutMenu?: readonly string[] }) => {
        stackOptions.push(args);
        return () => ({});
    },
}));

// eslint-disable-next-line import/first
import HomeStackLayout from "@/app/(tabs)/(home)/_layout";

const lastWithoutMenu = () =>
    stackOptions[stackOptions.length - 1]?.withoutMenu;

describe("Home tab stack", () => {
    afterEach(() => {
        stackOptions.length = 0;
        resetStores();
    });

    it("holds no Security or Terms of Service: they go over whichever tab opened them", async () => {
        await render(createElement(HomeStackLayout));
        expect(screens.map((s) => s.name)).not.toContain("security");
        expect(screens.map((s) => s.name)).not.toContain("termsOfService");
    });

    it("leaves the account menu off Profile while Back leads somewhere", async () => {
        signInRegistered();
        await render(createElement(HomeStackLayout));
        expect(lastWithoutMenu()).toEqual(["profile"]);
    });

    it("keeps the account menu on Profile for an account pending deletion: its Log Out is the only way out", async () => {
        signInRegistered({ closedDateTime: "2026-11-01T00:00:00Z" });
        await render(createElement(HomeStackLayout));
        expect(lastWithoutMenu()).toEqual([]);
    });
});
