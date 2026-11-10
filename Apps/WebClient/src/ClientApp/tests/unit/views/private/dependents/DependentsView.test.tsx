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

import { LoadStatus } from "@/models/datasetState";
import type { Dependent } from "@/models/dependent";
import { services } from "@/services/container";
import { useConfigStore } from "@/stores/config.store";
import { useDependentStore } from "@/stores/dependent.store";
import { setBreakpoint } from "@/test/breakpointMock";
import { resetRouter, router } from "@/test/expoRouterMock";
import {
    configFixture,
    fakeAuth,
    immunizationResultFixture,
    laboratoryOrdersFixture,
    requestResult,
    resetStores,
    signInRegistered,
    updateStores,
} from "@/test/fixtures";
import {
    existsByTestId,
    inAct,
    inputValue,
    isDisabled,
    isSelected,
    press,
    render,
    screen,
    textOf,
} from "@/test/ui";
import { formatDate, today } from "@/utils/dateUtil";
import { DependentsView } from "@/views/private/dependents/DependentsView";
import { DependentTimelineView } from "@/views/private/dependents/DependentTimelineView";

jest.mock(
    "@/hooks/useBreakpoint",
    // eslint-disable-next-line @typescript-eslint/no-require-imports -- jest.mock factories are hoisted, so they must require
    () => require("@/test/breakpointMock") as object
);

/** A sideways swipe on an HgWindow (v-window): negative `dx` is a swipe to the left, to the next tab. */
const swipe = (windowTestID: string, dx: number) =>
    inAct(() =>
        fireGestureHandler(getByGestureTestId(`${windowTestID}-swipe`), [
            { state: State.BEGAN, translationX: 0, translationY: 0 },
            { state: State.ACTIVE, translationX: dx / 2, translationY: 0 },
            { state: State.END, translationX: dx, translationY: 0 },
        ])
    );

const toIso = (years: number) =>
    formatDate(today().minus({ years }), "yyyy-MM-dd");

const dependent = (
    id: string,
    years = 5,
    overrides: Partial<Dependent> = {}
): Dependent => ({
    ownerId: id,
    delegateId: "ME",
    totalDelegateCount: 3,
    reasonCode: 0,
    version: 1,
    dependentInformation: {
        hdid: id,
        PHN: `PHN${id}`,
        firstname: "Sam",
        lastname: "Testfive",
        dateOfBirth: toIso(years),
    },
    ...overrides,
});

function useConfig(timelineEnabled: boolean, datasets: string[]) {
    const base = configFixture.webClient.featureToggleConfiguration;
    updateStores(() =>
        useConfigStore.setState({
            config: {
                ...configFixture,
                webClient: {
                    ...configFixture.webClient,
                    featureToggleConfiguration: {
                        ...base,
                        datasets: datasets.map((name) => ({
                            name,
                            enabled: true,
                        })),
                        dependents: {
                            enabled: true,
                            timelineEnabled,
                            datasets: [],
                        },
                    },
                },
            },
            status: "loaded",
        })
    );
}

