import type { DateTime } from "luxon";

import { EntryType } from "@/constants/entryType";
import type { LaboratoryOrder, LaboratoryTest } from "@/models/laboratory";
import { fromIso } from "@/utils/dateUtil";

import { TimelineEntry, type UserComment } from "./timelineEntry";

const resultOutOfRange = "Out of Range";
const resultInRange = "-"; // HealthGateway.Laboratory.MapProfiles.LaboratoryTestProfile

export class LaboratoryTestViewModel {
    public testName: string;
    public result: string;
    public status: string;
    public statusInfo: string[];

    constructor(model: LaboratoryTest) {
        this.testName = model.batteryType;
        this.result = model.result;
        this.status = model.testStatus;
        this.statusInfo = LaboratoryTestViewModel.getStatusText(this.status);
    }

    private static getStatusText(status: string): string[] {
        switch (status?.toUpperCase()) {
            case "PENDING":
                return [
                    "Most results are ready about 3 days after your test.",
                    "Pathology tests, like tissue biopsies, can take several weeks.",
                ];
            case "COMPLETED":
            case "CORRECTED":
                return ["Download the PDF to see your detailed results."];
            case "CANCELLED":
                return [
                    "Contact your care provider if you have any questions.",
                ];
            default:
                return [];
        }
    }
}

/** Port of models/timeline/labResultTimelineEntry.ts */
export class LabResultTimelineEntry extends TimelineEntry {
    public labPdfId: string;
    public reportingLab: string;
    public reportId: string;
    public collectionDateTime: DateTime | undefined;
    public timelineDateTime: DateTime;
    public commonName: string;
    public orderingProvider: string;
    public orderStatus: string;
    public reportAvailable: boolean;
    public tests: LaboratoryTestViewModel[];

    private readonly getComments: (entryId: string) => UserComment[] | null;

    public constructor(
        model: LaboratoryOrder,
        getComments: (entryId: string) => UserComment[] | null = () => null
    ) {
        super(
            model.labPdfId,
            EntryType.LabResult,
            fromIso(model.timelineDateTime)
        );
        this.commonName = model.commonName;
        this.reportAvailable = model.reportAvailable;
        this.labPdfId = model.labPdfId;
        if (model.collectionDateTime)
            this.collectionDateTime = fromIso(model.collectionDateTime);
        this.timelineDateTime = fromIso(model.timelineDateTime);
        this.orderingProvider = model.orderingProvider;
        this.reportingLab = model.reportingSource;
        this.reportId = model.reportId;
        this.orderStatus = model.orderStatus;
        this.tests = model.laboratoryTests.map(
            (test) => new LaboratoryTestViewModel(test)
        );
        this.sortResults();
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        const text = (
            this.commonName +
            this.reportingLab +
            this.tests.map((t) => t.testName).join()
        ).toUpperCase();
        return text.includes(keyword.toUpperCase());
    }

    /** Out of range first, then in range, then the rest alphabetically */
    private sortResults(): void {
        this.tests.sort((a, b) => {
            if (a.result === b.result) return 0;
            if (a.result === resultOutOfRange) return -1;
            if (b.result === resultOutOfRange) return 1;
            if (a.result === resultInRange) return -1;
            if (b.result === resultInRange) return 1;
            return a.result.localeCompare(b.result);
        });
    }
}
