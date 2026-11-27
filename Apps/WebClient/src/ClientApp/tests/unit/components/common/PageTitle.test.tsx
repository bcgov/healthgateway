import { describe, expect, it } from "@jest/globals";
import { Platform, Text } from "react-native";

import { PageTitle } from "@/components/common/PageTitle";
import { idOf, render, screen } from "@/test/ui";

describe("PageTitle (PageTitleComponent.vue port)", () => {
    it("the title is #subject (Cypress checkTimelineHasLoaded / contains('#subject', …)) with an append slot", async () => {
        await render(<PageTitle title="Home" append={<Text>Add</Text>} />);
        expect(idOf(screen.getByText("Home"))).toBe("subject");
        expect(screen.getByText("Add")).toBeTruthy();
    });

    it("a title the native navigation bar shows is left off the page on iOS and Android, kept on web", async () => {
        await render(<PageTitle title="Security" inNavigationBar />);
        if (Platform.OS === "web")
            expect(idOf(screen.getByText("Security"))).toBe("subject");
        else expect(screen.queryByText("Security")).toBeNull();
    });
});