describe("Dependents", () => {
    beforeEach(() => {
        setBreakpoint(1366);
        resetRouter("/dependents");
        services.initialize(configFixture, fakeAuth());
        jest.spyOn(services.tracking, "trackEvent").mockImplementation(
            () => undefined
        );
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    describe("legacy view (dependents.timelineEnabled off)", () => {
        beforeEach(() =>
            useConfig(false, ["Immunization", "LabResult", "ClinicalDocument"])
        );

        it("shows the card by PHN with its age note, profile and only the enabled tabs", async () => {
            useConfig(false, ["Immunization"]);
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            await render(<DependentsView />);
            expect(
                screen.getByText(/under the age of 12 to view their/)
            ).toBeTruthy();
            expect(screen.getByTestId("dependent-card-PHNKID")).toBeTruthy();
            expect(textOf(screen.getByTestId("dependentName"))).toBe(
                "Sam Testfive"
            );
            expect(inputValue(screen.getByTestId("dependentPHN"))).toBe(
                "PHNKID"
            );
            expect(
                screen.getByTestId("immunization-tab-title-KID")
            ).toBeTruthy();
            expect(
                screen.queryByTestId("lab-results-tab-title-KID")
            ).toBeNull();
            expect(screen.getByTestId("profile-tab-title-KID")).toBeTruthy();
            // web client #6948: a panel configured off is not rendered at all, so it can't be swiped to on mobile
            expect(existsByTestId("immunization-tab-KID")).toBe(true);
            expect(existsByTestId("lab-results-tab-KID")).toBe(false);
            expect(existsByTestId("clinical-document-tab-KID")).toBe(false);
        });

        it("disables the data tabs and offers removal for an aged-out dependent", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("OLD", 14),
            ]);
            const remove = jest
                .spyOn(services.dependent, "removeDependent")
                .mockResolvedValue();
            await render(<DependentsView />);
            expect(screen.getByText("Your access has expired")).toBeTruthy();
            expect(
                isDisabled(screen.getByTestId("immunization-tab-title-OLD"))
            ).toBe(true);
            expect(screen.getByTestId("dependentTab")).toBeTruthy();
            expect(existsByTestId("immunization-tab-OLD")).toBe(false);
            expect(existsByTestId("lab-results-tab-OLD")).toBe(false);
            expect(existsByTestId("clinical-document-tab-OLD")).toBe(false);
            await press(screen.getByText("Remove Dependent"));
            expect(remove).toHaveBeenCalled();
        });

        it("loads immunizations when the tab opens and shows dated history rows", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            const get = jest
                .spyOn(services.immunization, "getPatientImmunizations")
                .mockResolvedValue(immunizationResultFixture);
            await render(<DependentsView />);
            expect(get).not.toHaveBeenCalled();
            await press(screen.getByTestId("immunization-tab-title-KID"));
            expect(get).toHaveBeenCalledWith("KID");
            // pressing the open tab again does not load again
            await press(screen.getByTestId("immunization-tab-title-KID"));
            expect(get).toHaveBeenCalledTimes(1);
            expect(
                textOf(screen.getByTestId("history-immunization-date-KID-0"))
            ).toBe("2023-Oct-15");
            expect(
                screen.getByTestId("dependent-immunization-disclaimer-alert")
            ).toBeTruthy();
            expect(
                screen.getByTestId(
                    "download-immunization-history-report-btn-KID"
                )
            ).toBeTruthy();
        });

        it("opens every tab with data, without React errors, and loads each once", async () => {
            const consoleError = jest
                .spyOn(console, "error")
                .mockImplementation(() => undefined);
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            jest.spyOn(
                services.immunization,
                "getPatientImmunizations"
            ).mockResolvedValue(immunizationResultFixture);
            const labs = jest
                .spyOn(services.laboratory, "getLaboratoryOrders")
                .mockResolvedValue(
                    requestResult({
                        orders: laboratoryOrdersFixture(),
                        loaded: true,
                        queued: false,
                        retryin: 0,
                    })
                );
            const documents = jest
                .spyOn(services.clinicalDocument, "getRecords")
                .mockResolvedValue(
                    requestResult([
                        {
                            id: "1",
                            fileId: "F",
                            name: "Discharge summary",
                            type: "Summary",
                            facilityName: "Lions Gate Hospital",
                            discipline: "Cancer",
                            serviceDate: "2021-11-15",
                        },
                    ])
                );
            await render(<DependentsView />);
            await press(screen.getByTestId("immunization-tab-title-KID"));
            await press(screen.getByText("Schedule"));
            expect(screen.getByTestId("schedule-due-date-KID-0")).toBeTruthy();
            await press(screen.getByTestId("lab-results-tab-title-KID"));
            expect(labs).toHaveBeenCalledWith("KID");
            expect(screen.getByTestId("lab-results-date-KID-0")).toBeTruthy();
            await press(screen.getByTestId("clinical-document-tab-title-KID"));
            expect(documents).toHaveBeenCalledWith("KID");
            expect(
                textOf(screen.getByTestId("clinical-document-name-KID-0"))
            ).toBe("Discharge summary");
            await press(screen.getByTestId("lab-results-tab-title-KID"));
            expect(labs).toHaveBeenCalledTimes(1);
            expect(consoleError).not.toHaveBeenCalled();
        });

        it("swipes between the enabled tabs, loading each as it opens, and between History and Schedule", async () => {
            useConfig(false, ["Immunization", "ClinicalDocument"]);
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            const immunizations = jest
                .spyOn(services.immunization, "getPatientImmunizations")
                .mockResolvedValue(immunizationResultFixture);
            const documents = jest
                .spyOn(services.clinicalDocument, "getRecords")
                .mockResolvedValue(requestResult([]));
            await render(<DependentsView />);
            await swipe("dependent-window-KID", -60);
            expect(
                isSelected(screen.getByTestId("immunization-tab-title-KID"))
            ).toBe(true);
            expect(immunizations).toHaveBeenCalledWith("KID");
            // the nested window takes its own swipe
            await swipe("immunization-window-KID", -60);
            expect(screen.getByTestId("schedule-due-date-KID-0")).toBeTruthy();
            // Lab Results is configured off, so the next swipe skips to Clinical Docs, and that is the end
            await swipe("dependent-window-KID", -60);
            expect(
                isSelected(
                    screen.getByTestId("clinical-document-tab-title-KID")
                )
            ).toBe(true);
            expect(documents).toHaveBeenCalledWith("KID");
            await swipe("dependent-window-KID", -60);
            expect(
                isSelected(
                    screen.getByTestId("clinical-document-tab-title-KID")
                )
            ).toBe(true);
            await swipe("dependent-window-KID", 60);
            await swipe("dependent-window-KID", 60);
            expect(
                isSelected(screen.getByTestId("profile-tab-title-KID"))
            ).toBe(true);
        });

        it("doesn't swipe an aged-out dependent off the profile", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("OLD", 14),
            ]);
            const immunizations = jest.spyOn(
                services.immunization,
                "getPatientImmunizations"
            );
            await render(<DependentsView />);
            await swipe("dependent-window-OLD", -60);
            expect(
                isSelected(screen.getByTestId("profile-tab-title-OLD"))
            ).toBe(true);
            expect(immunizations).not.toHaveBeenCalled();
        });

        it("hides the wide columns on a phone", async () => {
            setBreakpoint(390);
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            jest.spyOn(
                services.immunization,
                "getPatientImmunizations"
            ).mockResolvedValue(immunizationResultFixture);
            await render(<DependentsView />);
            await press(screen.getByTestId("immunization-tab-title-KID"));
            expect(screen.queryByTestId("history-lot-number-KID-0")).toBeNull();
            expect(
                screen.getByTestId("history-provider-clinic-KID-0")
            ).toBeTruthy();
        });

        it("downloads the immunization report through the sensitive document prompt", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            jest.spyOn(
                services.immunization,
                "getPatientImmunizations"
            ).mockResolvedValue(immunizationResultFixture);
            const generate = jest
                .spyOn(services.report, "generateReport")
                .mockResolvedValue(
                    requestResult({ fileName: "r.pdf", data: "AAAA" })
                );
            await render(<DependentsView />);
            await press(screen.getByTestId("immunization-tab-title-KID"));
            await press(
                screen.getByTestId(
                    "download-immunization-history-report-btn-KID"
                )
            );
            await press(
                screen.getByTestId(
                    "download-immunization-history-report-pdf-btn-KID"
                )
            );
            await press(screen.getByTestId("generic-message-submit-btn"));
            expect(generate.mock.calls[0][0]).toMatchObject({
                template: "DependentImmunization",
                type: "PDF",
            });
        });

        it("confirms before removing, with the legacy button text", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            const remove = jest
                .spyOn(services.dependent, "removeDependent")
                .mockResolvedValue();
            await render(<DependentsView />);
            await press(screen.getByTestId("dependentMenuBtn"));
            await press(screen.getByTestId("deleteDependentMenuBtn"));
            expect(
                textOf(screen.getByTestId("generic-message-submit-btn"))
            ).toBe("Remove Dependent");
            await press(screen.getByTestId("generic-message-ok-btn"));
            expect(remove).not.toHaveBeenCalled();
        });
    });

    describe("timeline view (dependents.timelineEnabled on)", () => {
        beforeEach(() => useConfig(true, ["Immunization"]));

        it("shows the card by hdid with a first name and initial, and opens the timeline", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            await render(<DependentsView />);
            expect(screen.getByTestId("dependent-card-KID")).toBeTruthy();
            expect(textOf(screen.getByTestId("dependentName"))).toBe("Sam T");
            await press(
                screen.getByTestId("dependent-health-records-button-KID")
            );
            expect(router.push).toHaveBeenCalledWith(
                "/dependents/KID/timeline"
            );
        });

        it("shows profile details and how many others have access", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            await render(<DependentsView />);
            await press(screen.getByText("Profile"));
            expect(inputValue(screen.getByTestId("dependent-phn"))).toBe(
                "PHNKID"
            );
            expect(
                inputValue(screen.getByTestId("dependent-other-delegate-count"))
            ).toBe("2");
        });

        it("an aged-out dependent gets the expired panel and disabled tabs", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("OLD", 14),
            ]);
            const remove = jest
                .spyOn(services.dependent, "removeDependent")
                .mockResolvedValue();
            await render(<DependentsView />);
            expect(
                screen.getByTestId("dependent-is-expired-div-OLD")
            ).toBeTruthy();
            await press(screen.getByTestId("remove-dependent-btn-OLD"));
            expect(remove).toHaveBeenCalled();
        });

        it("swipes to Export and Profile, but not for an aged-out dependent", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
                dependent("OLD", 14),
            ]);
            await render(<DependentsView />);
            // the Export tab mounts on first open, so nothing mounted means the aged-out card didn't move
            await swipe("dependent-window-OLD", -60);
            expect(existsByTestId("report-type")).toBe(false);
            await swipe("dependent-window-KID", -60);
            expect(screen.getByTestId("report-type")).toBeTruthy();
            await swipe("dependent-window-KID", -60);
            expect(inputValue(screen.getByTestId("dependent-phn"))).toBe(
                "PHNKID"
            );
        });

        it("always refreshes the list", async () => {
            const getAll = jest
                .spyOn(services.dependent, "getAll")
                .mockResolvedValue([]);
            updateStores(() =>
                useDependentStore.setState({ status: LoadStatus.LOADED })
            );
            await render(<DependentsView />);
            expect(getAll).toHaveBeenCalled();
        });
    });

    describe("DependentTimelineView", () => {
        beforeEach(() => useConfig(true, []));

        it("sends an unknown dependent to /unauthorized", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([]);
            await render(<DependentTimelineView id="STRANGER" />);
            expect(router.push).toHaveBeenCalledWith("/unauthorized");
        });

        it("titles the page, breadcrumbs the name and goes back to the list", async () => {
            jest.spyOn(services.dependent, "getAll").mockResolvedValue([
                dependent("KID"),
            ]);
            await render(<DependentTimelineView id="KID" />);
            expect(textOf(screen.getByTestId("page-title"))).toContain(
                "Health Records for Sam T"
            );
            expect(
                textOf(screen.getByTestId("breadcrumb-dependent-name"))
            ).toContain("Sam T");
            expect(screen.queryByTestId("add-comment-text-area")).toBeNull();
            await press(screen.getByTestId("backBtn"));
            expect(
                Platform.OS === "web" ? router.push : router.dismissTo
            ).toHaveBeenCalledWith("/dependents");
        });
    });
});
