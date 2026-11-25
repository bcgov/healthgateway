import type { DateTime } from "luxon";

import { EntryType } from "@/constants/entryType";
import type { MedicationRequest } from "@/models/medicationRequest";
import { fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntry } from "./timelineEntry";

/** Port of models/timeline/specialAuthorityRequestTimelineEntry.ts */
export class SpecialAuthorityRequestTimelineEntry extends TimelineEntry {
    public drugName: string;
    public requestStatus?: string;
    public prescriberName?: string;
    public effectiveDate?: DateTime;
    public expiryDate?: DateTime;
    public referenceNumber: string;

    public constructor(model: MedicationRequest) {
        super(
            model.referenceNumber,
            EntryType.SpecialAuthorityRequest,
            fromIsoDate(model.requestedDate)
        );
        this.drugName = model.drugName ?? "";
        this.requestStatus = model.requestStatus;
        this.prescriberName = [
            model.prescriberFirstName,
            model.prescriberLastName,
        ]
            .filter(Boolean)
            .join(" ");
        this.effectiveDate = model.effectiveDate
            ? fromIsoDate(model.effectiveDate)
            : undefined;
        this.expiryDate = model.expiryDate
            ? fromIsoDate(model.expiryDate)
            : undefined;
        this.referenceNumber = model.referenceNumber;
    }

    protected containsText(keyword: string): boolean {
        return (
            (this.drugName ?? "") +
            (this.prescriberName ?? "") +
            (this.requestStatus ?? "") +
            (this.referenceNumber ?? "")
        )
            .toUpperCase()
            .includes(keyword.toUpperCase());
    }
}
