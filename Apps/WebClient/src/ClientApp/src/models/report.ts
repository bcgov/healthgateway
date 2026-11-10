export type { RequestResult } from "./requestResult";

/** Port of models/report.ts, reportHeader.ts, reportField.ts and reportRequest.ts. */
export interface Report {
    fileName: string;
    /** base64 */
    data: string;
}

export interface ReportHeader {
    phn: string;
    name: string;
    dateOfBirth: string;
    datePrinted: string;
    filterText: string;
    isRedacted?: boolean;
}

export interface ReportField {
    key: string;
    title?: string;
    thAlign?: "start" | "end" | "center";
    tdAlign?: "start" | "end" | "center";
    width?: number | string;
    /** test id on the header cell (`thAttr["data-testid"]`) */
    thTestID?: string;
    /** test id on each body cell (`tdAttr["data-testid"]`) */
    tdTestID?: string | ((index: number) => string);
    /** `tdAttr.style: "white-space: nowrap"` */
    tdNoWrap?: boolean;
}

export interface ReportRequest {
    data: unknown;
    template: TemplateType;
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
