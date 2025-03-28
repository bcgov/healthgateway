import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import { HospitalVisit } from "@/models/encounter";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { UserComment } from "@/models/userComment";

// The hospital visit timeline entry model
export default class HospitalVisitTimelineEntry extends TimelineEntry {
    public healthService: string;
    public visitType: string;
    public outpatient: boolean;
    public facility: string;
    public provider: string;
    public healthAuthority?: string;
    public admitDateTime: DateWrapper;
    public endDateTime?: DateWrapper;

    private readonly getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: HospitalVisit,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.encounterId,
            EntryType.HospitalVisit,
            DateWrapper.fromIso(model.admitDateTime)
        );
        this.healthService = model.healthService;
        this.visitType = model.visitType;
        this.outpatient = model.visitType === "Outpatient Visit";
        this.facility = model.facility;
        this.healthAuthority = model.healthAuthority;
        this.admitDateTime = DateWrapper.fromIso(model.admitDateTime);
        this.endDateTime = model.endDateTime
            ? DateWrapper.fromIso(model.endDateTime)
            : undefined;
        this.provider = model.provider;
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
