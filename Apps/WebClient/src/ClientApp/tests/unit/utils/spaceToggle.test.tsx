import { describe, expect, it, jest } from "@jest/globals";
import { Platform } from "react-native";

import { spaceToggleProps } from "@/utils/spaceToggle";

const keyEvent = (key: string, repeat = false) => ({
    key,
    repeat,
    preventDefault: jest.fn(),
});

describe("spaceToggleProps", () => {
    it("web: toggles on Space (not on a held key, not on other keys) and keeps the page from scrolling; native: nothing", () => {
        const onToggle = jest.fn();
        const props = spaceToggleProps(onToggle) as {
            onKeyDown?: (e: ReturnType<typeof keyEvent>) => void;
        };
        if (Platform.OS !== "web") {
            expect(props).toEqual({});
            return;
        }
        const space = keyEvent(" ");
        props.onKeyDown?.(space);
        expect(onToggle).toHaveBeenCalledTimes(1);
        expect(space.preventDefault).toHaveBeenCalled();
        props.onKeyDown?.(keyEvent("Spacebar"));
        expect(onToggle).toHaveBeenCalledTimes(2);
        const held = keyEvent(" ", true);
        props.onKeyDown?.(held);
        expect(onToggle).toHaveBeenCalledTimes(2);
        expect(held.preventDefault).toHaveBeenCalled();
        const enter = keyEvent("Enter");
        props.onKeyDown?.(enter);
        expect(onToggle).toHaveBeenCalledTimes(2);
        expect(enter.preventDefault).not.toHaveBeenCalled();
    });

    it("nothing while disabled or without a handler", () => {
        expect(spaceToggleProps(() => undefined, true)).toEqual({});
        expect(spaceToggleProps(undefined)).toEqual({});
    });
});
