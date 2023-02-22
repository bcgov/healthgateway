import { CommentEntryType } from "@/constants/commentEntryType";
import { FeatureToggleConfiguration } from "@/models/configData";

export enum EntryType {
    MedicationRequest = "MedicationRequest",
    Medication = "Medication",
    Immunization = "Immunization",
    Covid19LaboratoryOrder = "Laboratory",
    LaboratoryOrder = "AllLaboratory",
    Encounter = "Encounter",
    Note = "Note",
    ClinicalDocument = "ClinicalDocument",
    HospitalVisit = "HospitalVisit",
}

export class EntryTypeDetails {
    type!: EntryType;
    name!: string;
    description!: string;
    icon!: string;
    component!: string;
    commentType!: CommentEntryType;
    eventName!: string;
    enabled!: (config: FeatureToggleConfiguration) => boolean;
}

const entryTypeMap = new Map<EntryType | undefined, EntryTypeDetails>();

function isDatasetEnabled(
    config: FeatureToggleConfiguration,
    datasetName: string
): boolean {
    return (
        config.datasets.find(
            (ds) => ds.name.toLowerCase() == datasetName.toLowerCase()
        )?.enabled ?? false
    );
}

entryTypeMap.set(EntryType.Immunization, {
    type: EntryType.Immunization,
    commentType: CommentEntryType.Immunization,
    name: "Immunizations",
    description:
        "View immunizations you received from public health and community pharmacies",
    icon: "syringe",
    component: "ImmunizationComponent",
    eventName: "immunizations",
    enabled: (config) => isDatasetEnabled(config, EntryType.Immunization),
});

entryTypeMap.set(EntryType.Medication, {
    type: EntryType.Medication,
    commentType: CommentEntryType.Medication,
    name: "Medications",
    description: "See your medication history dating back to 1995",
    icon: "pills",
    component: "MedicationComponent",
    eventName: "medications",
    enabled: (config) => isDatasetEnabled(config, EntryType.Medication),
});

entryTypeMap.set(EntryType.LaboratoryOrder, {
    type: EntryType.LaboratoryOrder,
    commentType: CommentEntryType.LaboratoryOrder,
    name: "Lab Results",
    description:
        "Find out your lab results within about 48 hours of taking a test",
    icon: "microscope",
    component: "LaboratoryOrderComponent",
    eventName: "lab_results",
    enabled: (config) => isDatasetEnabled(config, "LabResult"),
});

entryTypeMap.set(EntryType.Covid19LaboratoryOrder, {
    type: EntryType.Covid19LaboratoryOrder,
    commentType: CommentEntryType.Covid19LaboratoryOrder,
    name: "COVID‑19 Tests",
    description:
        "View and download your COVID‑19 test results as soon as they are available",
    icon: "vial",
    component: "Covid19LaboratoryOrderComponent",
    eventName: "covid_test",
    enabled: (config) => isDatasetEnabled(config, "Covid19TestResult"),
});

entryTypeMap.set(EntryType.Encounter, {
    type: EntryType.Encounter,
    commentType: CommentEntryType.Encounter,
    name: "Health Visits",
    description:
        "See the last seven years of your health visits billed to the BC Medical Services Plan",
    icon: "stethoscope",
    component: "EncounterComponent",
    eventName: "health_visits",
    enabled: (config) => isDatasetEnabled(config, "HealthVisit"),
});

entryTypeMap.set(EntryType.Note, {
    type: EntryType.Note,
    commentType: CommentEntryType.None,
    name: "My Notes",
    description: "Create and edit your own notes on your health records",
    icon: "edit",
    component: "NoteComponent",
    eventName: "my_notes",
    enabled: (config) => isDatasetEnabled(config, EntryType.Note),
});

entryTypeMap.set(EntryType.MedicationRequest, {
    type: EntryType.MedicationRequest,
    commentType: CommentEntryType.MedicationRequest,
    name: "Special Authority",
    description:
        "Check the status of your Special Authority Requests since March 2021",
    icon: "file-medical",
    component: "MedicationRequestComponent",
    eventName: "special_authority",
    enabled: (config) => isDatasetEnabled(config, "SpecialAuthority"),
});

entryTypeMap.set(EntryType.ClinicalDocument, {
    type: EntryType.ClinicalDocument,
    commentType: CommentEntryType.ClinicalDocument,
    name: "Clinical Documents",
    description:
        "View documents shared by your care providers. You can get consultation notes, hospital discharge summaries, outpatient clinic notes and more.",
    icon: "file-waveform",
    component: "ClinicalDocumentComponent",
    eventName: "document",
    enabled: (config) => isDatasetEnabled(config, EntryType.ClinicalDocument),
});

entryTypeMap.set(EntryType.HospitalVisit, {
    type: EntryType.HospitalVisit,
    commentType: CommentEntryType.HospitalVisit,
    name: "Hospital Visits",
    description:
        "View a list of your hospital visits. You can get the admission and discharge dates, location and provider for each visit.",
    icon: "house-medical",
    component: "HospitalVisitComponent",
    eventName: "hospital_visits",
    enabled: (config) => isDatasetEnabled(config, EntryType.HospitalVisit),
});

export { entryTypeMap };
