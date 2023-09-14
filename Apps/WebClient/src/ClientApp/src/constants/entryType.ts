import { CommentEntryType } from "@/constants/commentEntryType";

export enum EntryType {
    ClinicalDocument = "ClinicalDocument",
    Covid19TestResult = "Covid19TestResult",
    DiagnosticImaging = "DiagnosticImaging",
    HealthVisit = "HealthVisit",
    HospitalVisit = "HospitalVisit",
    Immunization = "Immunization",
    LabResult = "LabResult",
    Medication = "Medication",
    Note = "Note",
    SpecialAuthorityRequest = "SpecialAuthorityRequest",
    BcCancerScreening = "BcCancerScreening",
}

export class EntryTypeDetails {
    type!: EntryType;
    name!: string;
    description!: string;
    reportEventName!: string;
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
    return [...entryTypeMap.values()].find((e) => e.moduleName === module);
}

entryTypeMap.set(EntryType.Immunization, {
    type: EntryType.Immunization,
    commentType: CommentEntryType.Immunization,
    name: "Immunizations",
    description:
        "View immunizations you received from public health and community pharmacies.",
    icon: "syringe",
    component: "ImmunizationTimelineComponent",
    eventName: "immunizations",
    moduleName: "Immunization",
    reportEventName: "Immunization",
});

entryTypeMap.set(EntryType.Medication, {
    type: EntryType.Medication,
    commentType: CommentEntryType.Medication,
    name: "Medications",
    description: "See your medication history dating back to 1995.",
    icon: "pills",
    component: "MedicationTimelineComponent",
    eventName: "medications",
    moduleName: "Medication",
    reportEventName: "Medication",
});

entryTypeMap.set(EntryType.LabResult, {
    type: EntryType.LabResult,
    commentType: CommentEntryType.LabResult,
    name: "Lab Results",
    description: "View and download the results of your lab tests.",
    icon: "microscope",
    component: "LabResultTimelineComponent",
    eventName: "lab_results",
    moduleName: "AllLaboratory",
    reportEventName: "Laboratory Tests",
});

entryTypeMap.set(EntryType.Covid19TestResult, {
    type: EntryType.Covid19TestResult,
    commentType: CommentEntryType.Covid19TestResult,
    name: "COVID‑19 Tests",
    description:
        "View and download your COVID‑19 test results as soon as they are available.",
    icon: "vial",
    component: "Covid19TestResultTimelineComponent",
    eventName: "covid_test",
    moduleName: "Laboratory",
    reportEventName: "COVID-19 Test",
});

entryTypeMap.set(EntryType.HealthVisit, {
    type: EntryType.HealthVisit,
    commentType: CommentEntryType.HealthVisit,
    name: "Health Visits",
    description:
        "See the last seven years of your health visits billed to the BC Medical Services Plan.",
    icon: "stethoscope",
    component: "HealthVisitTimelineComponent",
    eventName: "health_visits",
    moduleName: "Encounter",
    reportEventName: "Health Visits",
});

entryTypeMap.set(EntryType.Note, {
    type: EntryType.Note,
    commentType: CommentEntryType.None,
    name: "My Notes",
    description: "Create and edit your own notes on your health records.",
    icon: "edit",
    component: "NoteTimelineComponent",
    eventName: "my_notes",
    moduleName: "Note",
    reportEventName: "Notes",
});

entryTypeMap.set(EntryType.SpecialAuthorityRequest, {
    type: EntryType.SpecialAuthorityRequest,
    commentType: CommentEntryType.SpecialAuthorityRequest,
    name: "Special Authority",
    description:
        "Check the status of your Special Authority Requests since March 2021.",
    icon: "file-medical",
    component: "SpecialAuthorityRequestTimelineComponent",
    eventName: "special_authority",
    moduleName: "MedicationRequest",
    reportEventName: "Special Authority Requests",
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
    reportEventName: "Clinical Documents",
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
    reportEventName: "Hospital Visits",
});

entryTypeMap.set(EntryType.DiagnosticImaging, {
    type: EntryType.DiagnosticImaging,
    commentType: CommentEntryType.DiagnosticImaging,
    name: "Imaging Reports",
    description: "View imaging reports for X-Rays, MRIs, Ultrasounds and more.",
    icon: "x-ray",
    component: "DiagnosticImagingTimelineComponent",
    eventName: "diagnostic_imaging",
    moduleName: "DiagnosticImaging",
    reportEventName: "Diagnostic Imaging Exams",
});

entryTypeMap.set(EntryType.BcCancerScreening, {
    type: EntryType.BcCancerScreening,
    commentType: CommentEntryType.BcCancerScreening,
    name: "BC Cancer Screening",
    description:
        "View and download your notices and results as soon as they are available.",
    icon: "ribbon",
    component: "BcCancerScreeningTimelineComponent",
    eventName: "bc_cancer_screening",
    moduleName: "BcCancerScreening",
    reportEventName: "BC Cancer Screening",
});

export { entryTypeMap };
