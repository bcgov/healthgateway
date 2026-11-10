import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { Platform } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { LoadStatus } from "@/models/datasetState";
import { PatientDataType } from "@/models/patientData";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { useUserStore } from "@/stores/user.store";
import { resetRouter } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { render, screen, textOf, unmount } from "@/test/ui";
import { DependentsView } from "@/views/private/dependents/DependentsView";
import { ReportsView } from "@/views/private/reports/ReportsView";
import { ServicesView } from "@/views/private/services/ServicesView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factory
    () => require("@/test/breakpointMock") as object
);

describe("pages without a connection", () => {
    beforeEach(() => {
        resetRouter("/services");
        services.initialize(configFixture, fakeAuth());
        useConfigStore.setState({ config: configFixture, status: "loaded" });
        signInRegistered();
    });

    afterEach(async () => {
        await unmount();
        updateStores(() =>
            useConnectivityStore.setState({
                isOnline: true,
                serviceUnavailable: false,
                syncPill: false,
                sessionStatus: "live",
            })
        );
        jest.restoreAllMocks();
        resetStores();
    });

    if (Platform.OS === "web") {
        it("web shows no notice", async () => {
            jest.spyOn(services.report, "generateReport");
            await render(<ServicesView />);
            expect(screen.queryByTestId("servicesOfflineNotice")).toBeNull();
        });
        return;
    }

    const offline = () =>
        useConnectivityStore.setState({
            isOnline: false,
            sessionStatus: "offline",
        });

    it("Download says it needs a connection, instead of its form", async () => {
        offline();
        await render(<ReportsView />);
        expect(screen.getByTestId("downloadOfflineNotice")).toBeTruthy();
    });

    it("Services shows the saved status and says it is saved", async () => {
        offline();
        const hdid = useUserStore.getState().user.hdid;
        usePatientDataStore.setState({
            states: {
                [hdid]: {
                    data: {
                        [PatientDataType.OrganDonorRegistrationStatus]: [],
                    },
                    status: LoadStatus.LOADED,
                    statusMessage: "",
                },
            },
        });
        await render(<ServicesView />);
        expect(textOf(screen.getByTestId("servicesOfflineNotice"))).toContain(
            "This is your saved status."
        );
    });

    it("Services without a saved status says so, and does not ask for it", async () => {
        offline();
        const get = jest.spyOn(services.patientData, "getPatientData");
        await render(<ServicesView />);
        expect(textOf(screen.getByTestId("servicesOfflineNotice"))).toContain(
            "isn't saved on this device"
        );
        expect(get).not.toHaveBeenCalled();
    });

    it("Dependents shows the saved list, without asking for it", async () => {
        offline();
        const get = jest.spyOn(services.dependent, "getAll");
        await render(<DependentsView />);
        expect(screen.getByTestId("dependentsOfflineNotice")).toBeTruthy();
        expect(get).not.toHaveBeenCalled();
        useDependentStore.setState({ status: LoadStatus.NONE });
    });

    it("the Syncing pill shows over any page and leaves it usable", async () => {
        useConnectivityStore.setState({ syncPill: true });
        await render(
            <PageScroll>
                <></>
            </PageScroll>
        );
        expect(textOf(screen.getByTestId("syncingPill"))).toContain("Syncing…");
        expect(screen.getByTestId("syncingPill").props.pointerEvents).toBe(
            "none"
        );
    });
});
