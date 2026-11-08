import { describe, expect, it, jest } from "@jest/globals";
import { useState } from "react";
import { Platform } from "react-native";

import { AppModal } from "@/components/ui/AppModal";
import { HgDatePicker } from "@/components/ui/HgDatePicker";
import { updateStores } from "@/test/fixtures";
import { changeText, idOf, inputValue, press, render, screen } from "@/test/ui";

const escapeKeyup = () =>
    updateStores(() =>
        document.dispatchEvent(new KeyboardEvent("keyup", { key: "Escape" }))
    );

describe("HgDatePicker (HgDatePickerComponent port)", () => {
    it("shows the ISO value as YYYY-MMM-DD and emits ISO dates as you type", async () => {
        const onChange = jest.fn();
        // v-model: the parent holds the value
        function Parent() {
            const [value, setValue] = useState("2026-10-30");
            return (
                <HgDatePicker
                    testID="noteDateInput"
                    value={value}
                    onChange={(v) => {
                        onChange(v);
                        setValue(v);
                    }}
                />
            );
        }
        await render(<Parent />);
        expect(inputValue(screen.getByTestId("noteDateInput"))).toBe(
            "2026-OCT-30"
        );
        await changeText(screen.getByTestId("noteDateInput"), "2025sep01");
        expect(onChange).toHaveBeenLastCalledWith("2025-09-01");
        expect(inputValue(screen.getByTestId("noteDateInput"))).toBe(
            "2025-SEP-01"
        );
        // an incomplete date clears the model but keeps what was typed
        await changeText(screen.getByTestId("noteDateInput"), "2025-SEP");
        expect(onChange).toHaveBeenLastCalledWith("");
        expect(inputValue(screen.getByTestId("noteDateInput"))).toBe(
            "2025-SEP"
        );
    });

    it("hide-details drops the messages; the id lands on the input", async () => {
        await render(
            <HgDatePicker
                testID="filterStartDateInput"
                nativeID="start-date"
                hideDetails
                errorMessages={["Required"]}
                value=""
                onChange={() => undefined}
            />
        );
        expect(screen.queryByText("Required")).toBeNull();
        const field = screen.getByTestId("filterStartDateInput");
        const input =
            Platform.OS === "web"
                ? (field as unknown as HTMLElement).querySelector("input")
                : field;
        expect(
            Platform.OS === "web" ? (input as HTMLInputElement).id : idOf(field)
        ).toBe("start-date");
    });

    if (Platform.OS !== "web") return;

    it("closes the calendar on Escape and returns focus to its button", async () => {
        await render(
            <HgDatePicker value="2026-10-30" onChange={() => undefined} />
        );
        const button = screen.getByLabelText("Open calendar");
        await press(button);
        expect(screen.getByTestId("date-picker-menu")).toBeTruthy();
        escapeKeyup();
        expect(screen.queryByTestId("date-picker-menu")).toBeNull();
        expect(document.activeElement).toBe(button);
    });

    it("in a dialog, Escape closes the calendar first and the dialog only on the next press", async () => {
        const onRequestClose = jest.fn<() => void>();
        await render(
            <AppModal transparent onRequestClose={onRequestClose}>
                <HgDatePicker value="2026-10-30" onChange={() => undefined} />
            </AppModal>
        );
        // react-native-web makes a modal active (so Escape reaches it) when its fade-in ends; jsdom never animates
        updateStores(() => {
            for (
                let el = document.querySelector("[aria-modal]")?.parentElement;
                el && el !== document.body;
                el = el.parentElement
            )
                el.dispatchEvent(new Event("animationend", { bubbles: true }));
        });
        expect(document.querySelector('[role="dialog"]')).not.toBeNull();
        await press(screen.getByLabelText("Open calendar"));
        expect(screen.getByTestId("date-picker-menu")).toBeTruthy();
        escapeKeyup();
        expect(screen.queryByTestId("date-picker-menu")).toBeNull();
        expect(onRequestClose).not.toHaveBeenCalled();
        escapeKeyup();
        expect(onRequestClose).toHaveBeenCalledTimes(1);
    });
});
