import { describe, expect, it, jest } from "@jest/globals";
import { Platform, Text } from "react-native";

import { HgTextField } from "@/components/common/HgTextField";
import { expectNoAxeViolations } from "@/test/axe";
import {
    attrOf,
    changeText,
    containsInput,
    press,
    render,
    screen,
} from "@/test/ui";

describe("HgTextField", () => {
    it("labels the input and passes typed text up", async () => {
        const onChangeText = jest.fn();
        await render(
            <HgTextField
                label="Email Address"
                value=""
                onChangeText={onChangeText}
                testID="email"
            />
        );
        expect(screen.getByText("Email Address")).toBeTruthy();
        await changeText(
            screen.getByLabelText("Email Address"),
            "dr@example.test"
        );
        expect(onChangeText).toHaveBeenCalledWith("dr@example.test");
    });

    it("lists every error message", async () => {
        await render(
            <HgTextField
                value=""
                errorMessages={["Required", "Invalid"]}
                testID="f"
            />
        );
        expect(screen.getByText("Required")).toBeTruthy();
        expect(screen.getByText("Invalid")).toBeTruthy();
    });

    it("clearable: an x empties the editable field", async () => {
        const onChangeText = jest.fn();
        await render(
            <HgTextField
                label="Email Address"
                value="dr@example.test"
                onChangeText={onChangeText}
                clearable
                testID="email"
            />
        );
        await press(screen.getByLabelText("Clear Email Address"));
        expect(onChangeText).toHaveBeenCalledWith("");
    });

    it("no clear button when read-only or empty; append-inner content renders", async () => {
        await render(
            <HgTextField
                label="Cell Number"
                value="x"
                onChangeText={() => undefined}
                clearable
                editable={false}
                appendInner={<Text>spinner</Text>}
                testID="sms"
            />
        );
        expect(screen.queryByLabelText("Clear Cell Number")).toBeNull();
        expect(screen.getByText("spinner")).toBeTruthy();
    });

    it("web: the test id wraps the input (`[data-testid=x] input`); native: it is the input", async () => {
        await render(
            <HgTextField
                label="Email"
                value=""
                onChangeText={() => undefined}
                testID="email-input"
            />
        );
        expect(containsInput(screen.getByTestId("email-input"))).toBe(true);
    });

    it("names the input after the placeholder when there is no label", async () => {
        await render(
            <HgTextField
                value=""
                onChangeText={() => undefined}
                placeholder="Your email address"
                testID="f"
            />
        );
        expect(screen.getByLabelText("Your email address")).toBeTruthy();
    });

    it("web: the error messages describe the invalid input, and nothing does while it is valid", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgTextField
                label="Email Address"
                value="nope"
                onChangeText={() => undefined}
                errorMessages={["Valid email is required"]}
                testID="f"
            />
        );
        const input = screen.getByLabelText("Email Address");
        expect(attrOf(input, "aria-invalid")).toBe("true");
        const describedBy = attrOf(input, "aria-describedby");
        expect(describedBy).toBeTruthy();
        const messages = document.getElementById(describedBy!);
        expect(messages?.textContent).toBe("Valid email is required");
        expect(messages?.getAttribute("role")).toBe("status");
        await expectNoAxeViolations(
            screen.getByTestId("f") as unknown as HTMLElement
        );
        await render(
            <HgTextField
                label="Email Address"
                value="dr@example.test"
                onChangeText={() => undefined}
                testID="f"
            />
        );
        const valid = screen.getByLabelText("Email Address");
        expect(attrOf(valid, "aria-invalid")).toBeUndefined();
        expect(attrOf(valid, "aria-describedby")).toBeUndefined();
        await expectNoAxeViolations(
            screen.getByTestId("f") as unknown as HTMLElement
        );
    });

    it("web: the counter describes the input too", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgTextField
                label="Title"
                value="abc"
                maxLength={100}
                counter
                onChangeText={() => undefined}
                testID="f"
            />
        );
        const input = screen.getByLabelText("Title");
        const counter = document.getElementById(
            attrOf(input, "aria-describedby")!
        );
        expect(counter?.textContent).toBe("3/100");
    });

    it("web: autocomplete is off unless the caller says what the field collects", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <HgTextField
                label="Title"
                value=""
                onChangeText={() => undefined}
            />
        );
        expect(attrOf(screen.getByLabelText("Title"), "autocomplete")).toBe(
            "off"
        );
        await render(
            <HgTextField
                label="Email Address"
                value=""
                autoComplete="email"
                onChangeText={() => undefined}
            />
        );
        expect(
            attrOf(screen.getByLabelText("Email Address"), "autocomplete")
        ).toBe("email");
    });
});
