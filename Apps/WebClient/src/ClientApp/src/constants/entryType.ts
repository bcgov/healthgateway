// Port of constants/entryType.ts (fields used by Home quick links and the add-quick-link dialog)
import { CommentEntryType } from "./commentEntryType";
import { Dataset } from "./tracking";

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

export interface EntryTypeDetails {
    type: EntryType;
    name: string;
    title: string;
    description: string;
    icon: string;
    moduleName: string;
    dataset: Dataset;
    commentType: CommentEntryType;
}

export const entryTypes: EntryTypeDetails[] = [
    {
        type: EntryType.Immunization,
        commentType: CommentEntryType.Immunization,
        name: "Immunizations",
        title: "Immunizations",
        description:
            "View immunizations you received from community pharmacies or public health, including COVID-19, and vaccine recommendations.",
        icon: "syringe",
        moduleName: "Immunization",
        dataset: Dataset.Immunizations,
    },
    {
        type: EntryType.Medication,
        commentType: CommentEntryType.Medication,
        name: "Medications",
        title: "Medications",
        description: "See your medication history dating back to 1995.",
        icon: "pills",
        moduleName: "Medication",
        dataset: Dataset.Medications,
    },
    {
        type: EntryType.LabResult,
        commentType: CommentEntryType.LabResult,
        name: "Lab Results",
        title: "Lab Results",
        description: "View and download the results of your lab tests.",
        icon: "microscope",
        moduleName: "AllLaboratory",
        dataset: Dataset.LabResults,
    },
    {
        type: EntryType.Covid19TestResult,
        commentType: CommentEntryType.Covid19TestResult,
        name: "COVID‑19 Tests",
        title: "COVID‑19 Tests",
        description:
            "View and download your COVID‑19 test results as soon as they are available.",
        icon: "vial",
        moduleName: "Laboratory",
        dataset: Dataset.Covid19Tests,
    },
    {
        type: EntryType.HealthVisit,
        commentType: CommentEntryType.HealthVisit,
        name: "Health Visits",
        title: "Health Visits",
        description:
            "See the last seven years of your health visits billed to the BC Medical Services Plan.",
        icon: "stethoscope",
        moduleName: "Encounter",
        dataset: Dataset.HealthVisits,
    },
    {
        type: EntryType.Note,
        commentType: CommentEntryType.None,
        name: "My Notes",
        title: "Note",
        description: "Create and edit your own notes on your health records.",
        icon: "pen-to-square",
        moduleName: "Note",
        dataset: Dataset.Notes,
    },
    {
        type: EntryType.SpecialAuthorityRequest,
        commentType: CommentEntryType.SpecialAuthorityRequest,
        name: "Special Authority",
        title: "Special Authority",
        description:
            "Check the status of your Special Authority Requests since March 2021.",
        icon: "file-medical",
        moduleName: "MedicationRequest",
        dataset: Dataset.SpecialAuthorityRequests,
    },
    {
        type: EntryType.ClinicalDocument,
        commentType: CommentEntryType.ClinicalDocument,
        name: "Clinical Documents",
        title: "Clinical Documents",
        description:
            "View documents shared by your care providers. You can get consultation notes, hospital discharge summaries, outpatient clinic notes and more.",
        icon: "file-waveform",
        moduleName: "ClinicalDocument",
        dataset: Dataset.ClinicalDocuments,
    },
    {
        type: EntryType.HospitalVisit,
        commentType: CommentEntryType.HospitalVisit,
        name: "Hospital Visits",
        title: "Hospital Visits",
        description:
            "View a list of your hospital visits. You can get the admission and discharge dates, location and provider for each visit.",
        icon: "house-medical",
        moduleName: "HospitalVisit",
        dataset: Dataset.HospitalVisits,
    },
    {
        type: EntryType.DiagnosticImaging,
        commentType: CommentEntryType.DiagnosticImaging,
        name: "Imaging Reports",
        title: "Imaging Reports",
        description:
            "View imaging reports for X-Rays, MRIs, Ultrasounds and more.",
        icon: "x-ray",
        moduleName: "DiagnosticImaging",
        dataset: Dataset.ImagingReports,
    },
    {
        type: EntryType.BcCancerScreening,
        commentType: CommentEntryType.BcCancerScreening,
        name: "BC Cancer Screening",
        title: "BC Cancer Screening",
        description:
            "View and download your notices and results as soon as they are available.",
        icon: "ribbon",
        moduleName: "BcCancerScreening",
        dataset: Dataset.BcCancerScreening,
    },
];

export const getEntryTypeByModule = (module: string) =>
    entryTypes.find((e) => e.moduleName === module);

export enum ServiceName {
    OrganDonorRegistration = "organDonorRegistration",
    HealthConnectRegistry = "healthConnectRegistry",
}

/** entryTypeMap.get(type) */
export const getEntryTypeDetails = (
    type: EntryType | undefined
): EntryTypeDetails | undefined => entryTypes.find((e) => e.type === type);
