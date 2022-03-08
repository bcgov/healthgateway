import { EntryType } from "@/constants/entryType";
import { DateWrapper } from "@/models/dateWrapper";
import TimelineEntry from "@/models/timelineEntry";
import { UserComment } from "@/models/userComment";

import MedicationRequest from "./MedicationRequest";

// The medication timeline entry model
export default class MedicationRequestTimelineEntry extends TimelineEntry {
    public drugName?: string;
    public requestStatus?: string;
    public prescriberFirstName?: string;
    public prescriberLastName?: string;
    public effectiveDate: DateWrapper;
    public expiryDate: DateWrapper;
    public referenceNumber: string;

    private getComments: (entyId: string) => UserComment[] | null;

    public constructor(
        model: MedicationRequest,
        getComments: (entyId: string) => UserComment[] | null
    ) {
        super(
            model.referenceNumber,
            EntryType.MedicationRequest,
            new DateWrapper(model.requestedDate)
        );

        this.drugName = model.drugName;
        this.requestStatus = model.requestStatus;
        this.prescriberFirstName = model.prescriberFirstName;
        this.prescriberLastName = model.prescriberLastName;
        this.effectiveDate = new DateWrapper(model.effectiveDate);
        this.expiryDate = new DateWrapper(model.expiryDate);
        this.referenceNumber = model.referenceNumber;
        this.getComments = getComments;
    }

    public get comments(): UserComment[] | null {
        return this.getComments(this.id);
    }

    public containsText(keyword: string): boolean {
        let text =
            (this.drugName || "") +
            (this.prescriberFirstName || "") +
            (this.prescriberLastName || "") +
            (this.requestStatus || "") +
            (this.referenceNumber || "");
        text = text.toUpperCase();
        return text.includes(keyword.toUpperCase());
    }
}
