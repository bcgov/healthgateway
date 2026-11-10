import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { AccessibilityInfo, Platform } from "react-native";

import { HgAlert } from "@/components/common/HgAlert";
import { attrOf, press, render, screen } from "@/test/ui";

describe("HgAlert", () => {
    it("renders title and text with the alert role", async () => {
        await render(
            <HgAlert
                type="info"
                title="Heads up"
                text="Something to know"
                testID="a"
            />
        );
        expect(screen.getByTestId("a")).toBeTruthy();
        expect(screen.getByText("Heads up")).toBeTruthy();
        expect(screen.getByText("Something to know")).toBeTruthy();
    });

    it("offers a close button only when closable", async () => {
        const onClose = jest.fn();
        await render(
            <HgAlert type="error" text="Bad" closable onClose={onClose} />
        );
        await press(screen.getByLabelText("Close"));
        expect(onClose).toHaveBeenCalledTimes(1);
    });

    it("has no close button otherwise", async () => {
        await render(<HgAlert type="warning" text="Careful" />);
        expect(screen.queryByLabelText("Close")).toBeNull();
    });

    describe("live", () => {
        afterEach(() => {
            jest.restoreAllMocks();
        });

        it("is a note by default, so a notice on the page at load is not read out on its own", async () => {
            const say = jest
                .spyOn(AccessibilityInfo, "announceForAccessibility")
                .mockImplementation(() => undefined);
            await render(<HgAlert type="info" text="On the page" testID="a" />);
            expect(attrOf(screen.getByTestId("a"), "role")).toBe(
                Platform.OS === "web" ? "note" : undefined
            );
            expect(say).not.toHaveBeenCalled();
        });

        it("live: a status region for the result of an action, an alert for an error", async () => {
            const say = jest
                .spyOn(AccessibilityInfo, "announceForAccessibility")
                .mockImplementation(() => undefined);
            await render(
                <HgAlert
                    type="success"
                    live="polite"
                    title="Updated"
                    text="Your email was saved"
                    testID="a"
                />
            );
            if (Platform.OS === "web") {
                expect(attrOf(screen.getByTestId("a"), "role")).toBe("status");
                expect(attrOf(screen.getByTestId("a"), "aria-live")).toBe(
                    "polite"
                );
            } else {
                expect(say).toHaveBeenCalledWith(
                    "Updated. Your email was saved"
                );
            }
            await render(
                <HgAlert
                    type="error"
                    live="assertive"
                    text="Failed"
                    testID="a"
                />
            );
            if (Platform.OS === "web")
                expect(attrOf(screen.getByTestId("a"), "role")).toBe("alert");
            else expect(say).toHaveBeenLastCalledWith("Failed");
        });
    });
});
