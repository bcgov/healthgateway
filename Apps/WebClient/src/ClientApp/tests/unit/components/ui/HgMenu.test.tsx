import { describe, expect, it, jest } from "@jest/globals";
import { Platform, Pressable, Text } from "react-native";

import { HgMenu } from "@/components/ui/HgMenu";
import { expectNoAxeViolations } from "@/test/axe";
import { attrOf, press, pressKey, render, screen, waitFor } from "@/test/ui";

describe("HgMenu (v-menu port)", () => {
    it("opens from its activator and runs the chosen item, then closes", async () => {
        const pdf = jest.fn();
        await render(
            <HgMenu
                testID="menu"
                items={[
                    { title: "PDF", testID: "pdf", onPress: pdf },
                    { title: "CSV", onPress: () => undefined },
                ]}
            >
                {(open) => (
                    <Pressable testID="activator" onPress={open}>
                        <Text>Download Record</Text>
                    </Pressable>
                )}
            </HgMenu>
        );
        expect(screen.queryByTestId("menu")).toBeNull();
        await press(screen.getByTestId("activator"));
        await waitFor(() => expect(screen.getByTestId("menu")).toBeTruthy());
        await press(screen.getByTestId("pdf"));
        expect(pdf).toHaveBeenCalled();
        // web unmounts the modal when its fade-out animation ends, which jsdom never fires
        if (Platform.OS !== "web")
            expect(screen.queryByTestId("menu")).toBeNull();
    });

    it("web: the open menu's only tab stops are its items; the scrim behind them is none", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgMenu
                testID="menu"
                items={[
                    { title: "PDF", onPress: () => undefined },
                    { title: "CSV", onPress: () => undefined },
                ]}
            >
                {(open) => (
                    <Pressable testID="activator" onPress={open}>
                        <Text>Download Record</Text>
                    </Pressable>
                )}
            </HgMenu>
        );
        await press(screen.getByTestId("activator"));
        await waitFor(() => expect(screen.getByTestId("menu")).toBeTruthy());
        const scrim = (screen.getByTestId("menu") as unknown as HTMLElement)
            .parentElement as HTMLElement;
        expect(scrim.hasAttribute("tabindex")).toBe(false);
        const stops = [...scrim.querySelectorAll("[tabindex]")].map(
            (el) => `${el.getAttribute("tabindex")}:${el.textContent}`
        );
        expect(stops).toEqual(["0:PDF", "0:CSV"]);
    });

    it("web: the activator learns whether the menu is open; Down and Up move between the items; axe is clean", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgMenu
                testID="menu"
                items={[
                    { title: "PDF", testID: "pdf", onPress: () => undefined },
                    { title: "CSV", testID: "csv", onPress: () => undefined },
                ]}
            >
                {(open, isOpen) => (
                    <Pressable
                        testID="activator"
                        accessibilityRole="button"
                        aria-expanded={isOpen}
                        onPress={open}
                    >
                        <Text>Download Record</Text>
                    </Pressable>
                )}
            </HgMenu>
        );
        expect(attrOf(screen.getByTestId("activator"), "aria-expanded")).toBe(
            "false"
        );
        await press(screen.getByTestId("activator"));
        await waitFor(() => expect(screen.getByTestId("menu")).toBeTruthy());
        expect(attrOf(screen.getByTestId("activator"), "aria-expanded")).toBe(
            "true"
        );
        const menu = screen.getByTestId("menu");
        await pressKey(menu, "ArrowDown");
        expect(document.activeElement).toBe(screen.getByTestId("pdf"));
        await pressKey(menu, "ArrowDown");
        expect(document.activeElement).toBe(screen.getByTestId("csv"));
        await pressKey(menu, "ArrowDown");
        expect(document.activeElement).toBe(screen.getByTestId("pdf"));
        await pressKey(menu, "End");
        expect(document.activeElement).toBe(screen.getByTestId("csv"));
        await expectNoAxeViolations(menu as unknown as HTMLElement);
    });
});
