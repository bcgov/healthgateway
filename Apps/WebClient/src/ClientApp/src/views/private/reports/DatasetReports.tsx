import { type Ref, useEffect, useImperativeHandle, useMemo } from "react";
import { View } from "react-native";

import { AppText, ExternalLink, HgDataTable } from "@/components/common";
import { ProtectiveWordDialog } from "@/components/site/ProtectiveWordDialog";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import type { MedicationStatement } from "@/models/medication";
import type { MedicationRequest } from "@/models/medicationRequest";
import {
    type Report,
    type ReportField,
    ReportFormatType,
    type ReportHeader,
    type RequestResult,
} from "@/models/report";
import { TemplateType } from "@/models/report";
import type { ReportFilter } from "@/models/reportFilter";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import {
    selectCovid19TestResults,
    selectCovid19TestResultsAreLoading,
    useCovid19TestResultStore,
} from "@/stores/covid19TestResult.store";
import {
    selectHealthVisits,
    selectHealthVisitsAreLoading,
    useHealthVisitStore,
} from "@/stores/healthVisit.store";
import {
    selectHospitalVisits,
    selectHospitalVisitsAreLoading,
    useHospitalVisitStore,
} from "@/stores/hospitalVisit.store";
import {
    selectLabResults,
    selectLabResultsAreLoading,
    useLabResultStore,
} from "@/stores/labResult.store";
import {
    selectMedications,
    selectMedicationsAreLoading,
    useMedicationStore,
} from "@/stores/medication.store";
import { selectNotesAreLoading, useNoteStore } from "@/stores/note.store";
import {
    selectSpecialAuthorityRequests,
    selectSpecialAuthorityRequestsAreLoading,
    useSpecialAuthorityRequestStore,
} from "@/stores/specialAuthorityRequest.store";
import { descending, formatDate, fromIso, fromIsoDate } from "@/utils/dateUtil";

/** defineExpose({ generateReport }) on each report component */
export interface ReportHandle {
    generateReport(
        reportFormatType: ReportFormatType,
        headerData: ReportHeader
    ): Promise<RequestResult<Report>>;
}

export interface DatasetReportProps {
    hdid: string;
    filter: ReportFilter;
    isDependent?: boolean;
    onIsLoadingChanged?: (isLoading: boolean) => void;
    onIsEmptyChanged?: (isEmpty: boolean) => void;
    ref?: Ref<ReportHandle>;
}

/** The part every `*ReportComponent.vue` repeats: the exposed report, its emits and loading the dataset on mount. */
function useDatasetReport<Row>(
    { hdid, onIsLoadingChanged, onIsEmptyChanged, ref }: DatasetReportProps,
    {
        isLoading,
        isEmpty,
        items,
        template,
        retrieve,
        errorName,
    }: {
        isLoading: boolean;
        isEmpty: boolean;
        items: (reportFormatType?: ReportFormatType) => Row[];
        template: TemplateType;
        retrieve: (hdid: string) => Promise<unknown>;
        errorName: string;
    }
) {
    useImperativeHandle(
        ref,
        () => ({
            generateReport: (reportFormatType, headerData) =>
                services.report.generateReport({
                    data: {
                        header: headerData,
                        records: items(reportFormatType),
                    },
                    template,
                    type: reportFormatType,
                }),
        }),
        [items, template]
    );

    useEffect(() => {
        onIsLoadingChanged?.(isLoading);
    }, [isLoading, onIsLoadingChanged]);

    useEffect(() => {
        onIsEmptyChanged?.(isEmpty);
    }, [isEmpty, onIsEmptyChanged]);

    useEffect(() => {
        // signed out while mounted (the store clears the hdid before navigation): nothing to load
        if (!hdid) return;
        retrieve(hdid).catch((error: unknown) =>
            logger.error(
                `Error loading ${errorName} data: ${describeError(error)}`
            )
        );
    }, [hdid, retrieve, errorName]);
}

interface BodyProps<Row> {
    isLoading: boolean;
    isEmpty: boolean;
    isDependent?: boolean;
    items: Row[];
    fields: ReportField[];
    testID: string;
    renderItem?: Partial<
        Record<string, (item: Row, index: number) => React.ReactNode>
    >;
    /** the preview table's name for the screen reader */
    caption?: string;
}

