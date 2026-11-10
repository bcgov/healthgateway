import { describe, expect, it, jest } from "@jest/globals";
import { useState } from "react";
import { State } from "react-native-gesture-handler";
import {
    fireGestureHandler,
    getByGestureTestId,
} from "react-native-gesture-handler/jest-utils";

import { HgTabPanel, HgTabs, HgWindow } from "@/components/common";
import { type HgTab } from "@/components/common/HgTabs";
import { neighbourTab } from "@/components/common/HgWindow";
import { inAct, isSelected, render, screen } from "@/test/ui";
import { swipeStep } from "@/utils/swipe";

const swipe = async (testID: string, dx: number, dy = 0) => {
    await inAct(() => {
        fireGestureHandler(getByGestureTestId(`${testID}-swipe`), [
            { state: State.BEGAN, translationX: 0, translationY: 0 },
            { state: State.ACTIVE, translationX: dx / 2, translationY: dy / 2 },
            { state: State.END, translationX: dx, translationY: dy },
        ]);
    });
};

function Tabs({ tabs, onPress }: { tabs: HgTab[]; onPress?: () => void }) {
    const [value, setValue] = useState(0);
    const withPress = tabs.map((t) => ({ ...t, onPress }));
    return (
        <>
            <HgTabs tabs={withPress} value={value} onChange={setValue} />
            <HgWindow
                tabs={withPress}
                value={value}
                onChange={setValue}
                testID="window"
            >
                {tabs.map((t) => (
                    <HgTabPanel key={t.value} active={t.value === value}>
                        <></>
                    </HgTabPanel>
                ))}
            </HgWindow>
        </>
    );
}

const three: HgTab[] = [
    { value: 0, title: "One", testID: "tab-0" },
    { value: 1, title: "Two", testID: "tab-1" },
    { value: 2, title: "Three", testID: "tab-2" },
];

describe("HgWindow (v-window touch)", () => {
    it("counts a swipe past 16px that moves less than half as far vertically", () => {
        expect(swipeStep(-40, 10)).toBe(1);
        expect(swipeStep(40, -10)).toBe(-1);
        expect(swipeStep(-16, 0)).toBe(0);
        expect(swipeStep(-40, 20)).toBe(0);
    });

    it("steps to the neighbouring enabled tab and stops at the ends", () => {
        const tabs: HgTab[] = [
            { value: 0, title: "A" },
            { value: 1, title: "B", disabled: true },
            { value: 3, title: "C" },
        ];
        expect(neighbourTab(tabs, 0, 1)?.value).toBe(3);
        expect(neighbourTab(tabs, 3, -1)?.value).toBe(0);
        expect(neighbourTab(tabs, 3, 1)).toBeUndefined();
        expect(neighbourTab(tabs, 0, -1)).toBeUndefined();
    });

    it("swiping left opens the next tab and right the previous, running the tab's press", async () => {
        const onPress = jest.fn();
        await render(<Tabs tabs={three} onPress={onPress} />);
        await swipe("window", -60);
        expect(isSelected(screen.getByTestId("tab-1"))).toBe(true);
        expect(onPress).toHaveBeenCalledTimes(1);
        await swipe("window", -60);
        expect(isSelected(screen.getByTestId("tab-2"))).toBe(true);
        await swipe("window", -60);
        expect(isSelected(screen.getByTestId("tab-2"))).toBe(true);
        await swipe("window", 60);
        expect(isSelected(screen.getByTestId("tab-1"))).toBe(true);
    });

    it("ignores a mostly vertical drag", async () => {
        await render(<Tabs tabs={three} />);
        await swipe("window", -40, 30);
        expect(isSelected(screen.getByTestId("tab-0"))).toBe(true);
    });
});
