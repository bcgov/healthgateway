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
<<<<<<< HEAD
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
        var strenght: string = "";
        if (other.medication.complexDose) {
            strenght = other.medication.complexDose
                ? other.medication.complexDose
                : "N/A";
        } else {
            strenght = other.medication.dosage + " " + other.medication.dosageUnit;
        }

        var details: EntryDetail[] = [
            new EntryDetail("Practitioner", other.practitionerSurname),
            new EntryDetail("Prescription Number", other.prescriptionIdentifier),
            new EntryDetail("Quantity", other.medication.quantity),
            new EntryDetail("Strength", strenght),
            new EntryDetail("Form", other.medication.form),
            new EntryDetail("Manufacturer", other.medication.manufacturer),
        ];

        super(
            "id-" + Math.random(),
            other.dispensedDate,
            other.medication.brandName,
            other.medication.genericName,
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
=======
  public readonly date?: Date;
  public readonly id?: string;
  public readonly title?: string;
  public readonly description?: string;
  public readonly type: EntryType;
  public readonly details: EntryDetail[];

  public constructor(other: MedicationStatement) {
    // The ID needs to come from the the server
    this.type = EntryType.Medication;
    this.id = "id-" + Math.random();
    this.date = other.dispensedDate;
    this.title = other.medicationSumary.brandName;
    this.description = other.medicationSumary.genericName;

    this.details = [
      new EntryDetail("Practitioner", other.practitionerSurname),
      new EntryDetail("Prescription Number", other.prescriptionIdentifier),
      new EntryDetail("Quantity", other.medicationSumary.quantity),
      new EntryDetail("Strength", "TODO"),
      new EntryDetail("Form", other.medicationSumary.form),
      new EntryDetail("Manufacturer", other.medicationSumary.manufacturer),
      new EntryDetail("Filled At", "TODO")
    ];
  }
>>>>>>> dev
}