/** "No records found." or the preview table: `v-else-if="!isDependent"`, `d-none d-md-block` */
function ReportBody<Row extends Record<string, unknown>>({
    isLoading,
    isEmpty,
    isDependent = false,
    items,
    fields,
    testID,
    renderItem,
    caption = "Report preview",
}: BodyProps<Row>) {
    const { mdAndUp } = useBreakpoint();
    if (isEmpty && !isLoading) return <AppText>No records found.</AppText>;
    if (isDependent || !mdAndUp) return null;
    return (
        <HgDataTable
            caption={caption}
            testID={testID}
            items={items}
            fields={fields}
            density="compact"
            loading={isLoading}
            renderItem={renderItem}
        />
    );
}

// ---- COVID-19 tests ----

const covid19Fields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "covid19DateItem" },
    { key: "test_type", title: "Type", tdTestID: "covid19TestTypeItem" },
    {
        key: "test_location",
        title: "Location",
        tdTestID: "covid19LocationItem",
    },
    { key: "result", title: "Result", tdTestID: "covid19ResultItem" },
];

/** Port of Covid19TestResultReportComponent.vue */
export function Covid19TestResultReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useCovid19TestResultStore(selectCovid19TestResults(hdid));
    const isLoading = useCovid19TestResultStore(
        selectCovid19TestResultsAreLoading(hdid)
    );
    const retrieve = useCovid19TestResultStore(
        (s) => s.retrieveCovid19TestResults
    );
    const visible = useMemo(
        () =>
            records
                .filter((r) =>
                    filter.allowsDate(
                        fromIso(r.labResults[0].collectedDateTime)
                    )
                )
                .sort((a, b) =>
                    descending(
                        fromIso(a.labResults[0].collectedDateTime),
                        fromIso(b.labResults[0].collectedDateTime)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.map((x) => {
                const labResult = x.labResults[0];
                return {
                    date: formatDate(fromIso(labResult.collectedDateTime)),
                    test_type: labResult.testType,
                    test_location: x.location ?? "",
                    result: labResult.filteredLabResultOutcome,
                };
            }),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.COVID,
        retrieve,
        errorName: "Covid19",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={covid19Fields}
            testID="covid19-report-table"
        />
    );
}

// ---- Health visits ----

const healthVisitFields: ReportField[] = [
    { key: "date", title: "Visit Date", tdTestID: "mspVisitDateItem" },
    { key: "specialty_description", title: "Specialty" },
    { key: "practitioner", title: "Practitioner" },
    { key: "clinic_practitioner", title: "Clinic/Practitioner" },
];

/** Port of HealthVisitReportComponent.vue */
export function HealthVisitReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useHealthVisitStore(selectHealthVisits(hdid));
    const isLoading = useHealthVisitStore(selectHealthVisitsAreLoading(hdid));
    const retrieve = useHealthVisitStore((s) => s.retrieve);
    const visible = useMemo(
        () =>
            records
                .filter((r) => filter.allowsDate(fromIsoDate(r.encounterDate)))
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.encounterDate),
                        fromIsoDate(b.encounterDate)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.map((x) => ({
                date: formatDate(fromIsoDate(x.encounterDate)),
                specialty_description: x.specialtyDescription,
                practitioner: x.practitionerName,
                clinic_practitioner: x.clinic.name,
            })),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.Encounter,
        retrieve,
        errorName: "encounter",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={healthVisitFields}
            testID="msp-visits-report-table"
        />
    );
}

// ---- Hospital visits ----

const hospitalVisitFields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "hospital-visit-date" },
    { key: "health_service", title: "Health Service" },
    { key: "visit_type", title: "Visit Type" },
    { key: "location", title: "Location" },
    { key: "provider", title: "Provider" },
];

/** Port of HospitalVisitReportComponent.vue */
export function HospitalVisitReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useHospitalVisitStore(selectHospitalVisits(hdid));
    const isLoading = useHospitalVisitStore(
        selectHospitalVisitsAreLoading(hdid)
    );
    const retrieve = useHospitalVisitStore((s) => s.retrieveHospitalVisits);
    const visible = useMemo(
        () =>
            records
                .filter((r) => filter.allowsDate(fromIso(r.admitDateTime)))
                .sort((a, b) =>
                    descending(
                        fromIso(a.admitDateTime),
                        fromIso(b.admitDateTime)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.map((x) => ({
                date: formatDate(fromIso(x.admitDateTime)),
                health_service: x.healthService,
                visit_type: x.visitType,
                location: x.facility,
                provider: x.provider,
            })),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.HospitalVisit,
        retrieve,
        errorName: "hospital visit",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={hospitalVisitFields}
            testID="hospital-visit-report-table"
        />
    );
}

// ---- Lab results ----

const labResultFields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "labResultDateItem" },
    { key: "test", title: "Test", tdTestID: "labResultTestTypeItem" },
    { key: "result", title: "Result", tdTestID: "labResultItem" },
    { key: "status", title: "Status", tdTestID: "labStatusItem" },
];

