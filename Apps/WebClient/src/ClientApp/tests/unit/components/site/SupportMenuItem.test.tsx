import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { SupportMenuItem } from "@/components/site/SupportMenuItem";
import { Path } from "@/constants/path";
import { allSupportLinks } from "@/navigation/supportLinks";
import { services } from "@/services/container";
import { resetRouter, router } from "@/test/expoRouterMock";
import { advanceTimers, isExpanded, press, render, screen } from "@/test/ui";

const onClose = jest.fn();

describe("SupportMenuItem", () => {
    beforeEach(() => {
        jest.useFakeTimers();
        onClose.mockClear();
        resetRouter(Path.Root);
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });

    afterEach(() => {
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    const open = () => press(screen.getByTestId("menu-btn-support-link"));

    it("starts closed", async () => {
        await render(<SupportMenuItem onClose={onClose} />);
        expect(screen.getByText("Support")).toBeTruthy();
        expect(isExpanded(screen.getByTestId("menu-btn-support-link"))).toBe(
            false
        );
    });

    it("opens in place to every footer link, and closes again", async () => {
        await render(<SupportMenuItem onClose={onClose} />);
        await open();
        for (const { text } of allSupportLinks) {
            expect(screen.getByText(text)).toBeTruthy();
        }
        await open();
        expect(isExpanded(screen.getByTestId("menu-btn-support-link"))).toBe(
            false
        );
    });

    it("external links only: leaves out the app's own pages (Terms of Service)", async () => {
        await render(<SupportMenuItem onClose={onClose} externalOnly />);
        await open();
        for (const link of allSupportLinks) {
            if (link.to) expect(screen.queryByText(link.text)).toBeNull();
            else expect(screen.getByText(link.text)).toBeTruthy();
        }
        expect(screen.queryByText("Terms of Service")).toBeNull();
    });

    it("closes the menu first, then follows the link", async () => {
        await render(<SupportMenuItem onClose={onClose} />);
        await open();
        await press(screen.getByTestId("footer-terms-of-service-link"));
        expect(onClose).toHaveBeenCalledTimes(1);
        if (Platform.OS === "ios") {
            expect(router.push).not.toHaveBeenCalled();
        }
        await advanceTimers(400);
        expect(router.push).toHaveBeenCalledWith(Path.TermsOfService);
    });
});
