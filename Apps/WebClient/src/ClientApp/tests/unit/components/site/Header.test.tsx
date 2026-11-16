import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform, View } from "react-native";

import { Header } from "@/components/site/Header.web";
import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { expectNoAxeViolations } from "@/test/axe";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, resetStores, signInRegistered } from "@/test/fixtures";
import {
    attrOf,
    press,
    pressKeepsDefault,
    render,
    screen,
    tagOf,
} from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

// web only: native shows each stack's navigation bar (nativeHeader.test.ts)
const describeWeb = Platform.OS === "web" ? describe : describe.skip;

describeWeb("Header", () => {
    beforeEach(() => {
        resetRouter("/");
        setBreakpoint(1366);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the logo link for a visitor, with a Log in button that routes to login", async () => {
        await render(<Header />);
        expect(
            screen.getByLabelText("Go to Health Gateway home page")
        ).toBeTruthy();
        await press(screen.getByTestId("loginBtn"));
        expect(router.push).toHaveBeenCalledWith(Path.Login);
        expect(screen.queryByTestId("headerDropdownBtn")).toBeNull();
        expect(screen.queryByTestId("navbar-toggle-button")).toBeNull();
    });

    it("has a header Log in button on the landing page", async () => {
        resetRouter(Path.Root);
        await render(<Header />);
        expect(screen.getByTestId("loginBtn")).toBeTruthy();
    });

    it.each([Path.LogoutComplete, Path.Unauthorized, Path.NotFound])(
        "on %s, signed out: the Log in button",
        async (path) => {
            resetRouter(path);
            await render(<Header />);
            expect(screen.getByTestId("loginBtn")).toBeTruthy();
        }
    );

    it("has no gear: signed out, the web header has no menu", async () => {
        resetRouter(Path.Login);
        await render(<Header />);
        expect(screen.queryByTestId("settingsMenuBtn")).toBeNull();
    });

    it("signed in, the account menu has no Support: the web keeps the footer", async () => {
        signInRegistered();
        await render(<Header />);
        await press(screen.getByTestId("headerDropdownBtn"));
        expect(screen.getByTestId("logoutBtn")).toBeTruthy();
        expect(screen.queryByTestId("menu-btn-support-link")).toBeNull();
    });

    it("hides the Log in button on the login page itself", async () => {
        resetRouter(Path.Login);
        await render(<Header />);
        expect(screen.queryByTestId("loginBtn")).toBeNull();
    });

    it("shows the initials avatar and the drawer toggle for a registered user", async () => {
        signInRegistered();
        await render(<Header />);
        expect(screen.getByText("DG")).toBeTruthy();
        expect(screen.queryByTestId("loginBtn")).toBeNull();
        expect(screen.getByTestId("navbar-toggle-button")).toBeTruthy();
    });

    it("sends the logo to Home when signed in and to the landing page otherwise", async () => {
        // browser history
        const went = router.push;
        resetRouter(Path.LogoutComplete);
        await render(<Header />);
        await press(screen.getByLabelText("Go to Health Gateway home page"));
        expect(went).toHaveBeenLastCalledWith(Path.Root);
        signInRegistered();
        resetRouter(Path.Timeline);
        await render(<Header />);
        await press(screen.getByLabelText("Go to Health Gateway home page"));
        expect(went).toHaveBeenLastCalledWith(Path.Home);
    });

    it("does not stack another Profile when already on it", async () => {
        signInRegistered();
        resetRouter(Path.Profile);
        await render(<Header />);
        await press(screen.getByTestId("headerDropdownBtn"));
        await press(screen.getByTestId("profileBtn"));
        expect(router.push).not.toHaveBeenCalled();
    });

    it("the logo is a real link to its destination, which the keyboard can follow", async () => {
        await render(<Header />);
        const logo = screen.getByLabelText("Go to Health Gateway home page");
        expect(tagOf(logo)).toBe("a");
        expect(attrOf(logo, "href")).toBe(Path.Root);
        // the press tracks and navigates itself: the browser must not follow the anchor as well
        expect(await pressKeepsDefault(logo)).toBe(false);
        expect(router.push).toHaveBeenLastCalledWith(Path.Root);
        signInRegistered();
        await render(<Header />);
        expect(
            attrOf(
                screen.getByLabelText("Go to Health Gateway home page"),
                "href"
            )
        ).toBe(Path.Home);
    });

    it("the drawer toggle and account button say what they open and whether it is open; axe is clean", async () => {
        signInRegistered();
        // the drawer the toggle controls lives beside the header in the shell
        await render(
            <>
                <Header />
                <View nativeID="sidenavbar" />
            </>
        );
        const toggle = screen.getByTestId("navbar-toggle-button");
        expect(attrOf(toggle, "aria-expanded")).toBe("true");
        expect(attrOf(toggle, "aria-controls")).toBe("sidenavbar");
        const account = screen.getByTestId("headerDropdownBtn");
        expect(attrOf(account, "aria-haspopup")).toBe("menu");
        expect(attrOf(account, "aria-expanded")).toBe("false");
        await expectNoAxeViolations(
            (toggle as unknown as HTMLElement).closest("[class]") as HTMLElement
        );
        await press(account);
        expect(
            attrOf(screen.getByTestId("headerDropdownBtn"), "aria-expanded")
        ).toBe("true");
    });
});
