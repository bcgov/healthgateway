import MedicationStatement from "@/models/medicationStatement";

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
    this.title = other.medication.brandName;
    this.description = other.medication.genericName;

    var pharmacyDescription: string = "";
    console.log(other.pharmacy);
    if (other.pharmacy) {
      pharmacyDescription = other.pharmacy.name ? other.pharmacy.name : "";
      pharmacyDescription +=
        " | " +
        (other.pharmacy.addressLine1 ? other.pharmacy.addressLine1 : "");
      pharmacyDescription +=
        " | " +
        (other.pharmacy.addressLine2 ? other.pharmacy.addressLine2 : "");
      pharmacyDescription +=
        " | " + (other.pharmacy.phoneNumber ? other.pharmacy.phoneNumber : "");
    }

    var strenght: string = "";
    if (other.medication.complexDose) {
      strenght = other.medication.complexDose
        ? other.medication.complexDose
        : "N/A";
    } else {
      strenght = other.medication.dosage + " " + other.medication.dosageUnit;
    }

    this.details = [
      new EntryDetail("Practitioner", other.practitionerSurname),
      new EntryDetail("Prescription Number", other.prescriptionIdentifier),
      new EntryDetail("Quantity", other.medication.quantity),
      new EntryDetail("Strength", strenght),
      new EntryDetail("Form", other.medication.form),
      new EntryDetail("Manufacturer", other.medication.manufacturer),
      new EntryDetail("Filled At", pharmacyDescription)
    ];
  }
}
