import { describe, expect, it, jest } from "@jest/globals";
import { fireEvent } from "@testing-library/react";
import { Platform } from "react-native";

import { CardActions, HgButton } from "@/components/ui/HgButton";
import { useThemeStore } from "@/stores/theme.store";
import { updateStores } from "@/test/fixtures";
import {
    hasShadow,
    isDisabled,
    press,
    render,
    screen,
    settle,
    type UiElement,
} from "@/test/ui";

/** NativeWind classes compile away on web; the native test renderer keeps them on the host */
const classesOf = (el: UiElement) =>
    String((el as { props: { className?: string } }).props.className);

/** web: the prepend icon's box (Vuetify's v-btn__prepend), the svg's parent */
const iconBoxOf = (el: UiElement) =>
    (el as unknown as HTMLElement).querySelector("svg")!.parentElement!.style;

/** web: the overlay's inline style */
const overlayOf = () =>
    (screen.getByTestId("button-overlay") as unknown as HTMLElement).style;

describe("HgButton", () => {
    it("renders its label as a button and calls onPress", async () => {
        const onPress = jest.fn();
        await render(<HgButton text="Log in" onPress={onPress} testID="btn" />);
        await press(screen.getByText("Log in"));
        expect(onPress).toHaveBeenCalledTimes(1);
        expect(screen.getByRole("button")).toBeTruthy();
    });

    it("is disabled while loading and ignores presses", async () => {
        const onPress = jest.fn();
        await render(
            <HgButton
                text="Save"
                icon="plus"
                loading
                onPress={onPress}
                testID="btn"
            />
        );
        await press(screen.getByTestId("btn"));
        expect(onPress).not.toHaveBeenCalled();
        expect(isDisabled(screen.getByTestId("btn"))).toBe(true);
    });

    it("renders every variant", async () => {
        await render(
            <>
                <HgButton text="Primary" />
                <HgButton text="Secondary" variant="secondary" />
                <HgButton text="Link" variant="link" uppercase={false} />
                <HgButton text="Error" variant="error" />
                <HgButton text="Error outline" variant="error-secondary" />
            </>
        );
        for (const t of [
            "Primary",
            "Secondary",
            "Link",
            "Error",
            "Error outline",
        ]) {
            expect(screen.getByText(t)).toBeTruthy();
        }
    });

    it("has v-btn's 16px side padding in either case, and 8px when slim or inside card actions", async () => {
        await render(
            <>
                <HgButton text="Upper" testID="upper" />
                <HgButton text="Mixed" uppercase={false} testID="mixed" />
                <HgButton text="Slim" slim testID="slim" />
                <CardActions>
                    <HgButton text="In actions" testID="actions" />
                    <HgButton text="Not slim" slim={false} testID="not-slim" />
                </CardActions>
            </>
        );
        if (Platform.OS === "web") return;
        expect(classesOf(screen.getByTestId("upper"))).toMatch(/\bpx-4\b/);
        expect(classesOf(screen.getByTestId("mixed"))).toMatch(/\bpx-4\b/);
        expect(classesOf(screen.getByTestId("slim"))).toMatch(/\bpx-2\b/);
        expect(classesOf(screen.getByTestId("actions"))).toMatch(/\bpx-2\b/);
        expect(classesOf(screen.getByTestId("not-slim"))).toMatch(/\bpx-4\b/);
    });

    it("draws a border only on the outlined variants", async () => {
        await render(
            <>
                <HgButton text="P" testID="primary" />
                <HgButton text="E" variant="error" testID="error" />
                <HgButton text="L" variant="link" testID="link" />
                <HgButton text="S" variant="secondary" testID="secondary" />
                <HgButton
                    text="ES"
                    variant="error-secondary"
                    testID="error-secondary"
                />
            </>
        );
        if (Platform.OS === "web") return;
        for (const id of ["primary", "error", "link"])
            expect(classesOf(screen.getByTestId(id))).not.toMatch(/\bborder\b/);
        for (const id of ["secondary", "error-secondary"])
            expect(classesOf(screen.getByTestId(id))).toMatch(/\bborder\b/);
    });

    it("casts the elevated shadow on enabled primary and error buttons only", async () => {
        await render(
            <>
                <HgButton text="P" testID="primary" />
                <HgButton text="E" variant="error" testID="error" />
                <HgButton text="S" variant="secondary" testID="secondary" />
                <HgButton text="L" variant="link" testID="link" />
                <HgButton text="D" disabled testID="disabled" />
            </>
        );
        expect(hasShadow(screen.getByTestId("primary"))).toBe(true);
        expect(hasShadow(screen.getByTestId("error"))).toBe(true);
        expect(hasShadow(screen.getByTestId("secondary"))).toBe(false);
        expect(hasShadow(screen.getByTestId("link"))).toBe(false);
        expect(hasShadow(screen.getByTestId("disabled"))).toBe(false);
    });

    it("puts the icon in Vuetify's 22.5px box, 8px from the label and 4px into the padding unless slim", async () => {
        await render(
            <>
                <HgButton text="Log in" icon="plus" testID="normal" />
                <HgButton text="Log in" icon="plus" slim testID="slim" />
            </>
        );
        if (Platform.OS !== "web") return;
        const normal = iconBoxOf(screen.getByTestId("normal"));
        expect(normal.width).toBe("22.5px");
        expect(normal.marginLeft).toBe("-4px");
        expect(normal.marginRight).toBe("8px");
        expect(iconBoxOf(screen.getByTestId("slim")).marginLeft).toBe("0px");
    });

    it.each([
        // text and outlined variants: 4 % of the label colour, as v-btn__overlay does
        ["light", "link", "0.04"],
        ["light", "secondary", "0.04"],
        // a dark fill in the light theme doubles it (Vuetify's overlay multiplier)
        ["light", "primary", "0.08"],
        ["dark", "link", "0.04"],
        // the dark theme's pale fill doubles it there
        ["dark", "primary", "0.08"],
    ] as const)(
        "web, %s theme: hovering a %s button tints it at %s, and leaving clears it",
        async (scheme, variant, opacity) => {
            if (Platform.OS !== "web") return;
            useThemeStore.setState({ preference: scheme });
            try {
                await render(
                    <HgButton text="Register" variant={variant} testID="btn" />
                );
                const button = screen.getByTestId(
                    "btn"
                ) as unknown as HTMLElement;
                expect(screen.queryByTestId("button-overlay")).toBeNull();
                fireEvent.mouseEnter(button);
                await settle();
                expect(overlayOf().opacity).toBe(opacity);
                fireEvent.mouseLeave(button);
                await settle();
                expect(screen.queryByTestId("button-overlay")).toBeNull();
            } finally {
                updateStores(() =>
                    useThemeStore.setState({ preference: "system" })
                );
            }
        }
    );

    it("web: keyboard focus tints the button at 12 %, over the hover tint, and blur clears it", async () => {
        if (Platform.OS !== "web") return;
        const matches = jest
            .spyOn(Element.prototype, "matches")
            .mockImplementation((selector) => selector === ":focus-visible");
        try {
            await render(
                <HgButton text="Register" variant="link" testID="btn" />
            );
            const button = screen.getByTestId("btn") as unknown as HTMLElement;
            fireEvent.mouseEnter(button);
            fireEvent.focus(button);
            await settle();
            expect(overlayOf().opacity).toBe("0.12");
            fireEvent.blur(button);
            await settle();
            // still hovered
            expect(overlayOf().opacity).toBe("0.04");
            fireEvent.mouseLeave(button);
            await settle();
            expect(screen.queryByTestId("button-overlay")).toBeNull();
        } finally {
            matches.mockRestore();
        }
    });

    it("web: a mouse focus (not :focus-visible) shows no focus tint", async () => {
        if (Platform.OS !== "web") return;
        const matches = jest
            .spyOn(Element.prototype, "matches")
            .mockReturnValue(false);
        try {
            await render(
                <HgButton text="Register" variant="link" testID="btn" />
            );
            fireEvent.focus(
                screen.getByTestId("btn") as unknown as HTMLElement
            );
            await settle();
            expect(screen.queryByTestId("button-overlay")).toBeNull();
        } finally {
            matches.mockRestore();
        }
    });

    it("web: a disabled button shows no tint", async () => {
        if (Platform.OS !== "web") return;
        await render(<HgButton text="Save" disabled testID="btn" />);
        fireEvent.mouseEnter(
            screen.getByTestId("btn") as unknown as HTMLElement
        );
        await settle();
        expect(screen.queryByTestId("button-overlay")).toBeNull();
    });

    it("passes the caller's hover and focus handlers through", async () => {
        const onHoverIn = jest.fn();
        const onFocus = jest.fn();
        await render(
            <HgButton
                text="Register"
                testID="btn"
                onHoverIn={onHoverIn}
                onFocus={onFocus}
            />
        );
        if (Platform.OS !== "web") return;
        const button = screen.getByTestId("btn") as unknown as HTMLElement;
        fireEvent.mouseEnter(button);
        fireEvent.focus(button);
        await settle();
        expect(onHoverIn).toHaveBeenCalledTimes(1);
        expect(onFocus).toHaveBeenCalledTimes(1);
    });
});
