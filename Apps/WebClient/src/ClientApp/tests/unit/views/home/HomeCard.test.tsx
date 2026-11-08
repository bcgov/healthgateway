import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { AppText } from "@/components/ui";
import { expectNoAxeViolations } from "@/test/axe";
import {
    attrOf,
    hasShadow,
    press,
    render,
    screen,
    tagOf,
    waitFor,
} from "@/test/ui";
import { HomeCard } from "@/views/home/HomeCard";

describe("HomeCard", () => {
    it("opens on press and shows its title and body", async () => {
        const onPress = jest.fn();
        await render(
            <HomeCard title="Medications" onPress={onPress} testID="card">
                <AppText>History since 1995</AppText>
            </HomeCard>
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
            <HomeCard
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
            <HomeCard title="Labs" onPress={() => undefined} testID="card" />
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
            <HomeCard
                title="Medications"
                onPress={onPress}
                onRemove={() => undefined}
                testID="card"
            >
                <AppText>History since 1995</AppText>
            </HomeCard>
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

    it("a card with no title takes its name from accessibilityLabel", async () => {
        await render(
            <HomeCard
                accessibilityLabel="Other record sources"
                onPress={() => undefined}
                testID="card"
            >
                <AppText>Some records may not appear here.</AppText>
            </HomeCard>
        );
        expect(screen.getByLabelText("Other record sources")).toBeTruthy();
    });
});
