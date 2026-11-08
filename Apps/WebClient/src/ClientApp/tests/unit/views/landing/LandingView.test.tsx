import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router } from "@/test/expoRouterMock";
import { configFixture, resetStores, signInRegistered } from "@/test/fixtures";
import { press, render, screen } from "@/test/ui";
import { LandingView } from "@/views/landing/LandingView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("LandingView", () => {
    beforeEach(() => {
        resetRouter(Path.Root);
        setBreakpoint(390);
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });

    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("keeps the heading and the way in on every platform", async () => {
        await render(<LandingView />);
        expect(
            screen.getByText("Access your health information online")
        ).toBeTruthy();
        await press(screen.getByTestId("btnLogin"));
        expect(router.push).toHaveBeenCalledWith(Path.Login);
        await press(screen.getByTestId("btnStart"));
        // native: /registration needs a login, which then returns there
        expect(router.push).toHaveBeenCalledWith(
            Platform.OS === "web"
                ? Path.Registration
                : {
                      pathname: Path.Login,
                      params: { redirect: Path.Registration },
                  }
        );
    });

    it("ends at the Register line on native; web keeps the web client's sections", async () => {
        await render(<LandingView />);
        const sections = [
            screen.queryByTestId("access-links-header"),
            screen.queryByText("Find your health records"),
            screen.queryByTestId("health-services-links-header"),
        ];
        for (const section of sections) {
            if (Platform.OS === "web") expect(section).toBeTruthy();
            else expect(section).toBeNull();
        }
    });

    // TEMPORARY, with `cypressOrder`: goes back to the navigation order after full Cypress validation
    if (Platform.OS === "web") {
        it("web shows the access tiles in the web client's order", async () => {
            await render(<LandingView />);
            expect(
                screen
                    .getAllByTestId(/^mobile-access-card-(?!carousel)/)
                    .map((card) =>
                        (card as unknown as HTMLElement)
                            .getAttribute("data-testid")
                            ?.replace("mobile-access-card-", "")
                    )
            ).toEqual([
                "HealthRecords",
                "DependentRecords",
                "RecordsManagement",
                "HealthServices",
            ]);
        });
    }

    it("hides the sign-in row once signed in", async () => {
        signInRegistered();
        await render(<LandingView />);
        expect(screen.queryByTestId("btnLogin")).toBeNull();
    });

    it("sizes the finding-records illustration like v-img max-width 300: the PNG's 303 by 251 shape", async () => {
        if (Platform.OS !== "web") return; // native ends at the Register line
        setBreakpoint(1366);
        await render(<LandingView />);
        const image = screen.getByLabelText("Illustration for finding records");
        // the box takes the shape; the image fills it, so it cannot take the PNG's own height
        const img = (image as unknown as HTMLElement).style;
        expect([img.width, img.height]).toEqual(["100%", "100%"]);
        const box = (image as unknown as HTMLElement).parentElement!.style;
        expect(box.maxWidth).toBe("300px");
        expect(box.width).toBe("100%");
        expect(box.height).toBe("");
    });
});
