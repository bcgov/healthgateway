import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";
import { act } from "react";
import { Platform } from "react-native";

import { PageScroll } from "@/components/site/PageScroll";
import { LoadStatus } from "@/models/datasetState";
import { PatientDataType } from "@/models/patientData";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useConnectivityStore } from "@/stores/connectivity.store";
import { useDependentStore } from "@/stores/dependent.store";
import { useImmunizationStore } from "@/stores/immunization.store";
import { useLabResultStore } from "@/stores/labResult.store";
import { useLoadingStore } from "@/stores/loading.store";
import { usePatientDataStore } from "@/stores/patientData.store";
import { refreshDependentsAndRecords } from "@/stores/refreshDatasets";
import { useUserStore } from "@/stores/user.store";
import { resetRouter } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import { advanceTimers, render, screen, unmount } from "@/test/ui";
import { DependentsView } from "@/views/dependents/DependentsView";
import { ServicesView } from "@/views/services/ServicesView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factory
    () => require("@/test/breakpointMock") as object
);
jest.mock("@/utils/refreshFeedback", () => ({
    playRefreshFeedback: jest.fn(async () => undefined),
}));

const DEPENDENT = {
    ownerId: "D1",
    dependentInformation: {
        firstname: "Sam",
        lastname: "T",
        dateOfBirth: "2020-01-01",
        PHN: "1",
    },
    protectedDependent: false,
};

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

describe("pull to refresh on Dependents and Services", () => {
    beforeEach(() => {
        jest.useFakeTimers();
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
                sessionStatus: "live",
                syncing: false,
            })
        );
        jest.useRealTimers();
        jest.restoreAllMocks();
        resetStores();
    });

    describe("refreshDependentsAndRecords", () => {
        it("fetches the list, then only the records already loaded", async () => {
            const list = jest
                .spyOn(services.dependent, "getAll")
                .mockResolvedValue([DEPENDENT] as never);
            const imms = jest
                .spyOn(services.immunization, "getPatientImmunizations")
                .mockResolvedValue(immunizationResultFixture);
            const labs = jest.spyOn(services.laboratory, "getLaboratoryOrders");
            useImmunizationStore.setState({
                states: {
                    D1: {
                        data: [],
                        recommendations: [],
                        status: LoadStatus.LOADED,
                        statusMessage: "",
                    },
                },
            } as never);
            await refreshDependentsAndRecords("HDID1");
            expect(list).toHaveBeenCalledTimes(1);
            expect(imms).toHaveBeenCalledWith("D1");
            expect(labs).not.toHaveBeenCalled();
            expect(useLabResultStore.getState().states.D1).toBeUndefined();
        });

        it("one dependent's failure does not stop the next", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                DEPENDENT,
                { ...DEPENDENT, ownerId: "D2" },
            ] as never);
            const imms = jest
                .spyOn(services.immunization, "getPatientImmunizations")
                .mockRejectedValueOnce(new Error("no"))
                .mockResolvedValue(immunizationResultFixture);
            const loaded = {
                data: [],
                recommendations: [],
                status: LoadStatus.LOADED,
                statusMessage: "",
            };
            useImmunizationStore.setState({
                states: { D1: loaded, D2: loaded },
            } as never);
            await refreshDependentsAndRecords("HDID1");
            expect(imms).toHaveBeenCalledWith("D2");
        });
    });

    if (Platform.OS === "web") {
        it("web has no pull gesture", async () => {
            await render(
                <PageScroll>
                    <ServicesView />
                </PageScroll>
            );
            expect(control()).toBeUndefined();
        });
        return;
    }

    it("Services refetches a stored status, without the page's loading overlay", async () => {
        const hdid = useUserStore.getState().user.hdid;
        const get = jest
            .spyOn(services.patientData, "getPatientData")
            .mockResolvedValue({ items: [] });
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
        await render(
            <PageScroll>
                <ServicesView />
            </PageScroll>
        );
        get.mockClear();
        const show = jest.spyOn(useLoadingStore.getState(), "show");
        await pull();
        expect(get).toHaveBeenCalledTimes(1);
        expect(show).not.toHaveBeenCalled();
    });

    it("Dependents refetches the list", async () => {
        const get = jest
            .spyOn(services.dependent, "getAll")
            .mockResolvedValue([] as never);
        await render(
            <PageScroll>
                <DependentsView />
            </PageScroll>
        );
        await act(async () => undefined);
        get.mockClear();
        await pull();
        expect(get).toHaveBeenCalledTimes(1);
        expect(useDependentStore.getState().status).toBe(LoadStatus.LOADED);
    });

    it("is off while the records can't be fetched, on both pages", async () => {
        useConnectivityStore.setState({ isOnline: false });
        await render(
            <PageScroll>
                <ServicesView />
            </PageScroll>
        );
        expect(control()).toBeUndefined();
        await unmount();
        await render(
            <PageScroll>
                <DependentsView />
            </PageScroll>
        );
        expect(control()).toBeUndefined();
    });
});
