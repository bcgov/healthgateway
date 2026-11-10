import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { AppText } from "@/components/common/AppText";
import { LiveRegion, textOfChildren } from "@/components/common/LiveRegion";
import { attrOf, render, screen } from "@/test/ui";

describe("LiveRegion", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

    it("web: a polite status region or an assertive alert; iOS, which has no live regions, hears the text", async () => {
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        await render(
            <LiveRegion testID="r">
                <AppText>Saved</AppText>
            </LiveRegion>
        );
        const region = screen.getByTestId("r");
        if (Platform.OS === "web") {
            expect(attrOf(region, "role")).toBe("status");
            expect(attrOf(region, "aria-live")).toBe("polite");
            expect(attrOf(region, "aria-atomic")).toBe("true");
            expect(say).not.toHaveBeenCalled();
        } else {
            // the native Jest project runs as iOS
            expect(say).toHaveBeenCalledWith("Saved");
        }
        await render(
            <LiveRegion
                politeness="assertive"
                testID="r"
                announce="Connection lost"
            >
                <AppText>Offline</AppText>
            </LiveRegion>
        );
        if (Platform.OS === "web") {
            expect(attrOf(screen.getByTestId("r"), "role")).toBe("alert");
            expect(attrOf(screen.getByTestId("r"), "aria-live")).toBe(
                "assertive"
            );
        } else {
            expect(say).toHaveBeenLastCalledWith("Connection lost");
        }
    });

    it("reads the text out of nested elements for the announcement", () => {
        expect(
            textOfChildren(
                <AppText>
                    Displaying <AppText>1</AppText> to {3} out of{" "}
                    <AppText>3</AppText> records
                </AppText>
            )
        ).toBe("Displaying 1 to 3 out of 3 records");
    });
});
