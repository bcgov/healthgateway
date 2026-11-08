import { describe, expect, it, jest } from "@jest/globals";
import { useState } from "react";
import { Platform } from "react-native";

import { AppText } from "@/components/ui/AppText";
import { HgTabPanel, HgTabs } from "@/components/ui/HgTabs";
import { expectNoAxeViolations } from "@/test/axe";
import {
    attrOf,
    isDisabled,
    press,
    pressKey,
    render,
    screen,
    tagOf,
} from "@/test/ui";

function Example({ onPressTwo }: { onPressTwo: () => void }) {
    const [value, setValue] = useState(0);
    return (
        <>
            <HgTabs
                value={value}
                onChange={setValue}
                tabs={[
                    { value: 0, title: "One", testID: "tab-one" },
                    {
                        value: 1,
                        title: "Two",
                        testID: "tab-two",
                        onPress: onPressTwo,
                    },
                    {
                        value: 2,
                        title: "Off",
                        testID: "tab-off",
                        disabled: true,
                    },
                ]}
            />
            <HgTabPanel active={value === 0} testID="panel-one">
                <AppText>first</AppText>
            </HgTabPanel>
            <HgTabPanel active={value === 1} testID="panel-two">
                <AppText>second</AppText>
            </HgTabPanel>
        </>
    );
}

describe("HgTabs and HgTabPanel (v-tabs, v-window-item)", () => {
    it("mounts a panel's content the first time it is shown, and runs the tab's press", async () => {
        const onPressTwo = jest.fn();
        await render(<Example onPressTwo={onPressTwo} />);
        expect(screen.getByText("first")).toBeTruthy();
        expect(screen.queryByText("second")).toBeNull();
        await press(screen.getByTestId("tab-two"));
        expect(onPressTwo).toHaveBeenCalled();
        expect(screen.getByText("second")).toBeTruthy();
    });

    it("a disabled tab cannot be selected", async () => {
        await render(<Example onPressTwo={() => undefined} />);
        expect(isDisabled(screen.getByTestId("tab-off"))).toBe(true);
        await press(screen.getByTestId("tab-off"));
        expect(screen.getByText("first")).toBeTruthy();
    });

    it("spaces the uppercase labels 1.25px apart, as `.v-tab.text-uppercase` (healthgateway d7ab5c429)", async () => {
        await render(<Example onPressTwo={() => undefined} />);
        // NativeWind classes compile away on web; the native test renderer keeps them
        if (Platform.OS === "web") return;
        const label = screen.getByText("One");
        expect(label.props.className).toMatch(/\buppercase\b/);
        expect(label.props.className).toContain("tracking-[1.25px]");
    });

    it("web: real tab buttons in a tablist, the selected one marked; Right and Left move the selection", async () => {
        if (Platform.OS !== "web") return;
        await render(<Example onPressTwo={() => undefined} />);
        const one = screen.getByTestId("tab-one");
        const two = screen.getByTestId("tab-two");
        expect(tagOf(one)).toBe("button");
        expect(attrOf(one, "role")).toBe("tab");
        expect(
            (one as unknown as HTMLElement).closest('[role="tablist"]')
        ).not.toBeNull();
        expect(attrOf(one, "aria-selected")).toBe("true");
        expect(attrOf(two, "aria-selected")).toBe("false");
        // the selected tab is the one tab stop; the others are reached with the arrow keys
        expect(attrOf(one, "tabindex")).toBe("0");
        expect(attrOf(two, "tabindex")).toBe("-1");
        await pressKey(one, "ArrowRight");
        expect(attrOf(screen.getByTestId("tab-two"), "aria-selected")).toBe(
            "true"
        );
        expect(screen.getByText("second")).toBeTruthy();
        // Right again skips the disabled tab and wraps to the first
        await pressKey(screen.getByTestId("tab-two"), "ArrowRight");
        expect(attrOf(screen.getByTestId("tab-one"), "aria-selected")).toBe(
            "true"
        );
        expect(attrOf(screen.getByTestId("panel-one"), "role")).toBe(
            "tabpanel"
        );
        await expectNoAxeViolations(
            (one as unknown as HTMLElement).closest(
                '[role="tablist"]'
            ) as HTMLElement
        );
    });
});
