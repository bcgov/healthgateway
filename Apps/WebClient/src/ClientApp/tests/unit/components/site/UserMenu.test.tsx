import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { UserMenu } from "@/components/site/UserMenu";
import { resetStores } from "@/test/fixtures";
import { isExpanded, press, render, screen } from "@/test/ui";

jest.mock("expo-constants", () => ({
    __esModule: true,
    default: { expoConfig: { version: "1.2.345" } },
}));

const handlers = {
    onClose: jest.fn(),
    onProfile: jest.fn(),
    onLogout: jest.fn(),
};

describe("UserMenu", () => {
    beforeEach(() => {
        jest.clearAllMocks();
    });

    afterEach(() => {
        resetStores();
    });

    it("lists Dark Mode among the entries, and Support only when asked to", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        expect(screen.getByTestId("profileUserName")).toBeTruthy();
        expect(screen.getByTestId("profileBtn")).toBeTruthy();
        expect(screen.getByTestId("menu-btn-dark-mode-link")).toBeTruthy();
        expect(screen.getByTestId("logoutBtn")).toBeTruthy();
        expect(screen.queryByTestId("appearance-toggle")).toBeNull();
        expect(screen.queryByTestId("menu-btn-support-link")).toBeNull();
    });

    it("shows Support when told to", async () => {
        await render(
            <UserMenu visible showProfile showSupport {...handlers} />
        );
        expect(screen.getByTestId("menu-btn-support-link")).toBeTruthy();
    });

    it("orders the entries Profile, Dark Mode, Security (native), Support, Feedback, Log Out", async () => {
        await render(
            <UserMenu
                visible
                showProfile
                showSupport
                onFeedback={jest.fn()}
                {...handlers}
            />
        );
        const ids = screen
            .getAllByTestId(
                /^(profileBtn|menu-btn-(dark-mode|security|support|feedback)-link|logoutBtn)$/
            )
            .map((el) =>
                Platform.OS === "web"
                    ? (el as unknown as Element).getAttribute("data-testid")
                    : (el.props.testID as string)
            );
        expect(ids).toEqual([
            "profileBtn",
            "menu-btn-dark-mode-link",
            ...(Platform.OS === "web" ? [] : ["menu-btn-security-link"]),
            "menu-btn-support-link",
            "menu-btn-feedback-link",
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

    it("ends with the version, on every platform", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        expect(screen.getByText("Version 1.2.345")).toBeTruthy();
    });

    it("still logs out", async () => {
        await render(<UserMenu visible showProfile {...handlers} />);
        await press(screen.getByTestId("logoutBtn"));
        expect(handlers.onLogout).toHaveBeenCalledTimes(1);
    });
});
