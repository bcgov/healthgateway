import { afterEach, describe, expect, it, jest } from "@jest/globals";
import { Dimensions, Platform, StyleSheet, Text } from "react-native";

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

// React Native's Jest setup reports a fontScale of 2 unless told otherwise
const scaleText = (fontScale: number) =>
    jest.spyOn(Dimensions, "get").mockReturnValue({
        width: 390,
        height: 844,
        scale: 3,
        fontScale,
    });

describe("HgTextField", () => {
    afterEach(() => {
        jest.restoreAllMocks();
    });

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

    it("wears its input's keyboard focus round the whole field box on web, its icon, × and slot included", async () => {
        await render(
            <HgTextField
                label="Search"
                value="flu"
                onChangeText={() => undefined}
                clearable
                prependInner={<Text testID="search-icon">S</Text>}
                appendInner={<Text testID="send">Send</Text>}
                testID="search"
            />
        );
        // NativeWind classes compile away in the web tests (in the browser they reach the DOM); the native test
        // renderer keeps them
        if (Platform.OS === "web") return;
        const input = screen.getByLabelText("Search");
        // global.css draws the ring round the host while its target has keyboard focus
        expect(String(input.props.className)).toMatch(
            /\bhg-field-focus-target\b/
        );
        let box = input.parent;
        while (
            box &&
            !/\bhg-field-focus-host\b/.test(String(box.props.className))
        )
            box = box.parent;
        expect(box).toBeTruthy();
        for (const el of [
            screen.getByTestId("search-icon"),
            screen.getByTestId("send"),
            screen.getByLabelText("Clear Search"),
        ]) {
            let node = el.parent;
            while (node && node !== box) node = node.parent;
            expect(node).toBe(box);
        }
        // the × keeps its own ring
        expect(
            String(screen.getByLabelText("Clear Search").props.className)
        ).not.toMatch(/\bhg-field-focus-target\b/);
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

    it("multiline numberOfLines is a minimum height on web and iOS (24px a line, scaled on native, plus padding)", async () => {
        if (Platform.OS !== "web") scaleText(1.5);
        await render(
            <HgTextField
                accessibilityLabel="Note"
                value=""
                onChangeText={() => undefined}
                multiline
                numberOfLines={3}
            />
        );
        const input = screen.getByLabelText("Note");
        if (Platform.OS === "web")
            expect((input as unknown as HTMLElement).style.minHeight).toBe(
                "96px"
            );
        else
            expect(
                (
                    StyleSheet.flatten(input.props.style as never) as
                        { minHeight?: number } | undefined
                )?.minHeight
            ).toBe(132);
    });

    it("native: a multiline field starts from its content's height, so it grows wherever it is placed", async () => {
        if (Platform.OS === "web") return;
        await render(
            <HgTextField
                accessibilityLabel="Comment"
                value=""
                onChangeText={() => undefined}
                multiline
            />
        );
        const classes = String(
            screen.getByLabelText("Comment").props.className
        );
        expect(classes).toMatch(/\bgrow\b/);
        expect(classes).not.toMatch(/\bflex-1\b/);
    });

    it("maxLines caps a multiline field's height on native, its lines scaled with the text, then it scrolls inside; web keeps growing", async () => {
        if (Platform.OS !== "web") scaleText(1.5);
        await render(
            <HgTextField
                accessibilityLabel="Note"
                value=""
                onChangeText={() => undefined}
                multiline
                autoGrow
                maxLines={5}
            />
        );
        const input = screen.getByLabelText("Note");
        if (Platform.OS === "web")
            expect((input as unknown as HTMLElement).style.maxHeight).toBe("");
        else
            expect(
                (
                    StyleSheet.flatten(input.props.style as never) as
                        { maxHeight?: number } | undefined
                )?.maxHeight
            ).toBe(204);
    });

    it("android: numberOfLines is left to the platform, without a minimum height", async () => {
        if (Platform.OS === "web") return;
        const os = jest.replaceProperty(Platform, "OS", "android");
        try {
            await render(
                <HgTextField
                    accessibilityLabel="Note"
                    value=""
                    onChangeText={() => undefined}
                    multiline
                    numberOfLines={3}
                />
            );
            const input = screen.getByLabelText("Note");
            expect(
                (
                    StyleSheet.flatten(input.props.style as never) as
                        { minHeight?: number } | undefined
                )?.minHeight
            ).toBeUndefined();
            expect(input.props.numberOfLines).toBe(3);
        } finally {
            os.restore();
        }
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
