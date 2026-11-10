import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { TrackedExternalUrl } from "@/constants/tracking";
import { services } from "@/services/container";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter } from "@/test/expoRouterMock";
import { configFixture, fakeAuth, resetStores } from "@/test/fixtures";
import { render, screen, textOf } from "@/test/ui";
import { VppLoginView } from "@/views/auth/VppLoginView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("VppLoginView (VppLoginView.vue port)", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter("/vppLogin");
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("shows the source copy and both actions", async () => {
        await render(<VppLoginView />);
        expect(
            screen.getByText("Do you want to open Health Gateway?")
        ).toBeTruthy();
        expect(
            textOf(screen.getByTestId("signout-close-windows-warning"))
        ).toContain(
            "Please remember to sign out and close both windows when you are done."
        );
        expect(screen.getByTestId("cancel-button")).toBeTruthy();
        expect(screen.getByTestId("continue-to-hgw-button")).toBeTruthy();
        expect(screen.getByTestId("click-hgw-link")).toBeTruthy();
        expect(TrackedExternalUrl.HealthGateway).toBe(
            "https://www.healthgateway.gov.bc.ca"
        );
    });
});
