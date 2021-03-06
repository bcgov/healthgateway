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
    Immunization,
    Medication,
    COVID,
    MedicationRequest,
}

export enum ReportFormatType {
    PDF,
    CSV,
    XLSX,
}
