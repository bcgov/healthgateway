import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { type ComponentType, lazy, Suspense } from "react";
import { Platform } from "react-native";

import { AppShell } from "@/components/site/AppShell";
import { AppText } from "@/components/ui/AppText";
import { PageTitle } from "@/components/ui/PageTitle";
import { Path } from "@/constants/path";
import { resetPageTitles } from "@/hooks/usePageTitle";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useLoadingStore } from "@/stores/loading.store";
import { expectNoAxeViolations } from "@/test/axe";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { inAct, render, screen, waitFor } from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
// the idle timer is its own test
jest.mock("@/components/site/IdleTimeout", () => ({ IdleTimeout: () => null }));

describe("AppShell (web)", () => {
    beforeEach(() => {
        resetRouter(Path.Home);
        setBreakpoint(1366);
        resetPageTitles();
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        signInRegistered();
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("has the landmarks, a skip link first, the page's document title, and moves the focus to main after a route change", async () => {
        if (Platform.OS !== "web") return;
        await render(
            <AppShell>
                <PageTitle title="Home" />
                <AppText>Body</AppText>
            </AppShell>
        );
        expect(screen.getByRole("banner")).toBeTruthy();
        expect(screen.getByRole("navigation", "Main")).toBeTruthy();
        expect(screen.getByRole("contentinfo")).toBeTruthy();
        const main = screen.getByRole("main") as unknown as HTMLElement;
        expect(main.id).toBe("main");
        expect(main.getAttribute("tabindex")).toBe("-1");
        const skip = document.querySelector(
            'a[href="#main"]'
        ) as HTMLAnchorElement;
        expect(skip.textContent).toBe("Skip to main content");
        expect(document.body.querySelector("a, button, [tabindex='0']")).toBe(
            skip
        );
        expect(document.title).toBe("Home | Health Gateway");
        expect(document.activeElement).toBe(document.body);
        // a client-side route change: the shell re-renders with the new path
        routerState.pathname = Path.Profile;
        updateStores(() => setBreakpoint(1366));
        await waitFor(() => expect(document.activeElement).toBe(main));
        // `aria-selected` on the drawer's links is Vuetify parity the owner is still to decide on (WP3)
        await expectNoAxeViolations(document.body, ["aria-allowed-attr"]);
    });
    it("on a phone, adds the footer only once the page has drawn, so the page does not push it down the screen", async () => {
        if (Platform.OS !== "web") return;
        setBreakpoint(400);
        // a route whose code is still loading, as Expo Router's async routes are on first load
        let load: (page: { default: ComponentType }) => void = () => undefined;
        const Page = lazy(
            () =>
                new Promise<{ default: ComponentType }>((resolve) => {
                    load = resolve;
                })
        );
        await render(
            <AppShell>
                <Suspense fallback={null}>
                    <Page />
                </Suspense>
            </AppShell>
        );
        expect(screen.queryByTestId("footer")).toBeNull();
        await inAct(() => load({ default: () => <AppText>Body</AppText> }));
        await waitFor(() => expect(screen.getByTestId("footer")).toBeTruthy());
    });
    it("on a phone, adds the footer only once the page has loaded, and keeps it while a dialog loads", async () => {
        if (Platform.OS !== "web") return;
        setBreakpoint(400);
        const { show, hide } = useLoadingStore.getState();
        show("page");
        await render(
            <AppShell>
                <AppText>
                    Breadcrumb and title, the content still loading
                </AppText>
            </AppShell>
        );
        expect(screen.queryByTestId("footer")).toBeNull();
        await inAct(() => hide("page"));
        expect(screen.getByTestId("footer")).toBeTruthy();
        await inAct(() => show("dialog"));
        expect(screen.getByTestId("footer")).toBeTruthy();
        await inAct(() => hide("dialog"));
    });
});
