import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { useState } from "react";
import { Platform, Text } from "react-native";

import { HeaderMenu } from "@/components/site/HeaderMenu";
import { HgButton } from "@/components/ui";
import { advanceTimers, press, render, screen } from "@/test/ui";

function Harness() {
    const [visible, setVisible] = useState(true);
    return (
        <>
            <HgButton
                testID="toggle"
                text="t"
                onPress={() => setVisible(!visible)}
            />
            <HeaderMenu visible={visible} onClose={() => setVisible(false)}>
                <Text>menu content</Text>
            </HeaderMenu>
        </>
    );
}

describe("HeaderMenu", () => {
    beforeEach(() => {
        jest.useFakeTimers();
    });

    afterEach(() => {
        jest.useRealTimers();
    });

    it("shows its content while open", async () => {
        await render(<Harness />);
        expect(screen.getByText("menu content")).toBeTruthy();
    });

    if (Platform.OS === "web") return;

    it("is a panel that stays mounted while it slides away", async () => {
        await render(<Harness />);
        expect(screen.getByTestId("headerMenu")).toBeTruthy();

        await press(screen.getByTestId("headerMenuBackdrop"));
        expect(screen.getByText("menu content")).toBeTruthy();

        await advanceTimers(1000);
        expect(screen.queryByText("menu content")).toBeNull();
    });
});
