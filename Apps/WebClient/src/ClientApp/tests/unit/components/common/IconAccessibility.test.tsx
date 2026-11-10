import { describe, expect, it } from "@jest/globals";
import { Platform } from "react-native";

import { Icon } from "@/components/common/Icon";
import { attrOf, render, screen } from "@/test/ui";

describe("Icon accessibility", () => {
    it("is decorative by default: hidden from the screen reader, with no role", async () => {
        await render(<Icon name="bars" size={16} testID="icon" />);
        const icon = screen.getByTestId("icon");
        if (Platform.OS === "web") {
            expect(attrOf(icon, "aria-hidden")).toBe("true");
            expect(attrOf(icon, "role")).toBeUndefined();
        } else {
            expect(icon.props.accessible).toBe(false);
            expect(icon.props.importantForAccessibility).toBe("no");
            expect(icon.props.accessibilityRole).toBeUndefined();
        }
    });

    it("with a label it is an image with that name", async () => {
        await render(
            <Icon
                name="circle-check"
                size={16}
                accessibilityLabel="Verified"
                testID="icon"
            />
        );
        const icon = screen.getByTestId("icon");
        if (Platform.OS === "web") {
            expect(attrOf(icon, "role")).toBe("img");
            expect(attrOf(icon, "aria-label")).toBe("Verified");
            expect(attrOf(icon, "aria-hidden")).toBeUndefined();
        } else {
            expect(icon.props.accessibilityRole).toBe("image");
            expect(icon.props.accessibilityLabel).toBe("Verified");
            expect(icon.props.accessible).toBeUndefined();
        }
    });
});
