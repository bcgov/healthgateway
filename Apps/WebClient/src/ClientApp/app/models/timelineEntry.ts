import MedicationStatement from "@/models/medicationStatement";
import Pharmacy from './pharmacy';

export enum EntryType {
    Medication,
    Laboratory,
    NONE
}

export class EntryDetail {
    public readonly name: string;
    public readonly value: string;

    public constructor(name: string, value: any) {
        this.name = name;
        this.value = value ? value.toString() : "N/A";
    }
}

// Timeline entry model
export default class TimelineEntry {
    public readonly id?: string;
    public readonly date?: Date;
    public readonly title?: string;
    public readonly description?: string;
    public readonly type: EntryType;
    public readonly details: EntryDetail[];

    public constructor(
        id: string,
        date?: Date,
        title?: string,
        description?: string,
        type?: EntryType,
        details?: EntryDetail[])
    {
        this.id = id;
        this.date = date;
        this.title = title;
        this.description = description;
        this.type = type ? type : EntryType.NONE;
        this.details = details ? details : [];
    }
}

export class MedicationTimelineEntry extends TimelineEntry {
    public readonly pharmacyId?: string;

    public constructor(other: MedicationStatement) {
        var details: EntryDetail[] = [
            new EntryDetail("Practitioner", other.practitionerSurname),
            new EntryDetail("Prescription Number", other.prescriptionIdentifier),
            new EntryDetail("Quantity", other.medicationSumary.quantity),
            new EntryDetail("Strength", "TODO"),
            new EntryDetail("Form", other.medicationSumary.form),
            new EntryDetail("Manufacturer", other.medicationSumary.manufacturer),
        ];

        super(
            "id-" + Math.random(),
            other.dispensedDate,
            other.medicationSumary.brandName,
            other.medicationSumary.genericName,
            EntryType.Medication,
            details
        );
        this.pharmacyId = other.pharmacyId;
    }

    public PopulatePharmacy(pharmacy: Pharmacy): void
    {
        var pharmacyDescription: string = 
            `${pharmacy.name} | ${pharmacy.addressLine1} | ${pharmacy.addressLine2} | ${pharmacy.phoneNumber}`;

        this.details.push(new EntryDetail("Filled At", pharmacyDescription));       
    }
}
