import { describe, expect, it, jest } from "@jest/globals";
import { within } from "@testing-library/react-native";
import { createElement, type ReactNode } from "react";
import { Text, View } from "react-native";

import { AppShell } from "@/components/site/AppShell";
import { PageScroll } from "@/components/site/PageScroll";
import { render, screen } from "@/test/ui";

jest.mock("react-native-safe-area-context", () => {
    const actual = jest.requireActual<
        typeof import("react-native-safe-area-context")
    >("react-native-safe-area-context");
    const react = jest.requireActual<typeof import("react")>("react");
    const native =
        jest.requireActual<typeof import("react-native")>("react-native");
    return {
        ...actual,
        SafeAreaView: ({
            edges,
            children,
        }: {
            edges: string[];
            children: ReactNode;
        }) =>
            react.createElement(
                native.View,
                { testID: `safe-${edges.join("-")}` },
                children
            ),
    };
});
jest.mock("@/components/site/DevBanner", () => ({ DevBanner: () => null }));
jest.mock("@/components/site/Header", () => {
    const react = jest.requireActual<typeof import("react")>("react");
    const native =
        jest.requireActual<typeof import("react-native")>("react-native");
    return {
        Header: () =>
            react.createElement(native.Text, { testID: "header" }, "header"),
    };
});
jest.mock("@/components/site/SessionBanner", () => ({
    SessionBanner: () => null,
}));
jest.mock("@/components/site/IdleTimeout", () => ({ IdleTimeout: () => null }));

describe("iPhone Duo's vertical rail", () => {
    const shell = () =>
        render(
            createElement(
                AppShell,
                null,
                createElement(View, { testID: "routes" })
            )
        );

    it("the routes reach the sides, so the tab bar can move into the rail", async () => {
        await shell();
        expect(
            within(screen.getByTestId("safe-top")).getByTestId("routes")
        ).toBeTruthy();
        for (const padded of screen.getAllByTestId("safe-left-right"))
            expect(within(padded).queryByTestId("routes")).toBeNull();
    });

    it("the shell's own bars stay clear of it", async () => {
        await shell();
        expect(
            screen
                .getAllByTestId("safe-left-right")
                .some((v) => within(v).queryByTestId("header") !== null)
        ).toBe(true);
    });

    it("each page's content stays clear of it", async () => {
        await render(
            createElement(
                PageScroll,
                null,
                createElement(Text, { testID: "content" }, "content")
            )
        );
        expect(
            within(screen.getByTestId("safe-left-right")).getByTestId("content")
        ).toBeTruthy();
    });
});
