import { describe, expect, it } from "@jest/globals";

import { EntryType } from "@/constants/entryType";
import { ReportFormatType } from "@/models/report";
import { getDataset, getFormat } from "@/utils/eventData";

describe("eventData (EventDataUtility port)", () => {
    it.each([
        [ReportFormatType.PDF, "PDF"],
        [ReportFormatType.CSV, "CSV"],
        [ReportFormatType.XLSX, "XLSX"],
    ])("format %s → %s", (type, format) => {
        expect(getFormat(type)).toBe(format);
    });

    it.each([
        [EntryType.Immunization, "Immunizations"],
        [EntryType.Medication, "Medications"],
        [EntryType.LabResult, "Lab Results"],
        [EntryType.Covid19TestResult, "COVID-19 Tests"],
        [EntryType.Note, "Notes"],
        [EntryType.BcCancerScreening, "BC Cancer Screening"],
        [EntryType.ClinicalDocument, "Clinical Documents"],
        [EntryType.DiagnosticImaging, "Imaging Reports"],
        [EntryType.HealthVisit, "Health Visits"],
        [EntryType.HospitalVisit, "Hospital Visits"],
        [EntryType.SpecialAuthorityRequest, "Special Authority Requests"],
    ])("dataset %s → %s", (type, dataset) => {
        expect(getDataset(type)).toBe(dataset);
    });

    it("unknown values throw", () => {
        expect(() => getFormat("DOC" as ReportFormatType)).toThrow();
        expect(() => getDataset(undefined)).toThrow();
    });
});
