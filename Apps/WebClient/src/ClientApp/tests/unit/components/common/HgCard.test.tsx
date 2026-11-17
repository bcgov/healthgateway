import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { AppText, HgCard } from "@/components/common";
import { expectNoAxeViolations } from "@/test/axe";
import {
    attrOf,
    hasShadow,
    press,
    render,
    screen,
    tagOf,
    textOf,
    waitFor,
} from "@/test/ui";

describe("HgCard", () => {
    it("opens on press and shows its title and body", async () => {
        const onPress = jest.fn();
        await render(
            <HgCard title="Medications" onPress={onPress} testID="card">
                <AppText>History since 1995</AppText>
            </HgCard>
        );
        expect(screen.getByText("Medications")).toBeTruthy();
        expect(screen.getByText("History since 1995")).toBeTruthy();
        await press(screen.getByTestId("card"));
        expect(onPress).toHaveBeenCalledTimes(1);
        expect(screen.queryByTestId("card-menu-button")).toBeNull();
    });

    it("offers Remove from the ⋮ menu when removable", async () => {
        const onRemove = jest.fn();
        await render(
            <HgCard
                title="Labs"
                onPress={() => undefined}
                onRemove={onRemove}
                testID="card"
            />
        );
        expect(screen.getByLabelText("Menu")).toBeTruthy();
        // the menu is anchored with measureInWindow, which the native test renderer never answers
        if (Platform.OS !== "web") return;
        await press(screen.getByTestId("card-menu-button"));
        await waitFor(() =>
            expect(screen.getByTestId("remove-quick-link-button")).toBeTruthy()
        );
        await press(screen.getByTestId("remove-quick-link-button"));
        expect(onRemove).toHaveBeenCalledTimes(1);
    });

    it("is an elevated card with no border (HgCardComponent since healthgateway d7ab5c429)", async () => {
        await render(
            <HgCard title="Labs" onPress={() => undefined} testID="card" />
        );
        const card = screen.getByTestId("card");
        expect(hasShadow(card)).toBe(true);
        // NativeWind classes compile away on web; the native test renderer keeps them
        if (Platform.OS === "web") return;
        expect(card.props.className).not.toMatch(/\bborder\b/);
    });

    it("the title is the card's button for the keyboard and the screen reader; the card itself is no tab stop", async () => {
        const onPress = jest.fn();
        await render(
            <HgCard
                title="Medications"
                onPress={onPress}
                onRemove={() => undefined}
                testID="card"
            >
                <AppText>History since 1995</AppText>
            </HgCard>
        );
        const control = screen.getByLabelText("Medications");
        await press(control);
        expect(onPress).toHaveBeenCalledTimes(1);
        if (Platform.OS !== "web") return;
        expect(tagOf(control)).toBe("button");
        expect(attrOf(screen.getByTestId("card"), "tabindex")).toBe("-1");
        const menu = screen.getByTestId("card-menu-button");
        expect(attrOf(menu, "aria-haspopup")).toBe("menu");
        expect(attrOf(menu, "aria-expanded")).toBe("false");
        await expectNoAxeViolations(
            screen.getByTestId("card") as unknown as HTMLElement
        );
    });

    it("shades on hover and press, and wears its button's keyboard focus round the whole card on web", async () => {
        await render(
            <>
                <HgCard
                    title="Labs"
                    onPress={() => undefined}
                    onRemove={() => undefined}
                    testID="card"
                />
                <HgCard
                    compact
                    title="Notes"
                    onPress={() => undefined}
                    onRemove={() => undefined}
                    testID="compact"
                />
            </>
        );
        // NativeWind classes compile away in the web tests (in the browser they reach the DOM); the native test
        // renderer keeps them
        if (Platform.OS === "web") return;
        const menus = screen.getAllByLabelText("Menu");
        expect(menus).toHaveLength(2);
        for (const [id, label] of [
            ["card", "Labs"],
            ["compact", "Notes"],
        ]) {
            const host = screen.getByTestId(id);
            const card = String(host.props.className);
            // global.css draws the ring round the host while its target has keyboard focus
            expect(card).toMatch(/\bhg-card-focus-host\b/);
            expect(card).toMatch(/\bhover:bg-surface-hover\b/);
            expect(card).toMatch(/\bactive:bg-surface-hover\b/);
            const control = screen.getByLabelText(label);
            expect(String(control.props.className)).toMatch(
                /\bhg-card-focus-target\b/
            );
            // :has() only matches a target inside its host
            let node = control.parent;
            while (node && node !== host) node = node.parent;
            expect(node).toBe(host);
            // pointer presses go to the card, which shades
            expect(control.props.pointerEvents).toBe("none");
        }
        // the ⋮ button keeps its own ring
        for (const menu of menus)
            expect(String(menu.props.className)).not.toMatch(
                /\bhg-card-focus-target\b/
            );
    });

    it("a card with no title takes its name from accessibilityLabel", async () => {
        await render(
            <HgCard
                accessibilityLabel="Other record sources"
                onPress={() => undefined}
                testID="card"
            >
                <AppText>Some records may not appear here.</AppText>
            </HgCard>
        );
        expect(screen.getByLabelText("Other record sources")).toBeTruthy();
    });

    it("shows its description as the body of the full card", async () => {
        await render(
            <HgCard
                title="Medications"
                description="History since 1995"
                onPress={() => undefined}
                testID="card"
            />
        );
        expect(screen.getByText("History since 1995")).toBeTruthy();
        expect(
            attrOf(screen.getByLabelText("Medications"), "aria-describedby")
        ).toBeUndefined();
    });
});

