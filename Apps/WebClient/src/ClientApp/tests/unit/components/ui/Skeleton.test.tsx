import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { Skeleton } from "@/components/ui/Skeleton";
import { attrOf, render, screen } from "@/test/ui";

describe("Skeleton", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("is a status region that says Loading, instead of a bare label on empty boxes", async () => {
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        await render(<Skeleton testID="skeleton" />);
        const region = screen.getByTestId("skeleton");
        expect(screen.getByText("Loading")).toBeTruthy();
        if (Platform.OS === "web") {
            expect(attrOf(region, "role")).toBe("status");
            expect(
                (region as unknown as HTMLElement).querySelector(
                    "[aria-busy='true']"
                )
            ).not.toBeNull();
        } else {
            expect(say).toHaveBeenCalledWith("Loading");
        }
    });
});
