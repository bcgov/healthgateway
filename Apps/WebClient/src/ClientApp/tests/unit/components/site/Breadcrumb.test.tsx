import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { Breadcrumb } from "@/components/site/Breadcrumb";
import { Path } from "@/constants/path";
import { Action, Destination, Text } from "@/constants/tracking";
import { services } from "@/services/container";
import { expectNoAxeViolations } from "@/test/axe";
import { resetRouter, router } from "@/test/expoRouterMock";
import { resetStores, signInRegistered } from "@/test/fixtures";
import {
    attrOf,
    press,
    pressKeepsDefault,
    render,
    screen,
    tagOf,
} from "@/test/ui";

describe("Breadcrumb", () => {
    beforeEach(() => resetRouter("/profile"));
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("renders nothing for a signed-out visitor", async () => {
        await render(
            <Breadcrumb
                items={[
                    { text: "Profile", active: true, testId: "bc-profile" },
                ]}
            />
        );
        expect(screen.queryByTestId("breadcrumbs")).toBeNull();
    });

    it("prefixes Home, marks the active item, and tracks the Home link", async () => {
        signInRegistered();
        const track = jest
            .spyOn(services.tracking, "trackEvent")
            .mockImplementation(() => undefined);
        await render(
            <Breadcrumb
                items={[
                    { text: "Profile", active: true, testId: "bc-profile" },
                ]}
            />
        );
        expect(screen.getByText("Home")).toBeTruthy();
        expect(screen.getByText("Profile")).toBeTruthy();
        await press(screen.getByTestId("breadcrumb-home"));
        expect(router.push).toHaveBeenCalledWith(Path.Home);
        expect(track).toHaveBeenCalledWith(
            expect.objectContaining({
                action: Action.InternalLink,
                text: Text.HomeBreadcrumb,
                destination: Destination.Home,
            })
        );
        await press(screen.getByTestId("bc-profile"));
        expect(router.push).toHaveBeenCalledTimes(1);
    });

    it("web: Home is a real link the keyboard can follow; the current page is text marked as the current page", async () => {
        if (Platform.OS !== "web") return;
        signInRegistered();
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        await render(
            <Breadcrumb
                items={[
                    { text: "Profile", active: true, testId: "bc-profile" },
                ]}
            />
        );
        const home = screen.getByTestId("breadcrumb-home");
        expect(tagOf(home)).toBe("a");
        expect(attrOf(home, "href")).toBe(Path.Home);
        // the press navigates itself: the browser must not follow the anchor as well
        expect(await pressKeepsDefault(home)).toBe(false);
        expect(router.push).toHaveBeenCalledWith(Path.Home);
        const current = screen.getByTestId("bc-profile");
        expect(tagOf(current)).not.toBe("a");
        expect(attrOf(current, "aria-current")).toBe("page");
        // Cypress (e2e/pages/breadcrumbs.js) finds the active crumb by this class
        expect(
            (current as unknown as HTMLElement).classList.contains(
                "v-breadcrumbs-item--active"
            )
        ).toBe(true);
    });

    it("web: a labelled navigation landmark that axe accepts", async () => {
        if (Platform.OS !== "web") return;
        signInRegistered();
        await render(
            <Breadcrumb
                items={[
                    { text: "Profile", active: true, testId: "bc-profile" },
                ]}
            />
        );
        const nav = screen.getByTestId("breadcrumbs");
        expect(tagOf(nav)).toBe("nav");
        expect(attrOf(nav, "aria-label")).toBe("Breadcrumb Nav");
        await expectNoAxeViolations(nav as unknown as HTMLElement);
    });
});
