import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { HgPagination, paginationRange } from "@/components/ui/HgPagination";
import { expectNoAxeViolations } from "@/test/axe";
import { press, render, screen } from "@/test/ui";

describe("paginationRange (Vuetify VPagination getRange)", () => {
    it.each([
        [3, 1, 8, [1, 2, 3]],
        [20, 1, 8, [1, 2, 3, 4, 5, 6, 7, "...", 20]],
        [20, 10, 8, [1, "...", 8, 9, 10, 11, 12, 13, "...", 20]],
        [20, 19, 8, [1, "...", 14, 15, 16, 17, 18, 19, 20]],
        [10, 5, 3, [1, "...", 5, "...", 10]],
        [10, 1, 3, [1, 2, "...", 10]],
        [10, 10, 3, [1, "...", 9, 10]],
    ])("length %d, page %d, %d visible", (length, page, visible, expected) => {
        expect(paginationRange(length, page, visible)).toEqual(expected);
    });
});

describe("HgPagination (v-pagination port)", () => {
    it("previous / pages / next change the page; ends are disabled", async () => {
        const onChange = jest.fn();
        await render(
            <HgPagination
                testID="pagination"
                length={2}
                page={1}
                onChange={onChange}
            />
        );
        await press(screen.getByLabelText("Next page"));
        expect(onChange).toHaveBeenCalledWith(2);
        await press(screen.getByLabelText("Go to page 2"));
        expect(onChange).toHaveBeenLastCalledWith(2);
        expect(screen.getByLabelText("Page 1, Current page")).toBeTruthy();
        await press(screen.getByLabelText("Previous page"));
        expect(onChange).toHaveBeenCalledTimes(2);
        if (Platform.OS === "web") {
            // Cypress: [data-testid=pagination] [data-icon=chevron-right]
            const nav = screen.getByTestId(
                "pagination"
            ) as unknown as HTMLElement;
            expect(
                nav.querySelector("[data-icon=chevron-right]")
            ).not.toBeNull();
        }
    });

    it("web: the ellipses are not buttons, so every button has a name", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgPagination
                length={20}
                page={10}
                onChange={() => undefined}
                testID="pages"
            />
        );
        const nav = screen.getByTestId("pages") as unknown as HTMLElement;
        const unnamed = [...nav.querySelectorAll("button")].filter(
            (button) => !button.getAttribute("aria-label")
        );
        expect(unnamed).toEqual([]);
        expect(nav.textContent).toContain("...");
        await expectNoAxeViolations(nav);
    });
});