/** Port of LabResultReportComponent.vue */
export function LabResultReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useLabResultStore(selectLabResults(hdid));
    const isLoading = useLabResultStore(selectLabResultsAreLoading(hdid));
    const retrieve = useLabResultStore((s) => s.retrieveLabResults);
    const visible = useMemo(
        () =>
            records
                .filter((r) => filter.allowsDate(fromIso(r.timelineDateTime)))
                .sort((a, b) =>
                    descending(
                        fromIso(a.timelineDateTime),
                        fromIso(b.timelineDateTime)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.flatMap((x) => {
                const date = formatDate(fromIso(x.timelineDateTime));
                return x.laboratoryTests.map((y) => ({
                    date,
                    test: y.batteryType ?? "",
                    result: y.result,
                    status: y.testStatus,
                }));
            }),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.Laboratory,
        retrieve,
        errorName: "Laboratory",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={labResultFields}
            testID="laboratory-report-table"
        />
    );
}

// ---- Medications ----

const notFoundText = "Not Found";
const checkDinText = "Check DIN";
const checkDinUrl = "https://health-products.canada.ca/dpd-bdpp/";

const medicationFields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "medicationDateItem" },
    { key: "din_pin", title: "DIN/PIN" },
    { key: "title", title: "Brand", tdTestID: "medicationReportBrandNameItem" },
    { key: "subtitle", title: "Generic" },
    { key: "practitioner", title: "Practitioner" },
    { key: "quantity", title: "Quantity" },
    { key: "strength", title: "Strength" },
    { key: "form", title: "Form" },
    { key: "manufacturer", title: "Manufacturer" },
];

/** formatActiveIngredients: over 120 characters becomes a "Check DIN" link (or the URL in CSV / XLSX) */
export function formatActiveIngredients(
    record: MedicationStatement,
    reportFormatType?: ReportFormatType
): { strength: string; check_din: string } {
    const activeIngredients = record.medicationSummary.activeIngredients ?? [];
    const formatted = activeIngredients
        .map((ingredient) => {
            const ingredientText = ingredient.ingredient?.trim() ?? "";
            const strengthText = ingredient.strength?.trim() ?? "";
            const strengthUnitText = ingredient.strengthUnit?.trim() ?? "";
            const combinedStrength =
                `${strengthText}${strengthUnitText}`.trim();
            if (!ingredientText && !combinedStrength) return "";
            if (activeIngredients.length === 1) return combinedStrength;
            return `${ingredientText} ${combinedStrength}`.trim();
        })
        .filter((value) => value.length > 0);

    if (formatted.length === 0)
        return { strength: notFoundText, check_din: "" };

    const result = formatted.join(", ");
    if (result.length > 120) {
        if (
            reportFormatType === ReportFormatType.XLSX ||
            reportFormatType === ReportFormatType.CSV
        )
            return {
                strength: `${checkDinText} (${checkDinUrl})`,
                check_din: "",
            };
        return { strength: "", check_din: checkDinText };
    }
    return { strength: result, check_din: "" };
}

function medicationRow(
    x: MedicationStatement,
    reportFormatType?: ReportFormatType
) {
    const formatted = formatActiveIngredients(x, reportFormatType);
    return {
        date: formatDate(fromIsoDate(x.dispensedDate)),
        din_pin: x.medicationSummary.din,
        title: x.medicationSummary.title,
        subtitle: x.medicationSummary.subtitle || notFoundText,
        practitioner: x.practitionerSurname ?? "",
        quantity:
            x.medicationSummary.quantity === undefined
                ? ""
                : x.medicationSummary.quantity.toString(),
        strength: formatted.strength,
        check_din: formatted.check_din,
        form: x.medicationSummary.form || notFoundText,
        manufacturer: x.medicationSummary.manufacturer || notFoundText,
    };
}

