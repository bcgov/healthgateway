import { CommentEntryType } from "@/constants/commentEntryType";

export enum EntryType {
    MedicationRequest = "MedicationRequest",
    Medication = "Medication",
    Immunization = "Immunization",
    Covid19LaboratoryOrder = "Laboratory",
    LaboratoryOrder = "AllLaboratory",
    Encounter = "Encounter",
    Note = "Note",
}

export class EntryTypeDetails {
    type!: EntryType;
    name!: string;
    description!: string;
    icon!: string;
    component!: string;
    commentType!: CommentEntryType;
    eventName!: string;
}

const entryTypeMap = new Map<EntryType | undefined, EntryTypeDetails>();

entryTypeMap.set(EntryType.Immunization, {
    type: EntryType.Immunization,
    commentType: CommentEntryType.Immunization,
    name: "Immunizations",
    description:
        "View your immunizations from public health. Vaccines received in a pharmacy may be recorded as a prescription medication.",
    icon: "syringe",
    component: "ImmunizationComponent",
    eventName: "immunizations",
});

entryTypeMap.set(EntryType.Medication, {
    type: EntryType.Medication,
    commentType: CommentEntryType.Medication,
    name: "Medications",
    description:
        "View your prescription medication records. Vaccines received in a pharmacy may be recorded as a prescription medication.",
    icon: "pills",
    component: "MedicationComponent",
    eventName: "medications",
});

entryTypeMap.set(EntryType.LaboratoryOrder, {
    type: EntryType.LaboratoryOrder,
    commentType: CommentEntryType.LaboratoryOrder,
    name: "Lab Results",
    description:
        "View your lab results. Most results are available within 24–48 hours.",
    icon: "microscope",
    component: "LaboratoryOrderComponent",
    eventName: "lab_results",
});

entryTypeMap.set(EntryType.Covid19LaboratoryOrder, {
    type: EntryType.Covid19LaboratoryOrder,
    commentType: CommentEntryType.Covid19LaboratoryOrder,
    name: "COVID‑19 Tests",
    description:
        "View and download your COVID‑19 test results as soon as they are available.",
    icon: "vial",
    component: "Covid19LaboratoryOrderComponent",
    eventName: "covid_test",
});

entryTypeMap.set(EntryType.Encounter, {
    type: EntryType.Encounter,
    commentType: CommentEntryType.Encounter,
    name: "Health Visits",
    description:
        "View the last seven years of your health visits, consultations and procedures billed to the BC Medical Services Plan.",
    icon: "user-md",
    component: "EncounterComponent",
    eventName: "health_visits",
});

entryTypeMap.set(EntryType.Note, {
    type: EntryType.Note,
    commentType: CommentEntryType.None,
    name: "My Notes",
    description: "View and edit notes that you added to your medical records.",
    icon: "edit",
    component: "NoteComponent",
    eventName: "my_notes",
});

entryTypeMap.set(EntryType.MedicationRequest, {
    type: EntryType.MedicationRequest,
    commentType: CommentEntryType.MedicationRequest,
    name: "Special Authority",
    description:
        "View the status of your Special Authority drug coverage requests made since March 2021.",
    icon: "clipboard-list",
    component: "MedicationRequestComponent",
    eventName: "special_authority",
});

export { entryTypeMap };
