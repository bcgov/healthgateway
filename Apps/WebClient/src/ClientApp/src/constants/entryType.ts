import { CommentEntryType } from "@/constants/commentEntryType";

export enum EntryType {
    ClinicalDocument = "ClinicalDocument",
    Covid19TestResult = "Covid19TestResult",
    HealthVisit = "HealthVisit",
    HospitalVisit = "HospitalVisit",
    Immunization = "Immunization",
    LabResult = "LabResult",
    Medication = "Medication",
    Note = "Note",
    SpecialAuthorityRequest = "SpecialAuthority",
}

export class EntryTypeDetails {
    type!: EntryType;
    name!: string;
    description!: string;
    icon!: string;
    component!: string;
    commentType!: CommentEntryType;
    eventName!: string;
    moduleName!: string;
}

const entryTypeMap = new Map<EntryType | undefined, EntryTypeDetails>();

export function getEntryTypeByModule(
    module: string
): EntryTypeDetails | undefined {
    return Array.from(entryTypeMap.values()).find(
        (e) => e.moduleName === module
    );
}

entryTypeMap.set(EntryType.Immunization, {
    type: EntryType.Immunization,
    commentType: CommentEntryType.Immunization,
    name: "Immunizations",
    description:
        "View immunizations you received from public health and community pharmacies",
    icon: "syringe",
    component: "ImmunizationTimelineComponent",
    eventName: "immunizations",
    moduleName: "Immunization",
});

entryTypeMap.set(EntryType.Medication, {
    type: EntryType.Medication,
    commentType: CommentEntryType.Medication,
    name: "Medications",
    description: "See your medication history dating back to 1995",
    icon: "pills",
    component: "MedicationTimelineComponent",
    eventName: "medications",
    moduleName: "Medication",
});

entryTypeMap.set(EntryType.LabResult, {
    type: EntryType.LabResult,
    commentType: CommentEntryType.LabResult,
    name: "Lab Results",
    description:
        "Find out your lab results within about 48 hours of taking a test",
    icon: "microscope",
    component: "LaboratoryOrderTimelineComponent",
    eventName: "lab_results",
    moduleName: "AllLaboratory",
});

entryTypeMap.set(EntryType.Covid19TestResult, {
    type: EntryType.Covid19TestResult,
    commentType: CommentEntryType.Covid19TestResult,
    name: "COVID‑19 Tests",
    description:
        "View and download your COVID‑19 test results as soon as they are available",
    icon: "vial",
    component: "Covid19LaboratoryOrderTimelineComponent",
    eventName: "covid_test",
    moduleName: "Laboratory",
});

entryTypeMap.set(EntryType.HealthVisit, {
    type: EntryType.HealthVisit,
    commentType: CommentEntryType.HealthVisit,
    name: "Health Visits",
    description:
        "See the last seven years of your health visits billed to the BC Medical Services Plan",
    icon: "stethoscope",
    component: "EncounterTimelineComponent",
    eventName: "health_visits",
    moduleName: "Encounter",
});

entryTypeMap.set(EntryType.Note, {
    type: EntryType.Note,
    commentType: CommentEntryType.None,
    name: "My Notes",
    description: "Create and edit your own notes on your health records",
    icon: "edit",
    component: "NoteTimelineComponent",
    eventName: "my_notes",
    moduleName: "Note",
});

entryTypeMap.set(EntryType.SpecialAuthorityRequest, {
    type: EntryType.SpecialAuthorityRequest,
    commentType: CommentEntryType.SpecialAuthorityRequest,
    name: "Special Authority",
    description:
        "Check the status of your Special Authority Requests since March 2021",
    icon: "file-medical",
    component: "MedicationRequestTimelineComponent",
    eventName: "special_authority",
    moduleName: "MedicationRequest",
});

entryTypeMap.set(EntryType.ClinicalDocument, {
    type: EntryType.ClinicalDocument,
    commentType: CommentEntryType.ClinicalDocument,
    name: "Clinical Documents",
    description:
        "View documents shared by your care providers. You can get consultation notes, hospital discharge summaries, outpatient clinic notes and more.",
    icon: "file-waveform",
    component: "ClinicalDocumentTimelineComponent",
    eventName: "document",
    moduleName: "ClinicalDocument",
});

entryTypeMap.set(EntryType.HospitalVisit, {
    type: EntryType.HospitalVisit,
    commentType: CommentEntryType.HospitalVisit,
    name: "Hospital Visits",
    description:
        "View a list of your hospital visits. You can get the admission and discharge dates, location and provider for each visit.",
    icon: "house-medical",
    component: "HospitalVisitTimelineComponent",
    eventName: "hospital_visits",
    moduleName: "HospitalVisit",
});

export { entryTypeMap };
