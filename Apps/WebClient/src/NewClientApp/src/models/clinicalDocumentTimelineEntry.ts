import { EntryType } from "@/constants/entryType";
import { ClinicalDocument } from "@/models/clinicalDocument";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

// The hospital visit timeline entry model
export default class ClinicalDocumentTimelineEntry extends TimelineEntry {
    public fileId: string;
    public name: string;
    public documentType: string;
    public facilityName: string;
    public discipline: string;
    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: ClinicalDocument,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.id,
            EntryType.ClinicalDocument,
            new DateWrapper(model.serviceDate)
        );
        this.fileId = model.fileId;
        this.name = model.name;
        this.documentType = model.type;
        this.facilityName = model.facilityName;
        this.discipline = model.discipline;
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        const fields = [
            this.name,
            this.documentType,
            this.discipline,
            this.facilityName,
        ];
        const text = fields.join(" ").toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
