import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder, LaboratoryTest } from "@/models/laboratory";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

const resultOutOfRange = "Out of Range";
const resultInRange = "In Range";

// The laboratory order timeline entry model
export default class LaboratoryOrderTimelineEntry extends TimelineEntry {
    public labPdfId: string;
    public reportingLab: string;
    public reportId: string;
    public collectionDateTime: DateWrapper | undefined;
    public timelineDateTime: DateWrapper;
    public commonName: string;
    public orderingProvider: string;
    public orderStatus: string;
    public reportAvailable: boolean;

    public tests: LaboratoryTestViewModel[];

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: LaboratoryOrder,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.labPdfId,
            EntryType.LaboratoryOrder,
            new DateWrapper(model.timelineDateTime, {
                hasTime: true,
            })
        );

        this.commonName = model.commonName;
        this.reportAvailable = model.reportAvailable;
        this.labPdfId = model.labPdfId;

        if (model.collectionDateTime != null) {
            this.collectionDateTime = new DateWrapper(
                model.collectionDateTime,
                {
                    hasTime: true,
                }
            );
        }

        this.timelineDateTime = new DateWrapper(model.timelineDateTime, {
            hasTime: true,
        });
        this.orderingProvider = model.orderingProvider;
        this.reportingLab = model.reportingSource;

        this.reportId = model.reportId;
        this.orderStatus = model.orderStatus;

        this.tests = [];
        model.laboratoryTests.forEach((test) =>
            this.tests.push(new LaboratoryTestViewModel(test))
        );

        this.sortResults();

        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        let text =
            this.commonName +
            this.reportingLab +
            this.tests.map<string>((t) => t.testName).join();
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
    }

    private sortResults(): void {
        this.tests.sort((a, b) => {
            if (a.result === b.result) {
                return 0;
            }
            if (a.result === resultOutOfRange) {
                return -1;
            }
            if (b.result === resultOutOfRange) {
                return 1;
            }
            if (a.result === resultInRange) {
                return -1;
            }
            if (b.result === resultInRange) {
                return 1;
            }
            return a.result.localeCompare(b.result);
        });
    }
}

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
