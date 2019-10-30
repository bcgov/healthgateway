import MedicationStatement from "@/models/medicationStatement";
import Pharmacy from "./pharmacy";

export enum EntryType {
  Medication,
  Laboratory,
  NONE
}

export class EntryDetail {
  public readonly name: string;
  public readonly value: string;
  public readonly newLine?: boolean;

  public constructor(name: string, value: any, newLine?: boolean) {
    this.name = name;
    this.value = value ? value.toString() : "";
    this.newLine = newLine;
  }
}

// The base timeline entry model
export default abstract class TimelineEntry {
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
    details?: EntryDetail[]
  ) {
    this.id = id;
    this.date = date;
    this.title = title;
    this.description = description;
    this.type = type ? type : EntryType.NONE;
    this.details = details ? details : [];
  }
}

// The medication timeline entry model
export class MedicationTimelineEntry extends TimelineEntry {
  public readonly pharmacyId?: string;
  public pharmacy?: Pharmacy;

  public constructor(other: MedicationStatement) {
    var details: EntryDetail[] = [
      new EntryDetail("Practitioner", other.practitionerSurname),
      new EntryDetail("Prescription Number", other.prescriptionIdentifier),
      new EntryDetail("Quantity", other.medicationSumary.quantity),
      new EntryDetail("Strength", "TODO"),
      new EntryDetail("Form", other.medicationSumary.form || "N/A"),
      new EntryDetail(
        "Manufacturer",
        other.medicationSumary.manufacturer || "N/A"
      )
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

  public PopulatePharmacy(pharmacy: Pharmacy): void {
    console.log(pharmacy);
    this.pharmacy = pharmacy;
    this.details.push(new EntryDetail("Filled At", "", true));
    this.details.push(new EntryDetail("", pharmacy.name));
    this.details.push(new EntryDetail("", pharmacy.addressLine1));
    this.details.push(new EntryDetail("", pharmacy.addressLine2));

    var formattedPhone: string = pharmacy.phoneNumber || "";
    formattedPhone = formattedPhone
      .replace(/[^0-9]/g, "")
      .replace(/(\d{3})(\d{3})(\d{4})/, "($1) $2-$3");
    this.details.push(new EntryDetail("", formattedPhone));
  }
}
