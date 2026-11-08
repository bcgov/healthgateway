import { describe, expect, it, jest } from "@jest/globals";
import { AccessibilityInfo, Platform, Text } from "react-native";

import { HgDataTable } from "@/components/ui/HgDataTable";
import type { ReportField } from "@/models/report";
import { attrOf, render, screen, textOf } from "@/test/ui";

const fields: ReportField[] = [
    { key: "name", title: "Name", tdTestID: "nameItem", thTestID: "nameTitle" },
    { key: "due", title: "Due", tdTestID: "dueItem" },
];
const items = [
    { name: "Tetanus", due: "2025-Jan-01" },
    { name: "Shingles", due: "2027-Jun-30" },
];

describe("HgDataTable (HgDataTableComponent.vue port)", () => {
    it("renders a header and one row per item with the field test ids", async () => {
        await render(<HgDataTable testID="t" items={items} fields={fields} />);
        expect(textOf(screen.getByTestId("nameTitle"))).toBe("Name");
        const cells = screen.getAllByTestId("nameItem");
        expect(cells.map(textOf)).toEqual(["Tetanus", "Shingles"]);
        if (Platform.OS === "web") {
            // Cypress: [data-testid=t] tbody tr
            const table = screen.getByTestId("t") as unknown as HTMLElement;
            expect(table.querySelectorAll("tbody tr")).toHaveLength(2);
            expect(table.querySelectorAll("thead th")).toHaveLength(2);
        }
    });

    it("header and item slots replace the default cell content", async () => {
        await render(
            <HgDataTable
                items={items}
                fields={fields}
                renderHeader={{ due: () => <Text>When</Text> }}
                renderItem={{
                    due: (item, i) => <Text>{`${i}:${item.due}`}</Text>,
                }}
            />
        );
        expect(screen.getByText("When")).toBeTruthy();
        expect(screen.getByText("1:2027-Jun-30")).toBeTruthy();
    });

    it("shows the skeleton while loading", async () => {
        await render(<HgDataTable loading items={items} fields={fields} />);
        expect(screen.getByTestId("table-skeleton-loader")).toBeTruthy();
        expect(screen.queryByText("Tetanus")).toBeNull();
    });

    it("the loading skeleton is a status region that says Loading", async () => {
        const say = jest
            .spyOn(AccessibilityInfo, "announceForAccessibility")
            .mockImplementation(() => undefined);
        await render(<HgDataTable loading items={items} fields={fields} />);
        const region = screen.getByTestId("table-skeleton-loader");
        expect(screen.getByText("Loading")).toBeTruthy();
        if (Platform.OS === "web") {
            expect(attrOf(region, "role")).toBe("status");
        } else {
            expect(say).toHaveBeenCalledWith("Loading");
        }
        say.mockRestore();
    });

    it("names the table for the screen reader; native cells carry their column name and the header cells are headings", async () => {
        await render(
            <HgDataTable
                testID="t"
                caption="Immunization schedule"
                items={items}
                fields={fields}
            />
        );
        if (Platform.OS === "web") {
            const table = screen.getByTestId("t") as unknown as HTMLElement;
            expect(table.querySelector("caption")?.textContent).toBe(
                "Immunization schedule"
            );
            return;
        }
        expect(screen.getByText("Immunization schedule")).toBeTruthy();
        expect(screen.getByLabelText("Name: Tetanus")).toBeTruthy();
        expect(screen.getByLabelText("Due: 2027-Jun-30")).toBeTruthy();
        expect(screen.getByText("Name").props.accessibilityRole as string).toBe(
            "header"
        );
    });
});
