import { describe, expect, it, jest } from "@jest/globals";
import { act } from "@testing-library/react-native";
import { Platform } from "react-native";

import { HgCheckbox } from "@/components/ui/HgCheckbox";
import {
    attrOf,
    isChecked,
    isDisabled,
    press,
    pressKey,
    render,
    screen,
} from "@/test/ui";

describe("HgCheckbox", () => {
    if (Platform.OS === "web") {
        it("web: the input over the box can be clicked, and toggles once", async () => {
            const onChange = jest.fn();
            await render(
                <HgCheckbox
                    checked={false}
                    onChange={onChange}
                    label="I agree"
                    testID="cb"
                />
            );
            const input = (
                screen.getByTestId("cb") as unknown as HTMLElement
            ).querySelector("input") as HTMLInputElement;
            expect(input.style.pointerEvents).not.toBe("none");
            await act(async () => input.click());
            expect(onChange).toHaveBeenCalledTimes(1);
            expect(onChange).toHaveBeenCalledWith(true);
        });
    }

    it("reports the opposite value on press and exposes the checked state", async () => {
        const onChange = jest.fn();
        await render(
            <HgCheckbox
                checked={false}
                onChange={onChange}
                label="I agree"
                testID="cb"
            />
        );
        expect(isChecked(screen.getByTestId("cb"))).toBe(false);
        await press(screen.getByTestId("cb"));
        expect(onChange).toHaveBeenCalledWith(true);
    });

    it("shows error messages under the row and respects disabled", async () => {
        const onChange = jest.fn();
        await render(
            <HgCheckbox
                checked
                onChange={onChange}
                label="Terms"
                disabled
                errorMessages={["You must accept"]}
                testID="cb"
            />
        );
        expect(screen.getByText("You must accept")).toBeTruthy();
        expect(isChecked(screen.getByTestId("cb"))).toBe(true);
        expect(isDisabled(screen.getByTestId("cb"))).toBe(true);
        await press(screen.getByTestId("cb"));
        expect(onChange).not.toHaveBeenCalled();
    });

    it("web: Space toggles it once, as Enter does, and neither does anything while disabled", async () => {
        if (Platform.OS !== "web") return;
        const onChange = jest.fn();
        await render(
            <HgCheckbox
                checked={false}
                onChange={onChange}
                label="I agree"
                testID="cb"
            />
        );
        await pressKey(screen.getByTestId("cb"), " ");
        expect(onChange).toHaveBeenCalledTimes(1);
        expect(onChange).toHaveBeenCalledWith(true);
        await pressKey(screen.getByTestId("cb"), "Enter");
        expect(onChange).toHaveBeenCalledTimes(2);
        await render(
            <HgCheckbox
                checked
                onChange={onChange}
                label="I agree"
                disabled
                testID="cb"
            />
        );
        await pressKey(screen.getByTestId("cb"), " ");
        await pressKey(screen.getByTestId("cb"), "Enter");
        expect(onChange).toHaveBeenCalledTimes(2);
    });

    it("web: its error messages describe the box", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgCheckbox
                checked={false}
                onChange={() => undefined}
                label="Terms"
                errorMessages={["You must accept"]}
                testID="cb"
            />
        );
        const box = screen.getByTestId("cb");
        expect(attrOf(box, "aria-invalid")).toBe("true");
        const messages = document.getElementById(
            attrOf(box, "aria-describedby")!
        );
        expect(messages?.textContent).toBe("You must accept");
        await render(
            <HgCheckbox
                checked
                onChange={() => undefined}
                label="Terms"
                testID="cb"
            />
        );
        expect(
            attrOf(screen.getByTestId("cb"), "aria-invalid")
        ).toBeUndefined();
    });
});
