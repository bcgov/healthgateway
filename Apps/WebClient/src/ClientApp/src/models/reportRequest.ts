export default interface ReportRequest {
    // Gets or sets the report data.
    data: unknown;

    // Gets or sets the report type.
    template: TemplateType;

    // Gets or sets the report type.
    type: ReportType;
}

export enum TemplateType {
    Medication,
}

export enum ReportType {
    PDF,
    EXCEL,
}
