import { describe, expect, it } from "@jest/globals";
import { Text } from "react-native";

import { PageTitle } from "@/components/common/PageTitle";
import { idOf, render, screen } from "@/test/ui";

describe("PageTitle (PageTitleComponent.vue port)", () => {
    it("the title is #subject (Cypress checkTimelineHasLoaded / contains('#subject', …)) with an append slot", async () => {
        await render(<PageTitle title="Home" append={<Text>Add</Text>} />);
        expect(idOf(screen.getByText("Home"))).toBe("subject");
        expect(screen.getByText("Add")).toBeTruthy();
    });
});
