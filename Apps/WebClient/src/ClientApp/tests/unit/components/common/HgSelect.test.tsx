import { describe, expect, it, jest } from "@jest/globals";
import { act, fireEvent } from "@testing-library/react";
import { useState } from "react";
import { Platform } from "react-native";

import { HgSelect } from "@/components/common/HgSelect";
import { expectNoAxeViolations } from "@/test/axe";
import { attrOf, press, pressKey, render, screen } from "@/test/ui";

const items = [
    { title: "One", value: "1" },
    { title: "Two", value: "2" },
];

function Multi({ onChange }: { onChange: (v: string[]) => void }) {
    const [value, setValue] = useState<string[]>([]);
    return (
        <HgSelect
            multiple
            testID="s"
            hint="Pick"
            items={items}
            value={value}
            onChange={(v) => {
                setValue(v);
                onChange(v);
            }}
        />
    );
}

describe("HgSelect (v-select)", () => {
    if (Platform.OS === "web") {
        it("web: opens with no backdrop, under its field, and closes on a press outside", async () => {
            await render(
                <HgSelect
                    testID="s"
                    label="Record Type"
                    items={items}
                    value={undefined}
                    onChange={jest.fn()}
                />
            );
            await press(screen.getByTestId("s"));
            const menu = document.querySelector(
                ".v-overlay--active.v-menu"
            ) as HTMLElement;
            expect(menu.querySelectorAll(".v-list-item")).toHaveLength(2);
            expect(menu.parentElement).toBe(document.body);
            // cy.vSelect sends mousedown to the field after opening it
            await act(async () => {
                fireEvent.mouseDown(screen.getByTestId("s") as never);
            });
            expect(screen.getByText("Two")).toBeTruthy();
            await act(async () => {
                fireEvent.mouseDown(document.body);
            });
            expect(screen.queryByText("Two")).toBeNull();
        });
    }

    it("shows the label until a value is chosen, then reports the pick and shows its title", async () => {
        const onChange = jest.fn();
        await render(
            <HgSelect
                testID="s"
                label="Record Type"
                items={items}
                value={undefined}
                onChange={onChange}
            />
        );
        expect(screen.getByText("Record Type")).toBeTruthy();
        await press(screen.getByTestId("s"));
        await press(screen.getByText("Two"));
        expect(onChange).toHaveBeenCalledWith("2");

        await render(
            <HgSelect
                testID="s"
                label="Record Type"
                items={items}
                value="1"
                onChange={onChange}
            />
        );
        expect(screen.getByText("One")).toBeTruthy();
    });

    it("toggles values in a multi-select", async () => {
        const onChange = jest.fn();
        await render(<Multi onChange={onChange} />);
        await press(screen.getByTestId("s"));
        await press(screen.getByText("One"));
        expect(onChange).toHaveBeenLastCalledWith(["1"]);
    });

    it("does not open while disabled", async () => {
        await render(
            <HgSelect
                testID="s"
                hint="Pick"
                disabled
                items={items}
                value={undefined}
                onChange={() => undefined}
            />
        );
        await press(screen.getByTestId("s"));
        expect(screen.queryByText("One")).toBeNull();
    });

    it("web: the field says it opens a list and whether the list is open", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgSelect
                testID="s"
                hint="Pick"
                items={items}
                value={undefined}
                onChange={() => undefined}
            />
        );
        const field = screen.getByTestId("s");
        expect(attrOf(field, "aria-haspopup")).toBe("listbox");
        expect(attrOf(field, "aria-expanded")).toBe("false");
        await press(field);
        expect(attrOf(screen.getByTestId("s"), "aria-expanded")).toBe("true");
    });

    it("web: the list is a listbox of options, the first focused on open, Down moves the focus; axe is clean", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgSelect
                testID="s"
                label="Record Type"
                items={items}
                value="2"
                onChange={() => undefined}
            />
        );
        await press(screen.getByTestId("s"));
        const list = document.querySelector('[role="listbox"]') as HTMLElement;
        expect(list).not.toBeNull();
        const options = [...list.querySelectorAll('[role="option"]')];
        expect(options.map((o) => o.textContent)).toEqual(["One", "Two"]);
        expect(options.map((o) => o.getAttribute("aria-selected"))).toEqual([
            "false",
            "true",
        ]);
        expect(document.activeElement).toBe(options[0]);
        await pressKey(list as never, "ArrowDown");
        expect(document.activeElement).toBe(options[1]);
        await expectNoAxeViolations(list);
    });
});
