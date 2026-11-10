import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";
import { State } from "react-native-gesture-handler";
import {
    fireGestureHandler,
    getByGestureTestId,
} from "react-native-gesture-handler/jest-utils";
import { useReducedMotion } from "react-native-reanimated";

import {
    drawerStaysOpen,
    Sidebar,
    startsDrawerDrag,
} from "@/components/site/Sidebar";
import { Path } from "@/constants/path";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useLayoutStore } from "@/stores/layout.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    configFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    advanceTimers,
    attrOf,
    inAct,
    isSelected,
    press,
    render,
    screen,
    tagOf,
} from "@/test/ui";

/** A drag on the open drawer's overlay that starts at `x` and moves `dx` sideways, released at `vx` pt/s. */
const dragDrawer = (x: number, dx: number, vx = 0) =>
    inAct(() =>
        fireGestureHandler(getByGestureTestId("sidenavbar-swipe"), [
            { state: State.BEGAN, x, translationX: 0 },
            { state: State.ACTIVE, x: x + dx / 2, translationX: dx / 2 },
            { state: State.ACTIVE, x: x + dx, translationX: dx },
            {
                state: State.END,
                x: x + dx,
                translationX: dx,
                velocityX: vx,
                velocityY: 0,
            },
        ])
    );

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

describe("Sidebar", () => {
    beforeEach(() => {
        resetRouter(Path.Timeline);
        setBreakpoint(1366);
        useLayoutStore.setState({ sidebarOpen: undefined });
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("renders nothing until the user is fully registered", async () => {
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        await render(<Sidebar />);
        expect(screen.queryByTestId("sidenavbar")).toBeNull();
    });

    it("lists the toggled items with the current route active and navigates on press", async () => {
        signInRegistered();
        await render(<Sidebar />);
        for (const id of [
            "menu-btn-home-link",
            "menu-btn-health-records-link",
            "menu-btn-dependents-link",
            "menu-btn-services-link",
            "menu-btn-reports-link",
            "menu-btn-feedback-link",
        ]) {
            expect(screen.getByTestId(id)).toBeTruthy();
        }
        expect(
            isSelected(screen.getByTestId("menu-btn-health-records-link"))
        ).toBe(true);
        expect(isSelected(screen.getByTestId("menu-btn-home-link"))).toBe(
            false
        );
        await press(screen.getByTestId("menu-btn-reports-link"));
        expect(router.push).toHaveBeenCalledWith(Path.Reports);
    });

    it("hides Dependents and Services when their toggles are off, as v-show does", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        const t = configFixture.webClient.featureToggleConfiguration;
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...t,
                        dependents: { ...t.dependents, enabled: false },
                        services: { ...t.services, enabled: false },
                    },
                },
            },
        });
        await render(<Sidebar />);
        const ariaHidden = (id: string) => {
            const node = screen.getByTestId(id) as unknown as {
                getAttribute?: (name: string) => string | null;
                props?: Record<string, unknown>;
            };
            return String(
                node.getAttribute?.("aria-hidden") ??
                    node.props?.["aria-hidden"]
            );
        };
        expect(ariaHidden("menu-btn-dependents-link")).toBe("true");
        expect(ariaHidden("menu-btn-services-link")).toBe("true");
        expect(ariaHidden("menu-btn-home-link")).not.toBe("true");
    });

    it("collapses to an icon rail on desktop and re-expands on press", async () => {
        signInRegistered();
        useLayoutStore.setState({ sidebarOpen: false });
        await render(<Sidebar />);
        expect(screen.queryByText("Health Records")).toBeNull();
        await press(screen.getByTestId("menu-btn-home-link"));
        expect(router.push).not.toHaveBeenCalled();
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
    });

    it("is a temporary drawer on phones: hidden until opened, closes after navigating", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        setBreakpoint(390);
        await render(<Sidebar />);
        // closed, it waits off screen with no scrim
        expect(
            (
                screen.getByTestId("sidenavbar") as unknown as HTMLElement
            ).closest("[inert]")
        ).not.toBeNull();
        expect(screen.queryByLabelText("Close navigation")).toBeNull();
        updateStores(() => useLayoutStore.setState({ sidebarOpen: true }));
        expect(screen.getByLabelText("Close navigation")).toBeTruthy();
        await press(screen.getByTestId("menu-btn-home-link"));
        expect(router.push).toHaveBeenCalledWith(Path.Home);
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
    });

    it("dims the whole page under the open drawer, as v-navigation-drawer's scrim does, with the drawer over it", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        setBreakpoint(390);
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        const scrim = (
            screen.getByLabelText("Close navigation") as unknown as HTMLElement
        ).parentElement!;
        const drawer = (
            screen.getByTestId("sidenavbar") as unknown as HTMLElement
        ).parentElement!;
        // not a strip beside the drawer: the drawer slides over it rather than into a gap
        expect(scrim.style.position).toBe("absolute");
        expect(scrim.style.left).toBe("0px");
        expect(scrim.style.right).toBe("0px");
        expect(scrim.style.opacity).toBe("0.2");
        expect(scrim.contains(drawer)).toBe(false);
        expect(drawer.style.position).toBe("absolute");
        expect(
            scrim.compareDocumentPosition(drawer) &
                Node.DOCUMENT_POSITION_FOLLOWING
        ).toBeTruthy();
    });

    it("drags shut from near its edge, as v-navigation-drawer's touch support does", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        jest.useFakeTimers();
        signInRegistered();
        setBreakpoint(390);
        await render(<Sidebar />);
        updateStores(() => useLayoutStore.setState({ sidebarOpen: true }));
        // a drag that starts in the middle of the drawer does nothing
        await dragDrawer(120, -150);
        await advanceTimers(250);
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
        // under half dragged back, slowly: stays open
        await dragDrawer(250, -100);
        await advanceTimers(250);
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
        // past half from the edge: closes once the drawer has slid away
        await dragDrawer(250, -150);
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
        await advanceTimers(250);
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
        jest.useRealTimers();
    });

    it("closes on a flick to the left, even a short one", async () => {
        if (Platform.OS !== "web") return;
        jest.useFakeTimers();
        signInRegistered();
        setBreakpoint(390);
        await render(<Sidebar />);
        updateStores(() => useLayoutStore.setState({ sidebarOpen: true }));
        await dragDrawer(300, -40, -900);
        await advanceTimers(250);
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
        jest.useRealTimers();
    });

    it("starts a drag only near the drawer's edge, and keeps it open on a flick right", () => {
        expect(startsDrawerDrag(240, 256)).toBe(true);
        expect(startsDrawerDrag(330, 256)).toBe(true);
        expect(startsDrawerDrag(230, 256)).toBe(false);
        expect(startsDrawerDrag(340, 256)).toBe(false);
        expect(drawerStaysOpen(-200, 600, 0, 256)).toBe(true);
        expect(drawerStaysOpen(-20, -600, 0, 256)).toBe(false);
        expect(drawerStaysOpen(-100, -300, 0, 256)).toBe(true);
        expect(drawerStaysOpen(-140, 0, 0, 256)).toBe(false);
    });

    it("closes the temporary drawer on Escape, as useEscapeKey does in SidebarComponent.vue", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        jest.useFakeTimers();
        signInRegistered();
        setBreakpoint(390);
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        expect(screen.getByLabelText("Close navigation")).toBeTruthy();
        updateStores(() =>
            globalThis.dispatchEvent(
                new KeyboardEvent("keydown", { key: "Escape" })
            )
        );
        expect(useLayoutStore.getState().sidebarOpen).toBe(false);
        await advanceTimers(200);
        expect(screen.queryByLabelText("Close navigation")).toBeNull();
        jest.useRealTimers();
    });

    it("slides shut over 0.2 s, as v-navigation-drawer does, out of reach meanwhile; at once when motion is reduced", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        jest.useFakeTimers();
        signInRegistered();
        setBreakpoint(390);
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        const overlay = () =>
            screen.queryByTestId("sidenavbar-overlay") as HTMLElement | null;
        expect(overlay()?.hasAttribute("inert")).toBe(false);
        updateStores(() => useLayoutStore.setState({ sidebarOpen: false }));
        // still on the page while it slides, but the page under it takes presses and focus
        expect(overlay()).not.toBeNull();
        expect(overlay()?.hasAttribute("inert")).toBe(true);
        expect(overlay()?.getAttribute("aria-hidden")).toBe("true");
        expect(overlay()?.style.pointerEvents).toBe("none");
        await advanceTimers(199);
        expect(overlay()).not.toBeNull();
        await advanceTimers(1);
        expect(overlay()).toBeNull();
        // reopened, it is in reach again
        updateStores(() => useLayoutStore.setState({ sidebarOpen: true }));
        expect(overlay()?.hasAttribute("inert")).toBe(false);
        expect(overlay()?.style.pointerEvents).toBe("auto");

        jest.mocked(useReducedMotion).mockReturnValue(true);
        await render(<Sidebar />);
        updateStores(() => useLayoutStore.setState({ sidebarOpen: false }));
        await advanceTimers(0);
        expect(overlay()).toBeNull();
        jest.mocked(useReducedMotion).mockReturnValue(false);
        jest.useRealTimers();
    });

    it("leaves the drawer at once when the window widens, where the permanent drawer takes over", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        setBreakpoint(390);
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        expect(screen.getByTestId("sidenavbar-overlay")).toBeTruthy();
        updateStores(() => setBreakpoint(1366));
        expect(screen.queryByTestId("sidenavbar-overlay")).toBeNull();
    });

    it("leaves Escape to the Feedback dialog open over the drawer", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        setBreakpoint(390);
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        await press(screen.getByTestId("menu-btn-feedback-link"));
        expect(screen.getByTestId("feedback-container")).toBeTruthy();
        updateStores(() => {
            globalThis.dispatchEvent(
                new KeyboardEvent("keydown", { key: "Escape" })
            );
            document.dispatchEvent(
                new KeyboardEvent("keyup", { key: "Escape" })
            );
        });
        // react-native-web closes the dialog on keyup; the drawer stays open
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
    });

    it("ignores Escape on desktop, where the drawer is permanent", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        useLayoutStore.setState({ sidebarOpen: true });
        await render(<Sidebar />);
        updateStores(() =>
            globalThis.dispatchEvent(
                new KeyboardEvent("keydown", { key: "Escape" })
            )
        );
        expect(useLayoutStore.getState().sidebarOpen).toBe(true);
    });

    it("web: the destinations are real links and Feedback, which opens a dialog, is a button; the drawer itself is no tab stop", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        await render(<Sidebar />);
        const home = screen.getByTestId("menu-btn-home-link");
        expect(tagOf(home)).toBe("a");
        expect(attrOf(home, "href")).toBe(Path.Home);
        // a native button: the browser itself presses it on Enter and Space
        expect(tagOf(screen.getByTestId("menu-btn-feedback-link"))).toBe(
            "button"
        );
        expect(attrOf(screen.getByTestId("sidenavbar"), "tabindex")).toBe("-1");
        await press(screen.getByTestId("menu-btn-feedback-link"));
        expect(screen.getByTestId("feedback-container")).toBeTruthy();
    });

    it("marks the current page's item aria-current (web) and the drawer carries the id the header's toggle controls", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        resetRouter(Path.Timeline);
        await render(<Sidebar />);
        expect(
            attrOf(
                screen.getByTestId("menu-btn-health-records-link"),
                "aria-current"
            )
        ).toBe("page");
        expect(
            attrOf(screen.getByTestId("menu-btn-home-link"), "aria-current")
        ).toBeUndefined();
        expect(attrOf(screen.getByTestId("sidenavbar"), "id")).toBe(
            "sidenavbar"
        );
    });

    it("eases its width over 0.2 s when it collapses to the rail, as v-navigation-drawer does, unless motion is reduced", async () => {
        if (Platform.OS !== "web") return; // the sidebar is web only
        signInRegistered();
        await render(<Sidebar />);
        const drawer = screen.getByTestId(
            "sidenavbar"
        ) as unknown as HTMLElement;
        expect(drawer.style.transitionProperty).toBe("width");
        expect(drawer.style.transitionDuration).toBe("200ms");
        expect(drawer.style.transitionTimingFunction).toBe(
            "cubic-bezier(0.4, 0, 0.2, 1)"
        );
        updateStores(() => useLayoutStore.setState({ sidebarOpen: false }));
        expect(drawer.style.width).toBe("56px");
        expect(drawer.style.transitionDuration).toBe("200ms");
        jest.mocked(useReducedMotion).mockReturnValue(true);
        await render(<Sidebar />);
        expect(
            (screen.getByTestId("sidenavbar") as unknown as HTMLElement).style
                .transitionDuration
        ).toBe("0ms");
        jest.mocked(useReducedMotion).mockReturnValue(false);
    });
});
