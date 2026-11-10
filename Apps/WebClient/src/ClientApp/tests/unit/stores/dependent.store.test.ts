import {
    afterEach,
    beforeEach,
    describe,
    expect,
    it,
    jest,
} from "@jest/globals";

import { LoadStatus } from "@/models/datasetState";
import type { Dependent } from "@/models/dependent";
import { formatDependentName } from "@/models/dependent";
import { ResultError } from "@/models/errors";
import { ReportFilterBuilder } from "@/models/reportFilter";
import { services } from "@/services/container";
import { useDependentStore } from "@/stores/dependent.store";
import { useErrorStore } from "@/stores/error.store";
import { getReportHeaderData } from "@/stores/report";
import {
    configFixture,
    fakeAuth,
    resetStores,
    signInRegistered,
} from "@/test/fixtures";

const dependent = (id: string, dateOfBirth: string): Dependent => ({
    ownerId: id,
    delegateId: "ME",
    totalDelegateCount: 1,
    reasonCode: 0,
    version: 1,
    dependentInformation: {
        hdid: id,
        PHN: "9874307168",
        firstname: "Sam",
        lastname: "Testfive",
        dateOfBirth,
    },
});

describe("useDependentStore (stores/dependent.ts port)", () => {
    beforeEach(() => {
        services.initialize(configFixture, fakeAuth());
        signInRegistered();
    });
    afterEach(() => {
        resetStores();
        jest.restoreAllMocks();
    });

    it("sorts youngest first and serves the stored list unless the cache is bypassed", async () => {
        const getAll = jest
            .spyOn(services.dependent, "getAll")
            .mockResolvedValue([
                dependent("OLD", "2015-01-01"),
                dependent("NEW", "2020-01-01"),
            ]);
        const store = useDependentStore.getState();
        await store.retrieveDependents("ME", false);
        expect(
            useDependentStore.getState().dependents.map((d) => d.ownerId)
        ).toEqual(["NEW", "OLD"]);
        await store.retrieveDependents("ME", false);
        expect(getAll).toHaveBeenCalledTimes(1);
        await store.retrieveDependents("ME", true);
        expect(getAll).toHaveBeenCalledTimes(2);
    });

    it("removes only the matching delegation", async () => {
        jest.spyOn(services.dependent, "removeDependent").mockResolvedValue();
        const keep = dependent("KEEP", "2015-01-01");
        const gone = dependent("GONE", "2015-01-01");
        useDependentStore.setState({ dependents: [keep, gone] });
        await useDependentStore.getState().removeDependent("ME", gone);
        expect(useDependentStore.getState().dependents).toEqual([keep]);
        expect(useDependentStore.getState().status).toBe(LoadStatus.LOADED);
    });

    it("a 429 on load warns and on delete errors; other errors add a banner", async () => {
        const tooMany = new ResultError("X", "busy", 429);
        jest.spyOn(services.dependent, "getAll").mockRejectedValue(tooMany);
        jest.spyOn(services.dependent, "removeDependent").mockRejectedValue(
            tooMany
        );
        const store = useDependentStore.getState();
        await expect(store.retrieveDependents("ME", true)).rejects.toBe(
            tooMany
        );
        expect(useErrorStore.getState().tooManyRequestsWarning).toBe("page");
        await expect(
            store.removeDependent("ME", dependent("A", "2015-01-01"))
        ).rejects.toBe(tooMany);
        expect(useErrorStore.getState().tooManyRequestsError).toBe("page");

        jest.spyOn(services.dependent, "getAll").mockRejectedValue(
            new ResultError("X", "broken", 500, "trace")
        );
        await expect(store.retrieveDependents("ME", true)).rejects.toThrow();
        expect(useErrorStore.getState().errors.length).toBeGreaterThan(0);
        expect(useDependentStore.getState().status).toBe(LoadStatus.ERROR);
    });

    it("a dependent's report header uses the dependent's details, others the patient's", () => {
        useDependentStore.setState({
            dependents: [dependent("KID", "2020-03-15")],
        });
        const filter = ReportFilterBuilder.buildEmpty();
        expect(getReportHeaderData("KID", filter)).toMatchObject({
            phn: "9874307168",
            name: "Sam Testfive",
            dateOfBirth: "2020-Mar-15",
        });
        expect(getReportHeaderData("ME", filter).name).not.toBe("Sam Testfive");
    });

    it("formats a name as first name and last initial", () => {
        expect(
            formatDependentName(
                dependent("A", "2020-01-01").dependentInformation
            )
        ).toBe("Sam T");
        expect(formatDependentName(undefined)).toBe("");
    });
});