/** Port of MedicationReportComponent.vue */
export function MedicationReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useMedicationStore(selectMedications(hdid));
    const isLoading = useMedicationStore(selectMedicationsAreLoading(hdid));
    const retrieve = useMedicationStore((s) => s.retrieveMedications);
    const visible = useMemo(
        () =>
            records
                .filter(
                    (r) =>
                        filter.allowsDate(fromIsoDate(r.dispensedDate)) &&
                        filter.allowsMedication(r.medicationSummary.brandName)
                )
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.dispensedDate),
                        fromIsoDate(b.dispensedDate)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () => visible.map((x) => medicationRow(x)),
        [visible]
    );
    const reportItems = useMemo(
        () => (reportFormatType?: ReportFormatType) =>
            visible.map((x) => medicationRow(x, reportFormatType)),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: reportItems,
        template: TemplateType.Medication,
        retrieve,
        errorName: "medication",
    });
    return (
        <View>
            <ReportBody
                isLoading={isLoading}
                isEmpty={isEmpty}
                isDependent={props.isDependent}
                items={items}
                fields={medicationFields}
                testID="medication-history-report-table"
                renderItem={{
                    strength: (item) =>
                        item.check_din === checkDinText ? (
                            <ExternalLink href={checkDinUrl}>
                                {checkDinText}
                            </ExternalLink>
                        ) : (
                            <AppText className="text-body-small">
                                {item.strength}
                            </AppText>
                        ),
                }}
            />
            <ProtectiveWordDialog hdid={hdid} />
        </View>
    );
}

// ---- My notes ----

const noteFields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "user-note-date" },
    { key: "title", title: "Title", tdTestID: "user-note-title" },
    { key: "note", title: "Note" },
];

/** Port of NoteReportComponent.vue */
export function NoteReport(props: DatasetReportProps) {
    const { filter } = props;
    const records = useNoteStore((s) => s.notes);
    const isLoading = useNoteStore(selectNotesAreLoading);
    const retrieve = useNoteStore((s) => s.retrieveNotes);
    const visible = useMemo(
        () =>
            records
                .filter((r) => filter.allowsDate(fromIsoDate(r.journalDate)))
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.journalDate),
                        fromIsoDate(b.journalDate)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.map((x) => ({
                date: formatDate(fromIsoDate(x.journalDate)),
                title: x.title,
                note: x.text,
            })),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.Notes,
        retrieve,
        errorName: "user note",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={noteFields}
            testID="notes-report-table"
        />
    );
}

// ---- Special Authority ----

const specialAuthorityFields: ReportField[] = [
    { key: "date", title: "Date", tdTestID: "medicationRequestDateItem" },
    { key: "requested_drug_name", title: "Requested Drug Name" },
    { key: "status", title: "Status" },
    { key: "prescriber_name", title: "Prescriber Name" },
    { key: "effective_date", title: "Effective Date" },
    { key: "expiry_date", title: "Expiry Date" },
    { key: "reference_number", title: "Reference Number" },
];

const prescriberName = (medication: MedicationRequest) =>
    `${medication.prescriberFirstName || " "} ${medication.prescriberLastName || " "}`;

/** Port of SpecialAuthorityRequestReportComponent.vue */
export function SpecialAuthorityRequestReport(props: DatasetReportProps) {
    const { hdid, filter } = props;
    const records = useSpecialAuthorityRequestStore(
        selectSpecialAuthorityRequests(hdid)
    );
    const isLoading = useSpecialAuthorityRequestStore(
        selectSpecialAuthorityRequestsAreLoading(hdid)
    );
    const retrieve = useSpecialAuthorityRequestStore(
        (s) => s.retrieveSpecialAuthorityRequests
    );
    const visible = useMemo(
        () =>
            records
                .filter((r) => filter.allowsDate(fromIsoDate(r.requestedDate)))
                .sort((a, b) =>
                    descending(
                        fromIsoDate(a.requestedDate),
                        fromIsoDate(b.requestedDate)
                    )
                ),
        [records, filter]
    );
    const items = useMemo(
        () =>
            visible.map((x) => ({
                date: formatDate(fromIsoDate(x.requestedDate)),
                requested_drug_name: x.drugName ?? "",
                status: x.requestStatus ?? "",
                prescriber_name: prescriberName(x),
                effective_date: formatDate(fromIsoDate(x.effectiveDate)),
                expiry_date: formatDate(fromIsoDate(x.expiryDate)),
                reference_number: x.referenceNumber,
            })),
        [visible]
    );
    const isEmpty = visible.length === 0;
    useDatasetReport(props, {
        isLoading,
        isEmpty,
        items: () => items,
        template: TemplateType.MedicationRequest,
        retrieve,
        errorName: "Special Authority requests",
    });
    return (
        <ReportBody
            isLoading={isLoading}
            isEmpty={isEmpty}
            isDependent={props.isDependent}
            items={items}
            fields={specialAuthorityFields}
            testID="medication-request-report-table"
        />
    );
}
