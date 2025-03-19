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
    logoUri?: string;
    icon!: string;
}

const entryTypeMap = new Map<EntryType | undefined, EntryTypeDetails>();

entryTypeMap.set(EntryType.Immunization, {
    type: EntryType.Immunization,
    name: "Immunizations",
    description:
        "View immunizations you received from community pharmacies or public health including COVID-19 and vaccine recommendations.",
    icon: "syringe",
});

entryTypeMap.set(EntryType.Medication, {
    type: EntryType.Medication,
    name: "Medications",
    description: "See your medication history dating back to 1995.",
    icon: "pills",
});

entryTypeMap.set(EntryType.LabResult, {
    type: EntryType.LabResult,
    name: "Lab Results",
    description: "View and download the results of your lab tests.",
    icon: "microscope",
});

entryTypeMap.set(EntryType.HealthVisit, {
    type: EntryType.HealthVisit,
    name: "Health Visits",
    description:
        "See the last seven years of your health visits billed to the BC Medical Services Plan.",
    icon: "stethoscope",
});

entryTypeMap.set(EntryType.Note, {
    type: EntryType.Note,
    name: "My Notes",
    description: "Create and edit your own notes on your health records.",
    icon: "edit",
});

entryTypeMap.set(EntryType.SpecialAuthorityRequest, {
    type: EntryType.SpecialAuthorityRequest,
    name: "Special Authority",
    description:
        "Check the status of your Special Authority Requests since March 2021.",
    icon: "file-medical",
});

entryTypeMap.set(EntryType.ClinicalDocument, {
    type: EntryType.ClinicalDocument,
    name: "Clinical Documents",
    description:
        "View documents shared by your care providers. You can get consultation notes, hospital discharge summaries, outpatient clinic notes and more.",
    icon: "file-waveform",
});

entryTypeMap.set(EntryType.HospitalVisit, {
    type: EntryType.HospitalVisit,
    name: "Hospital Visits",
    description:
        "View a list of your hospital visits. You can get the admission and discharge dates, location and provider for each visit.",
    icon: "house-medical",
});

entryTypeMap.set(EntryType.DiagnosticImaging, {
    type: EntryType.DiagnosticImaging,
    name: "Imaging Reports",
    description: "View imaging reports for X-Rays, MRIs, Ultrasounds and more.",
    icon: "x-ray",
});

entryTypeMap.set(EntryType.BcCancerScreening, {
    type: EntryType.BcCancerScreening,
    name: "BC Cancer Screening",
    description:
        "View and download your notices and results as soon as they are available.",
    icon: "ribbon",
});

export { entryTypeMap };
