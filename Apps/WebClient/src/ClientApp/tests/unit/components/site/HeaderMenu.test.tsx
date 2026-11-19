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

import { HgButton } from "@/components/common";
import { HeaderMenu } from "@/components/site/HeaderMenu.web";
import { render, screen } from "@/test/ui";

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

// web only: native's menus are the platform's own (nativeHeader.test.ts)
const describeWeb = Platform.OS === "web" ? describe : describe.skip;

describeWeb("HeaderMenu", () => {
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
});
