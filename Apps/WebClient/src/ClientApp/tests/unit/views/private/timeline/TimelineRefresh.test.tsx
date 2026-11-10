import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { act, useState } from "react";
import { Platform } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { EntryType } from "@/constants/entryType";
import { services } from "@/services/container";
import { useAppLockStore } from "@/stores/appLock.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useErrorStore } from "@/stores/error.store";
import { markTimelineDatasetsStale } from "@/stores/refreshDatasets";
import { setBreakpoint } from "@/test/breakpointMock";
import { routerState } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { advanceTimers, render, screen, textOf, unmount } from "@/test/ui";
import { playRefreshFeedback } from "@/utils/refreshFeedback";
import { Timeline } from "@/views/private/timeline/Timeline";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/refreshFeedback", () => ({
    playRefreshFeedback: jest.fn(async () => undefined),
}));

describe("Timeline pull to refresh", () => {
    const fetch = () =>
        jest.spyOn(services.immunization, "getPatientImmunizations");

    const page = (refreshable: boolean) => (
        <PageScroll>
            <Timeline
                hdid="HDID1"
                entryTypes={[EntryType.Immunization]}
                refreshable={refreshable}
            />
        </PageScroll>
    );

    const control = () =>
        (
            screen.queryByTestId("pageScroll")?.props as
                | {
                      refreshControl?: {
                          props: { onRefresh: () => Promise<void> };
                      };
                  }
                | undefined
        )?.refreshControl;
    /** The refresh, past the indicator's minimum time on screen. */
    const pull = async () => {
        let finished: Promise<void> | undefined;
        await act(async () => {
            finished = control()?.props.onRefresh();
        });
        await advanceTimers(1200);
        await act(async () => {
            await finished;
        });
    };

    beforeEach(() => {
        jest.useFakeTimers();
        jest.mocked(playRefreshFeedback).mockClear();
        setBreakpoint(390);
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });

    afterEach(async () => {
        await unmount();
        resetStores();
        jest.restoreAllMocks();
        jest.useRealTimers();
    });

    if (Platform.OS === "web") {
        it("is not on web, where the browser reloads the page", async () => {
            fetch().mockResolvedValue(immunizationResultFixture);
            await render(page(true));
            expect(control()).toBeUndefined();
        });
        return;
    }

    it("is only offered where the page asks for it", async () => {
        fetch().mockResolvedValue(immunizationResultFixture);
        await render(page(false));
        expect(control()).toBeUndefined();
    });

    it("fetches again and shows the new records", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        expect(spy).toHaveBeenCalledTimes(1);
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 to 2 out of 2 records"
        );

        spy.mockResolvedValue({
            ...immunizationResultFixture,
            immunizations: immunizationResultFixture.immunizations.slice(0, 1),
        });
        await pull();
        expect(spy).toHaveBeenCalledTimes(2);
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 out of 1 records"
        );
    });

    it("gives the haptic, sound and visual feedback, briefly", async () => {
        fetch().mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        expect(screen.queryByTestId("pageRefreshIndicator")).toBeNull();

        await act(async () => {
            void control()?.props.onRefresh();
        });
        expect(playRefreshFeedback).toHaveBeenCalledTimes(1);
        expect(screen.getByTestId("pageRefreshIndicator")).toBeTruthy();
        expect(screen.getByText("Refreshing…")).toBeTruthy();

        // still up after the fetch itself finished, then gone
        await advanceTimers(600);
        expect(screen.getByTestId("pageRefreshIndicator")).toBeTruthy();
        await advanceTimers(700);
        expect(screen.queryByTestId("pageRefreshIndicator")).toBeNull();
    });

    it("only flashes the Refreshing pill; the spinner stays while the fetch runs", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        spy.mockReturnValue(new Promise(() => undefined));
        await act(async () => {
            void control()?.props.onRefresh();
        });
        expect(screen.getByTestId("pageRefreshIndicator")).toBeTruthy();
        await advanceTimers(1300);
        expect(screen.queryByTestId("pageRefreshIndicator")).toBeNull();
        expect(
            (control()?.props as { refreshing?: boolean } | undefined)
                ?.refreshing
        ).toBe(true);
    });

    it("only flashes the Retrieving pill; the placeholders stay while loading", async () => {
        fetch().mockReturnValue(new Promise(() => undefined));
        await render(page(true));
        expect(screen.getByTestId("loading-toast")).toBeTruthy();
        await advanceTimers(1300);
        expect(screen.queryByTestId("loading-toast")).toBeNull();
        expect(screen.getByTestId("content-placeholders")).toBeTruthy();
    });

    it("empties the list, then builds it up again as the records arrive", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 to 2 out of 2 records"
        );

        let arrive: (value: typeof immunizationResultFixture) => void = () =>
            undefined;
        spy.mockReturnValue(new Promise((resolve) => (arrive = resolve)));
        await act(async () => {
            void control()?.props.onRefresh();
        });
        expect(screen.queryByTestId("timeline-record-count")).toBeNull();
        expect(screen.getByTestId("content-placeholders")).toBeTruthy();

        await act(async () => {
            arrive(immunizationResultFixture);
        });
        expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
            "Displaying 1 to 2 out of 2 records"
        );
    });

    it("leaves a source that fails empty, behind the error, as a browser refresh does", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        await render(page(true));

        spy.mockRejectedValue(new Error("offline"));
        await pull();
        expect(screen.queryByTestId("timeline-record-count")).toBeNull();
        expect(useErrorStore.getState().errors.length).toBeGreaterThan(0);
    });

    describe("with device security on (a saved copy)", () => {
        beforeEach(() => {
            useAppLockStore.setState({ enabled: true });
        });
        afterEach(() => {
            updateStores(() => useAppLockStore.setState({ enabled: false }));
        });

        it("keeps the records on screen while the new ones load", async () => {
            const spy = fetch();
            spy.mockResolvedValue(immunizationResultFixture);
            await render(page(true));
            spy.mockReturnValue(new Promise(() => undefined));
            await act(async () => {
                void control()?.props.onRefresh();
            });
            expect(spy).toHaveBeenCalledTimes(2);
            expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
                "Displaying 1 to 2 out of 2 records"
            );
        });

        it("keeps a source's saved records when its sync fails", async () => {
            const spy = fetch();
            spy.mockResolvedValue(immunizationResultFixture);
            await render(page(true));
            spy.mockRejectedValue(new Error("offline"));
            await pull();
            expect(textOf(screen.getByTestId("timeline-record-count"))).toBe(
                "Displaying 1 to 2 out of 2 records"
            );
            expect(useErrorStore.getState().errors.length).toBeGreaterThan(0);
        });
    });

    it("clears the old error before trying again", async () => {
        const spy = fetch();
        spy.mockRejectedValue(new Error("offline"));
        await render(page(true));
        expect(useErrorStore.getState().errors.length).toBeGreaterThan(0);

        spy.mockResolvedValue(immunizationResultFixture);
        await pull();
        expect(useErrorStore.getState().errors).toHaveLength(0);
    });

    it("offers no pull while the app cannot fetch", async () => {
        fetch().mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        await act(async () => {
            useConnectivityStore.setState({ isOnline: false });
        });
        expect(control()).toBeUndefined();
        await act(async () => {
            useConnectivityStore.setState({ isOnline: true });
        });
    });

    it("does not sync when the records come back into view, however long they were away", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        // renders the page again on demand, so it reads the new focus
        let renderAgain: () => void = () => undefined;
        function Host() {
            const [, setTick] = useState(0);
            renderAgain = () => setTick((n) => n + 1);
            return page(true);
        }
        await render(<Host />);
        expect(spy).toHaveBeenCalledTimes(1);
        // the old rule synced once the records had been away 30 seconds
        const now = Date.now();
        jest.spyOn(Date, "now").mockReturnValue(now + 60 * 60 * 1000);

        for (const focused of [false, true]) {
            routerState.focused = focused;
            await act(async () => renderAgain());
        }
        await advanceTimers(0);
        expect(spy).toHaveBeenCalledTimes(1);
        routerState.focused = true;
    });

    it("syncs when asked: back online, or logged in again", async () => {
        const spy = fetch();
        spy.mockResolvedValue(immunizationResultFixture);
        await render(page(true));
        await act(async () => {
            markTimelineDatasetsStale("HDID1");
            useConnectivityStore.getState().requestSync();
        });
        await advanceTimers(0);
        expect(spy).toHaveBeenCalledTimes(2);
    });

    it("a pull during a first load joins it instead of asking again", async () => {
        const spy = fetch();
        spy.mockReturnValue(new Promise(() => undefined));
        await render(page(true));
        await act(async () => {
            void control()?.props.onRefresh();
        });
        await advanceTimers(1200);
        expect(spy).toHaveBeenCalledTimes(1);
    });
});
