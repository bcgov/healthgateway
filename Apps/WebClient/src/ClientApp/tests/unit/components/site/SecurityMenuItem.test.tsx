import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { SecurityMenuItem } from "@/components/site/SecurityMenuItem";
import { Path } from "@/constants/path";
import { resetRouter, router } from "@/test/expoRouterMock";
import { advanceTimers, press, render, screen } from "@/test/ui";

describe("SecurityMenuItem", () => {
    const onClose = jest.fn();

    beforeEach(() => {
        jest.useFakeTimers();
        onClose.mockClear();
        resetRouter(Path.Home);
    });
    afterEach(() => {
        jest.useRealTimers();
    });

    if (Platform.OS === "web") {
        it("is not on web, which has no device security", async () => {
            await render(<SecurityMenuItem onClose={onClose} />);
            expect(screen.queryByTestId("menu-btn-security-link")).toBeNull();
        });
        return;
    }

    it("closes the menu, then opens the Security page", async () => {
        await render(<SecurityMenuItem onClose={onClose} />);
        await press(screen.getByTestId("menu-btn-security-link"));
        expect(onClose).toHaveBeenCalledTimes(1);
        expect(router.push).not.toHaveBeenCalled();
        await advanceTimers(400);
        expect(router.push).toHaveBeenCalledWith(Path.Security);
    });
});
