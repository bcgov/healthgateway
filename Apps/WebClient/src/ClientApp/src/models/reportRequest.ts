export default interface ReportRequest {
    // Gets or sets the report data.
    data: unknown;

    // Gets or sets the report type.
    template: TemplateType;

    // Gets or sets the report type.
    type: ReportFormatType;
}

export enum TemplateType {
    Encounter,
    DependentImmunization,
    Immunization,
    Medication,
    COVID,
    MedicationRequest,
    Notes,
    Laboratory,
    HospitalVisit,
}

export enum ReportFormatType {
    PDF,
    CSV,
    XLSX,
}
