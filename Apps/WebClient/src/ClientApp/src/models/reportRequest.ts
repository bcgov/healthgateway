export default interface ReportRequest {
    // Gets or sets the report data.
    data: unknown;

    // Gets or sets the report type.
    template: TemplateType;

    // Gets or sets the report type.
    type: ReportFormatType;
}

export enum TemplateType {
    Encounter = "Encounter",
    DependentImmunization = "DependentImmunization",
    DependentImmunizationRecommendation = "DependentImmunizationRecommendation",
    Immunization = "Immunization",
    ImmunizationRecommendation = "ImmunizationRecommendation",
    Medication = "Medication",
    COVID = "COVID",
    MedicationRequest = "MedicationRequest",
    Notes = "Notes",
    Laboratory = "Laboratory",
    HospitalVisit = "HospitalVisit",
}

export enum ReportFormatType {
    PDF = "PDF",
    CSV = "CSV",
    XLSX = "XLSX",
}

export const reportMimeTypeMap = new Map<ReportFormatType, string>([
    [ReportFormatType.PDF, "application/pdf"],
    [ReportFormatType.CSV, "text/csv"],
    [ReportFormatType.XLSX, "application/vnd.openxmlformats"],
]);
