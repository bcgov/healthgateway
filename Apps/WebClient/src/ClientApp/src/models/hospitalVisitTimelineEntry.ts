import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import { HospitalVisit } from "@/models/encounter";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The clinical document timeline entry model
export default class HospitalVisitTimelineEntry extends TimelineEntry {
    public healthService?: string;
    public visitType?: string;
    public outpatient: boolean;
    public facility?: string;
    public provider?: string;
    public healthAuthority?: string;
    public admitDateTime: DateWrapper;
    public endDateTime: DateWrapper;

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: HospitalVisit,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.encounterId,
            EntryType.HospitalVisit,
            new DateWrapper(model.admitDateTime, { hasTime: true })
        );
        this.healthService = model.healthService;
        this.visitType = model.visitType;
        this.outpatient = model.visitType === "Outpatient Visit";
        this.facility = model.facility;
        this.healthAuthority = model.healthAuthority;
        this.admitDateTime = new DateWrapper(model.admitDateTime, {
            hasTime: true,
        });
        this.endDateTime = new DateWrapper(model.endDateTime, {
            hasTime: true,
        });
        this.provider = model.provider.length > 0 ? model.provider[0] : "";
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        const fields = [
            this.facility,
            this.healthService,
            this.visitType,
            this.healthAuthority,
            this.provider,
        ];
        const text = fields.join(" ").toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
