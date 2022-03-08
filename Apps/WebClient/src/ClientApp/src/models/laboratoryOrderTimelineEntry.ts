import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import { LaboratoryOrder, LaboratoryTest } from "@/models/laboratory";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

const resultOutOfRange = "Out of Range";
const resultInRange = "In Range";
const statusFinal = "Final";

// The laboratory order timeline entry model
export default class LaboratoryOrderTimelineEntry extends TimelineEntry {
    public laboratoryReportId: string;
    public reportingLab: string;
    public reportId: string;
    public collectionDateTime: DateWrapper;
    public commonName: string;
    public orderingProvider: string;
    public testStatus: string;
    public reportAvailable: boolean;
    public downloadLabel: string | null = null;

    public tests: LaboratoryTestViewModel[];

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: LaboratoryOrder,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.reportId,
            EntryType.LaboratoryOrder,
            new DateWrapper(model.collectionDateTime, {
                hasTime: true,
            })
        );

        this.commonName = model.commonName;
        this.reportAvailable = model.reportAvailable;
        this.laboratoryReportId = model.laboratoryReportId;
        this.collectionDateTime = new DateWrapper(model.collectionDateTime, {
            hasTime: true,
        });
        this.orderingProvider = model.orderingProvider;
        this.reportingLab = model.reportingSource;

        this.reportId = model.reportId;
        this.testStatus = model.testStatus;

        this.tests = [];
        model.laboratoryTests.forEach((test) => {
            this.tests.push(new LaboratoryTestViewModel(test));
        });

        this.sortResults();

        this.downloadLabel = "Incomplete";
        if (this.tests.every((test) => test.testStatus === statusFinal)) {
            this.downloadLabel = "Final";
        }

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

    private sortResults() {
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
    public testStatus: string;

    constructor(model: LaboratoryTest) {
        this.testName = model.batteryType;

        this.result = "Pending";
        if (model.testStatus === statusFinal) {
            this.result = model.outOfRange ? resultOutOfRange : resultInRange;
        }

        this.testStatus = model.testStatus;
    }
}
