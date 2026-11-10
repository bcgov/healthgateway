import { EntryType } from "@/constants/entryType";

import { BcCancerScreeningEntry } from "./BcCancerScreeningEntry";
import { ClinicalDocumentEntry } from "./ClinicalDocumentEntry";
import { Covid19TestResultEntry } from "./Covid19TestResultEntry";
import { DiagnosticImagingEntry } from "./DiagnosticImagingEntry";
import type { TimelineEntryProps } from "./entryProps";
import { HealthVisitEntry } from "./HealthVisitEntry";
import { HospitalVisitEntry } from "./HospitalVisitEntry";
import { ImmunizationEntry } from "./ImmunizationEntry";
import { LabResultEntry } from "./LabResultEntry";
import { MedicationEntry } from "./MedicationEntry";
import { NoteEntry } from "./NoteEntry";
import { SpecialAuthorityRequestEntry } from "./SpecialAuthorityRequestEntry";

export type { TimelineEntryProps } from "./entryProps";

/** entryTypeMap[type].component: each dataset's entry component, added sub-chunk by sub-chunk. */
const entryComponents: Partial<
    Record<EntryType, (props: TimelineEntryProps) => React.ReactNode>
> = {
    [EntryType.Immunization]: ImmunizationEntry,
    [EntryType.Medication]: MedicationEntry,
    [EntryType.LabResult]: LabResultEntry,
    [EntryType.Covid19TestResult]: Covid19TestResultEntry,
    [EntryType.HealthVisit]: HealthVisitEntry,
    [EntryType.Note]: NoteEntry,
    [EntryType.SpecialAuthorityRequest]: SpecialAuthorityRequestEntry,
    [EntryType.ClinicalDocument]: ClinicalDocumentEntry,
    [EntryType.HospitalVisit]: HospitalVisitEntry,
    [EntryType.DiagnosticImaging]: DiagnosticImagingEntry,
    [EntryType.BcCancerScreening]: BcCancerScreeningEntry,
};

export function TimelineEntryView(props: TimelineEntryProps) {
    const Component = entryComponents[props.entry.type];
    return Component ? <Component {...props} /> : null;
}
