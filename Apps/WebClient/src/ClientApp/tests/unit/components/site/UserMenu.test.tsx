import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { UserMenu } from "@/components/site/UserMenu.web";
import { resetStores } from "@/test/fixtures";
import { isExpanded, press, render, screen } from "@/test/ui";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "3.0.1", extra: { build: 345 } } },
}));

const handlers = {
    onClose: jest.fn(),
    onProfile: jest.fn(),
    onLogout: jest.fn(),
};

// web only: native's account menu is the navigation bar's (useHeaderMenu.test.ts)
const describeWeb = Platform.OS === "web" ? describe : describe.skip;

describeWeb("UserMenu", () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    afterEach(() => {
        resetStores();
    });

    it("orders the entries Profile, Dark Mode, Log Out, with no Security, Support or Feedback", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        expect(screen.getByTestId("profileUserName")).toBeTruthy();
        expect(screen.queryByTestId("appearance-toggle")).toBeNull();
        const ids = screen
            .getAllByTestId(
                /^(profileBtn|menu-btn-(dark-mode|security|support|feedback)-link|logoutBtn)$/
            )
            .map((el) =>
                (el as unknown as Element).getAttribute("data-testid")
            );
        expect(ids).toEqual([
            "profileBtn",
            "menu-btn-dark-mode-link",
            "logoutBtn",
        ]);
    });

    it("stays open while a Dark Mode choice is made", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        await press(screen.getByTestId("menu-btn-dark-mode-link"));
        await press(screen.getByTestId("dark-mode-on"));
        expect(handlers.onClose).not.toHaveBeenCalled();
        expect(isExpanded(screen.getByTestId("menu-btn-dark-mode-link"))).toBe(
            true
        );
    });

    it("ends with the version", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        expect(screen.getByText("v3.0.1.0345")).toBeTruthy();
    });

    it("still logs out", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        await press(screen.getByTestId("logoutBtn"));
        expect(handlers.onLogout).toHaveBeenCalledTimes(1);
    });
});
