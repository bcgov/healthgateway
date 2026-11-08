import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { AppText } from "@/components/ui/AppText";
import { ExternalLink, NewTabHint } from "@/components/ui/ExternalLink";
import { attrOf, press, render, screen } from "@/test/ui";
import { openExternal } from "@/utils/openExternal";

jest.mock("@/utils/openExternal", () => ({
    openExternal: jest.fn(async () => undefined),
}));

describe("ExternalLink", () => {
    it("flows inside a sentence; web renders a new-tab anchor, native opens the browser", async () => {
        const onPress = jest.fn();
        await render(
            <AppText>
                Visit{" "}
                <ExternalLink href="https://example.test" onPress={onPress}>
                    this page
                </ExternalLink>
                .
            </AppText>
        );
        const link = screen.getByText("this page");
        if (Platform.OS === "web") {
            expect(attrOf(link, "href")).toBe("https://example.test");
            expect(attrOf(link, "target")).toBe("_blank");
        }
        await press(link);
        expect(onPress).toHaveBeenCalled();
        if (Platform.OS === "web") expect(openExternal).not.toHaveBeenCalled();
        else expect(openExternal).toHaveBeenCalledWith("https://example.test");
    });

    it("web: a new-tab link is described by the hidden new-tab hint; a same-window link is not", async () => {
        await render(
            <>
                <NewTabHint />
                <AppText>
                    <ExternalLink href="https://example.test" testID="tab">
                        Support
                    </ExternalLink>
                    <ExternalLink
                        href="https://example.test"
                        sameWindow
                        testID="same"
                    >
                        Portal
                    </ExternalLink>
                </AppText>
            </>
        );
        if (Platform.OS !== "web") {
            expect(screen.queryByText("opens in a new tab")).toBeNull();
            return;
        }
        const tab = screen.getByTestId("tab");
        // the visible text stays the whole link text; the hint is a description
        expect((tab as unknown as HTMLElement).textContent).toBe("Support");
        const hint = document.getElementById(attrOf(tab, "aria-describedby")!);
        expect(hint?.textContent).toBe("opens in a new tab");
        expect(
            attrOf(screen.getByTestId("same"), "aria-describedby")
        ).toBeUndefined();
    });
});
