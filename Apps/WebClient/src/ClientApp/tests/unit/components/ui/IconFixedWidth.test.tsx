import { describe, expect, it } from "@jest/globals";
import { Platform, StyleSheet } from "react-native";

import { HgAlert } from "@/components/ui/HgAlert";
import { Icon } from "@/components/ui/Icon";
import { render, screen, type UiElement } from "@/test/ui";

/** web: the svg's width, height and viewBox; native: the fixed-width box around the glyph */
function box(el: UiElement) {
    if (Platform.OS === "web") {
        const svg = el as unknown as SVGElement;
        return {
            width: Number(svg.getAttribute("width")),
            height: Number(svg.getAttribute("height")),
            viewBox: svg.getAttribute("viewBox"),
        };
    }
    let node = (el as { parent: UiElement | null }).parent;
    while (node && !(node.props as { style?: unknown }).style)
        node = (node as { parent: UiElement | null }).parent;
    const style = StyleSheet.flatten(
        (node?.props as { style?: never }).style
    ) as { width?: number; height?: number };
    return { width: style.width, height: style.height, viewBox: null };
}

describe("Icon fixedWidth (v-icon's box)", () => {
    it("draws the glyph size tall in a box 1.25 × size wide, the glyph centred", async () => {
        await render(
            <Icon
                name="cloud-arrow-down"
                size={27.5}
                fixedWidth
                testID="icon"
            />
        );
        const b = box(screen.getByTestId("icon"));
        expect(b.width).toBeCloseTo(34.375);
        expect(b.height).toBe(27.5);
        // cloud-arrow-down is 576 by 512: the box is 640 wide, so the glyph starts 32 in
        if (Platform.OS === "web") expect(b.viewBox).toBe("-32 0 640 512");
    });

    it("centres a narrower glyph in the same box", async () => {
        if (Platform.OS !== "web") return; // the native font centres it in the box
        await render(<Icon name="xmark" size={15} fixedWidth testID="icon" />);
        const b = box(screen.getByTestId("icon"));
        expect(b.width).toBeCloseTo(18.75);
        expect(b.height).toBe(15);
        // xmark is 384 by 512: the box is 640 wide, so the glyph starts 128 in
        expect(b.viewBox).toBe("-128 0 640 512");
    });

    it("leaves the square icon as it was without the prop", async () => {
        if (Platform.OS !== "web") return;
        await render(<Icon name="cloud-arrow-down" size={20} testID="icon" />);
        const b = box(screen.getByTestId("icon"));
        expect([b.width, b.height, b.viewBox]).toEqual([20, 20, "0 0 576 512"]);
    });
});

describe("HgAlert icons (v-alert)", () => {
    it("has the 28px prepend icon and a 15px close icon in a 32 by 32 button", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgAlert
                type="info"
                text="Complete your profile"
                closable
                onClose={() => undefined}
                testID="alert"
            />
        );
        const svgs = (
            screen.getByTestId("alert") as unknown as HTMLElement
        ).querySelectorAll("svg");
        expect(svgs[0].getAttribute("height")).toBe("28");
        expect(svgs[0].getAttribute("width")).toBe("35");
        expect(svgs[1].getAttribute("height")).toBe("15");
        const close = screen.getByLabelText("Close") as unknown as HTMLElement;
        expect(close.contains(svgs[1])).toBe(true);
    });
});
