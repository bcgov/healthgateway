import { describe, expect, it, jest } from "@jest/globals";
import { render } from "@testing-library/react-native";
import { createElement, type ReactNode } from "react";

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

// eslint-disable-next-line import/first
import HomeStackLayout from "@/app/(tabs)/(home)/_layout";

describe("Home tab stack", () => {
    it("keeps a single Security: opening it again moves the open one up instead of stacking another", async () => {
        await render(createElement(HomeStackLayout));
        expect(screens).toContainEqual(
            expect.objectContaining({
                name: "security",
                dangerouslySingular: true,
            })
        );
    });
});
