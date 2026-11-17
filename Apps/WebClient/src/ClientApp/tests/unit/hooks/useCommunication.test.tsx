import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { act } from "react";
import { AppState, type AppStateStatus } from "react-native";

import { CommunicationBanner } from "@/components/site/CommunicationBanner";
import { Path } from "@/constants/path";
import { useCommunicationRetrieval } from "@/hooks/useCommunication";
import { type Communication, CommunicationType } from "@/models/communication";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, routerState } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { render, renderHook, screen, unmount, waitFor } from "@/test/ui";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

const SIX_HOURS = 6 * 60 * 60 * 1000;
const START = Date.UTC(2026, 10, 16, 12);

/** The native app: one retrieval in the shell, and a banner on each of the five tabs' pages. */
function FiveTabs() {
    useCommunicationRetrieval();
    return (
        <>
            {[1, 2, 3, 4, 5].map((tab) => (
                <CommunicationBanner key={tab} />
            ))}
        </>
    );
}

describe("useCommunicationRetrieval", () => {
    let now: number;
    let getActive: jest.SpiedFunction<typeof services.communication.getActive>;
    let appStateListeners: ((state: AppStateStatus) => void)[];
    const appStateChanges = async (state: AppStateStatus) => {
        await act(async () => {
            appStateListeners.forEach((listener) => listener(state));
            await Promise.resolve();
        });
    };

    beforeEach(() => {
        now = START;
        appStateListeners = [];
        jest.spyOn(AppState, "addEventListener").mockImplementation(
            (_type, listener) => {
                appStateListeners.push(listener);
                return {
                    remove: () => {
                        appStateListeners = appStateListeners.filter(
                            (l) => l !== listener
                        );
                    },
                };
            }
        );
        jest.spyOn(Date, "now").mockImplementation(() => now);
        setBreakpoint(390);
        resetRouter(Path.Root);
        services.initialize(configFixture, fakeAuth());
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    cache: {
                        default: 3600,
                        services: { Communications: 21600 },
                    },
                },
            },
            status: "loaded",
        });
        getActive = jest
            .spyOn(services.communication, "getActive")
            .mockImplementation(
                async (type) =>
                    ({
                        resourcePayload: {
                            text:
                                type === CommunicationType.InApp
                                    ? "In app message"
                                    : "Public banner",
                            communicationTypeCode: type,
                        } as Communication,
                    }) as never
            );
    });
    afterEach(async () => {
        await unmount();
        resetStores();
        jest.restoreAllMocks();
    });

    it("makes one call for five banners, and every banner shows it", async () => {
        await render(<FiveTabs />);
        await waitFor(() =>
            expect(screen.getAllByTestId("communicationBanner")).toHaveLength(5)
        );
        expect(getActive).toHaveBeenCalledTimes(1);
        expect(getActive).toHaveBeenCalledWith(CommunicationType.Banner);
    });

    it("asks again on a page change only once the configured time has passed", async () => {
        const hook = await renderHook(() => useCommunicationRetrieval());
        expect(getActive).toHaveBeenCalledTimes(1);

        routerState.pathname = Path.Timeline;
        now = START + SIX_HOURS - 1;
        await hook.rerender();
        expect(getActive).toHaveBeenCalledTimes(1);

        routerState.pathname = Path.Home;
        now = START + SIX_HOURS;
        await hook.rerender();
        expect(getActive).toHaveBeenCalledTimes(2);
    });

    it("back in the foreground, asks again only once the configured time has passed", async () => {
        await renderHook(() => useCommunicationRetrieval());
        expect(getActive).toHaveBeenCalledTimes(1);

        now = START + SIX_HOURS - 1;
        await appStateChanges("background");
        await appStateChanges("active");
        expect(getActive).toHaveBeenCalledTimes(1);

        now = START + SIX_HOURS;
        await appStateChanges("background");
        expect(getActive).toHaveBeenCalledTimes(1);
        await appStateChanges("active");
        expect(getActive).toHaveBeenCalledTimes(2);
    });

    it("stops listening for the foreground once unmounted", async () => {
        const hook = await renderHook(() => useCommunicationRetrieval());
        await hook.unmount();
        expect(appStateListeners).toHaveLength(0);
    });

    it("after an environment switch, asks the new environment once its services are ready", async () => {
        await renderHook(() => useCommunicationRetrieval());
        expect(getActive).toHaveBeenCalledTimes(1);
        const other = {
            ...configFixture,
            serviceEndpoints: {
                ...configFixture.serviceEndpoints,
                GatewayApi: "https://api.other/gateway/",
            },
        };
        // bootstrap: the new configuration arrives just before the services built from it
        updateStores(() =>
            useConfigStore.setState({ config: other, status: "loaded" })
        );
        services.initialize(other, fakeAuth());
        const otherGetActive = jest
            .spyOn(services.communication, "getActive")
            .mockResolvedValue({ resourcePayload: null } as never);
        await waitFor(() => expect(otherGetActive).toHaveBeenCalledTimes(1));
        expect(getActive).toHaveBeenCalledTimes(1);
    });

    it("fetches the in-app message once registered, and the banners show it", async () => {
        signInRegistered();
        await render(<FiveTabs />);
        await waitFor(() =>
            expect(screen.getAllByTestId("communicationBanner")).toHaveLength(5)
        );
        expect(getActive).toHaveBeenCalledTimes(1);
        expect(getActive).toHaveBeenCalledWith(CommunicationType.InApp);
    });
});