describe("HgCard compact (Home's phone tile)", () => {
    const description = "History since 1995";

    it("shows the title and opens on press, from the tile or its button", async () => {
        const onPress = jest.fn();
        await render(
            <HgCard
                compact
                title="Medications"
                description={description}
                onPress={onPress}
                testID="card"
            />
        );
        expect(textOf(screen.getByTestId("card-button-title"))).toBe(
            "Medications"
        );
        await press(screen.getByTestId("card"));
        await press(screen.getByLabelText("Medications"));
        expect(onPress).toHaveBeenCalledTimes(2);
        expect(hasShadow(screen.getByTestId("card"))).toBe(true);
    });

    it("leaves the description off the tile: it describes the button for the screen reader", async () => {
        await render(
            <HgCard
                compact
                title="Medications"
                description={description}
                onPress={() => undefined}
                testID="card"
            />
        );
        const control = screen.getByLabelText("Medications");
        if (Platform.OS === "web") {
            // react-native-web drops accessibilityHint: hidden text describes the button instead
            const describedBy = attrOf(control, "aria-describedby")!;
            expect(document.getElementById(describedBy)?.textContent).toBe(
                description
            );
            expect(tagOf(control)).toBe("button");
            await expectNoAxeViolations(
                screen.getByTestId("card") as unknown as HTMLElement
            );
        } else {
            expect(screen.queryByText(description)).toBeNull();
            expect(control.props.accessibilityHint).toBe(description);
        }
    });

    it("titles a card with no title from its accessibilityLabel", async () => {
        await render(
            <HgCard
                compact
                accessibilityLabel="Health Connect Registry"
                onPress={() => undefined}
                testID="card"
            />
        );
        expect(textOf(screen.getByTestId("card-button-title"))).toBe(
            "Health Connect Registry"
        );
        expect(screen.getByLabelText("Health Connect Registry")).toBeTruthy();
    });

    it("keeps the ⋮ menu with Remove", async () => {
        const onRemove = jest.fn();
        await render(
            <HgCard
                compact
                title="Labs"
                onPress={() => undefined}
                onRemove={onRemove}
                testID="card"
            />
        );
        expect(screen.getByLabelText("Menu")).toBeTruthy();
        // the menu is anchored with measureInWindow, which the native test renderer never answers
        if (Platform.OS !== "web") return;
        await press(screen.getByTestId("card-menu-button"));
        await waitFor(() =>
            expect(screen.getByTestId("remove-quick-link-button")).toBeTruthy()
        );
        await press(screen.getByTestId("remove-quick-link-button"));
        expect(onRemove).toHaveBeenCalledTimes(1);
    });
});
