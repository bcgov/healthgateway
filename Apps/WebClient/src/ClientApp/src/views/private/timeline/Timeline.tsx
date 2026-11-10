import { useIsFocused } from "expo-router";
import { useEffect, useMemo, useRef, useState } from "react";
import { Image, Platform, View } from "react-native";

import {
    AppText,
    ExternalLink,
    HgAlert,
    HgButton,
    HgChip,
    HgPagination,
    LiveRegion,
    VisuallyHidden,
} from "@/components/common";
import {
    usePageRefresh,
    usePageRetrieving,
    useScrollToTop,
} from "@/components/site/PageScrollContext";
import { ProtectiveWordDialog } from "@/components/site/ProtectiveWordDialog";
import { getDataSource } from "@/constants/dataSource";
import { EntryType, getEntryTypeDetails } from "@/constants/entryType";
import { ErrorSourceType } from "@/constants/errorType";
import {
    Action,
    Destination,
    Origin,
    Text,
    TrackedExternalUrl,
    Type,
} from "@/constants/tracking";
import { useBreakpoint } from "@/hooks/useBreakpoint";
import {
    type BcCancerScreening,
    type DiagnosticImagingExam,
    HealthDataType,
    PatientDataType,
} from "@/models/patientData";
import { BcCancerScreeningTimelineEntry } from "@/models/timeline/bcCancerScreeningTimelineEntry";
import { ClinicalDocumentTimelineEntry } from "@/models/timeline/clinicalDocumentTimelineEntry";
import { Covid19TestResultTimelineEntry } from "@/models/timeline/covid19TestResultTimelineEntry";
import { DiagnosticImagingTimelineEntry } from "@/models/timeline/diagnosticImagingTimelineEntry";
import { HealthVisitTimelineEntry } from "@/models/timeline/healthVisitTimelineEntry";
import { HospitalVisitTimelineEntry } from "@/models/timeline/hospitalVisitTimelineEntry";
import { ImmunizationTimelineEntry } from "@/models/timeline/immunizationTimelineEntry";
import { LabResultTimelineEntry } from "@/models/timeline/labResultTimelineEntry";
import { MedicationTimelineEntry } from "@/models/timeline/medicationTimelineEntry";
import { NoteTimelineEntry } from "@/models/timeline/noteTimelineEntry";
import { SpecialAuthorityRequestTimelineEntry } from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import { DateGroup, type TimelineEntry } from "@/models/timeline/timelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { services } from "@/services/container";
import { describeError, logger } from "@/services/logger";
import { entryTypeToPatientDataTypeMap } from "@/services/patientData.service";
import { useAppLockStore } from "@/stores/appLock.store";
import {
    selectClinicalDocuments,
    selectClinicalDocumentsAreLoading,
    useClinicalDocumentStore,
} from "@/stores/clinicalDocument.store";
import {
    entryCommentsGetter,
    selectCommentsAreLoading,
    useCommentStore,
} from "@/stores/comment.store";
import {
    selectCanFetch,
    useConnectivityStore,
} from "@/stores/connectivity.store";
import {
    selectCovid19TestResults,
    selectCovid19TestResultsAreLoading,
    useCovid19TestResultStore,
} from "@/stores/covid19TestResult.store";
import { useErrorStore } from "@/stores/error.store";
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
    selectImmunizations,
    selectImmunizationsAreLoading,
    useImmunizationStore,
} from "@/stores/immunization.store";
import {
    selectLabResults,
    selectLabResultsAreLoading,
    selectLabResultsAreQueued,
    useLabResultStore,
} from "@/stores/labResult.store";
import {
    selectMedications,
    selectMedicationsAreLoading,
    useMedicationStore,
} from "@/stores/medication.store";
import { selectNotesAreLoading, useNoteStore } from "@/stores/note.store";
import {
    patientDataRecords,
    selectPatientDataAreLoading,
    selectPatientDataState,
    usePatientDataStore,
} from "@/stores/patientData.store";
import {
    clearTimelineDatasets,
    markTimelineDatasetsStale,
} from "@/stores/refreshDatasets";
import {
    selectSpecialAuthorityRequests,
    selectSpecialAuthorityRequestsAreLoading,
    useSpecialAuthorityRequestStore,
} from "@/stores/specialAuthorityRequest.store";
import {
    selectHasActiveFilter,
    selectSelectedEntryTypes,
    useTimelineStore,
} from "@/stores/timeline.store";
import { useUserStore } from "@/stores/user.store";
import { descending, formatDate, fromIsoDate } from "@/utils/dateUtil";

import { TimelineEntryView } from "./entries";
import { HealthVisitBillingInfo } from "./entries/HealthVisitEntry";
import { FilterDialog } from "./FilterDialog";
import { FullscreenTimelineEntry } from "./FullscreenTimelineEntry";

const EmptyState =
    require("@/assets/images/timeline/empty-state.png") as number;

const pageSize = 25;

interface Props {
    hdid: string;
    entryTypes?: EntryType[];
    commentsAreEnabled?: boolean;
    /** Native: pull the page down to fetch the records again (stated deviation: the web client has no such gesture). */
    refreshable?: boolean;
}

/**
 * Most responses come quickly, but lab results can hold their first answer for minutes. The spinner stops here; the
 * loading toast and each dataset's own state carry on.
 */
const REFRESH_SPINNER_MAX_MS = 10000;

/** v-skeleton-loader "divider, avatar, heading" × 3 */
function TimelineSkeleton() {
    return (
        // a status region that says "Loading", so the screen reader hears it instead of empty boxes
        <LiveRegion
            politeness="polite"
            testID="content-placeholders"
            className="py-2 mb-4"
        >
            <View aria-busy>
                {[0, 1, 2].map((i) => (
                    <View key={i}>
                        <View className="h-px bg-border-light" />
                        <View className="flex-row items-center py-4 px-4">
                            <View className="w-12 h-12 rounded-full bg-border-light mr-4" />
                            <View
                                className="h-6 rounded bg-border-light"
                                style={{ width: "50%" }}
                            />
                        </View>
                    </View>
                ))}
                <View className="h-px bg-border-light" />
            </View>
            <VisuallyHidden>Loading</VisuallyHidden>
        </LiveRegion>
    );
}

/**
 * v-snackbar "Retrieving your health records", fixed at the bottom (native: the page's short "Retrieving…" pill, see
 * AGENTS.md). It closes itself after Vuetify's default 5 s timeout, even while a slow dataset is still loading.
 */
function LoadingToast() {
    const [expired, setExpired] = useState(false);
    useEffect(() => {
        const timer = setTimeout(() => setExpired(true), 5000);
        return () => clearTimeout(timer);
    }, []);
    usePageRetrieving(Platform.OS !== "web" && !expired);
    if (expired || Platform.OS !== "web") return null;
    return (
        <View
            className="items-center absolute left-0 right-0 bottom-2"
            style={{ position: "fixed", zIndex: 10 } as object}
            pointerEvents="none"
        >
            {/* a status, not an alert: said when the reader is free, without interrupting */}
            <LiveRegion
                politeness="polite"
                testID="loading-toast"
                className="bg-[#333333] rounded px-4 py-3.5 min-w-[344px] max-w-[672px]"
            >
                <AppText className="text-white text-body-medium">
                    Retrieving your health records
                </AppText>
            </LiveRegion>
        </View>
    );
}

/** Port of TimelineComponent.vue (the filter banner arrives with sub-chunk 4.2.13). */
export function Timeline({
    hdid,
    entryTypes = [],
    commentsAreEnabled = false,
    refreshable = false,
}: Props) {
    const { isMobile } = useBreakpoint();
    const scrollToTop = useScrollToTop();
    const immunizations = useImmunizationStore(selectImmunizations(hdid));
    const immunizationsAreLoading = useImmunizationStore(
        selectImmunizationsAreLoading(hdid)
    );
    const retrieveImmunizations = useImmunizationStore(
        (s) => s.retrieveImmunizations
    );
    const medications = useMedicationStore(selectMedications(hdid));
    const medicationsAreLoading = useMedicationStore(
        selectMedicationsAreLoading(hdid)
    );
    const retrieveMedications = useMedicationStore(
        (s) => s.retrieveMedications
    );
    const labResults = useLabResultStore(selectLabResults(hdid));
    const labResultsAreLoading = useLabResultStore(
        selectLabResultsAreLoading(hdid)
    );
    const labResultsAreQueued = useLabResultStore(
        selectLabResultsAreQueued(hdid)
    );
    const retrieveLabResults = useLabResultStore((s) => s.retrieveLabResults);
    const covid19TestResults = useCovid19TestResultStore(
        selectCovid19TestResults(hdid)
    );
    const covid19TestResultsAreLoading = useCovid19TestResultStore(
        selectCovid19TestResultsAreLoading(hdid)
    );
    const retrieveCovid19TestResults = useCovid19TestResultStore(
        (s) => s.retrieveCovid19TestResults
    );
    const healthVisits = useHealthVisitStore(selectHealthVisits(hdid));
    const healthVisitsAreLoading = useHealthVisitStore(
        selectHealthVisitsAreLoading(hdid)
    );
    const retrieveHealthVisits = useHealthVisitStore((s) => s.retrieve);
    const notes = useNoteStore((s) => s.notes);
    const notesAreLoading = useNoteStore(selectNotesAreLoading);
    const retrieveNotes = useNoteStore((s) => s.retrieveNotes);
    const specialAuthorityRequests = useSpecialAuthorityRequestStore(
        selectSpecialAuthorityRequests(hdid)
    );
    const specialAuthorityRequestsAreLoading = useSpecialAuthorityRequestStore(
        selectSpecialAuthorityRequestsAreLoading(hdid)
    );
    const retrieveSpecialAuthorityRequests = useSpecialAuthorityRequestStore(
        (s) => s.retrieveSpecialAuthorityRequests
    );
    const clinicalDocuments = useClinicalDocumentStore(
        selectClinicalDocuments(hdid)
    );
    const clinicalDocumentsAreLoading = useClinicalDocumentStore(
        selectClinicalDocumentsAreLoading(hdid)
    );
    const retrieveClinicalDocuments = useClinicalDocumentStore(
        (s) => s.retrieve
    );
    const hospitalVisits = useHospitalVisitStore(selectHospitalVisits(hdid));
    const hospitalVisitsAreLoading = useHospitalVisitStore(
        selectHospitalVisitsAreLoading(hdid)
    );
    const retrieveHospitalVisits = useHospitalVisitStore(
        (s) => s.retrieveHospitalVisits
    );
    const comments = useCommentStore((s) => s.comments);
    const commentsAreLoading = useCommentStore(selectCommentsAreLoading);
    const retrieveComments = useCommentStore((s) => s.retrieveComments);
    const patientDataState = usePatientDataStore(selectPatientDataState(hdid));
    const patientDataAreLoading = usePatientDataStore(
        selectPatientDataAreLoading(hdid)
    );
    const retrievePatientData = usePatientDataStore(
        (s) => s.retrievePatientData
    );
    const selectedDate = useTimelineStore((s) => s.selectedDate);
    const filter = useTimelineStore((s) => s.filter);
    const hasActiveFilter = useTimelineStore(selectHasActiveFilter);
    const filterKey = [
        filter.keyword,
        filter.startDate,
        filter.endDate,
        ...filter.entryTypes,
    ].join("|");
    const selectedEntryTypes = useTimelineStore(selectSelectedEntryTypes);
    const blockedDataSources = useUserStore((s) => s.user.blockedDataSources);

    const unfilteredTimelineEntries = useMemo(() => {
        const entries: TimelineEntry[] = [];
        const getComments = entryCommentsGetter(comments);
        for (const request of specialAuthorityRequests)
            entries.push(
                new SpecialAuthorityRequestTimelineEntry(request, getComments)
            );
        for (const medication of medications)
            entries.push(new MedicationTimelineEntry(medication, getComments));
        for (const result of covid19TestResults)
            entries.push(
                new Covid19TestResultTimelineEntry(result, getComments)
            );
        for (const order of labResults)
            entries.push(new LabResultTimelineEntry(order, getComments));
        for (const healthVisit of healthVisits)
            entries.push(
                new HealthVisitTimelineEntry(healthVisit, getComments)
            );
        for (const visit of hospitalVisits)
            entries.push(new HospitalVisitTimelineEntry(visit, getComments));
        for (const doc of clinicalDocuments)
            entries.push(new ClinicalDocumentTimelineEntry(doc, getComments));
        for (const note of notes) entries.push(new NoteTimelineEntry(note));
        for (const immunization of immunizations)
            entries.push(new ImmunizationTimelineEntry(immunization));
        for (const exam of patientDataRecords(patientDataState, [
            PatientDataType.DiagnosticImaging,
            PatientDataType.BcCancerScreening,
        ])) {
            if (exam.type === HealthDataType.DiagnosticImagingExam)
                entries.push(
                    new DiagnosticImagingTimelineEntry(
                        exam as DiagnosticImagingExam,
                        getComments
                    )
                );
            else if (exam.type === HealthDataType.BcCancerScreening)
                entries.push(
                    new BcCancerScreeningTimelineEntry(
                        exam as BcCancerScreening,
                        getComments
                    )
                );
        }
        // newest first
        entries.sort((a, b) => descending(a.date, b.date));
        return entries;
    }, [
        specialAuthorityRequests,
        medications,
        covid19TestResults,
        labResults,
        healthVisits,
        hospitalVisits,
        clinicalDocuments,
        notes,
        patientDataState,
        immunizations,
        comments,
    ]);

    const filteredTimelineEntries = useMemo(
        () =>
            filter.hasActiveFilter()
                ? unfilteredTimelineEntries.filter((e) =>
                      e.filterApplies(filter)
                  )
                : unfilteredTimelineEntries,
        [unfilteredTimelineEntries, filter]
    );

    const numberOfPages =
        filteredTimelineEntries.length > pageSize
            ? Math.ceil(filteredTimelineEntries.length / pageSize)
            : 1;

    // setPageFromDate(linearDate): come back to the page that was showing
    const [currentPage, setCurrentPage] = useState(() => {
        const linearDate = useTimelineStore.getState().linearDate;
        const index = filteredTimelineEntries.findIndex(
            (entry) => entry.date.valueOf() === linearDate.valueOf()
        );
        return index >= 0 ? Math.floor(index / pageSize) + 1 : 1;
    });
    // the current page beyond the last page (fewer entries after a filter change)
    const page = Math.min(currentPage, numberOfPages);

    const visibleTimelineEntries = useMemo(
        () =>
            filteredTimelineEntries.slice(
                (page - 1) * pageSize,
                Math.min(page * pageSize, filteredTimelineEntries.length)
            ),
        [filteredTimelineEntries, page]
    );
    const dateGroups = useMemo(
        () =>
            DateGroup.sortGroups(
                DateGroup.createGroups(visibleTimelineEntries)
            ),
        [visibleTimelineEntries]
    );

    function datasetIsLoading(entryType: EntryType): boolean {
        switch (entryType) {
            case EntryType.Immunization:
                return immunizationsAreLoading;
            case EntryType.Medication:
                return medicationsAreLoading;
            case EntryType.LabResult:
                return labResultsAreLoading;
            case EntryType.Covid19TestResult:
                return covid19TestResultsAreLoading;
            case EntryType.HealthVisit:
                return healthVisitsAreLoading;
            case EntryType.Note:
                return notesAreLoading;
            case EntryType.SpecialAuthorityRequest:
                return specialAuthorityRequestsAreLoading;
            case EntryType.ClinicalDocument:
                return clinicalDocumentsAreLoading;
            case EntryType.HospitalVisit:
                return hospitalVisitsAreLoading;
            case EntryType.DiagnosticImaging:
            case EntryType.BcCancerScreening:
                return patientDataAreLoading;
            default:
                // datasets not ported yet (sub-chunks 4.2.2 onward) never load
                return false;
        }
    }

    const selectedDatasetsAreLoading = entryTypes.some(
        (entryType) =>
            selectedEntryTypes.has(entryType) && datasetIsLoading(entryType)
    );
    const isFullyLoaded =
        !entryTypes.some((entryType) => datasetIsLoading(entryType)) &&
        !(commentsAreEnabled && commentsAreLoading);
    const isOnlyClinicalDocumentSelected =
        selectedEntryTypes.size === 1 &&
        selectedEntryTypes.has(EntryType.ClinicalDocument);
    const isOnlyDiagnosticImagingSelected =
        selectedEntryTypes.size === 1 &&
        selectedEntryTypes.has(EntryType.DiagnosticImaging);
    const isOnlyBcCancerScreeningSelected =
        selectedEntryTypes.size === 1 &&
        selectedEntryTypes.has(EntryType.BcCancerScreening);
    const isOnlyHealthVisitSelected =
        selectedEntryTypes.size === 1 &&
        selectedEntryTypes.has(EntryType.HealthVisit);
    const isOnlyImmunizationSelected =
        selectedEntryTypes.size === 1 &&
        selectedEntryTypes.has(EntryType.Immunization);

    const lowerPageStart = (page - 1) * pageSize + 1;
    const upperPageEnd = Math.min(
        page * pageSize,
        filteredTimelineEntries.length
    );
    const recordCountMessage =
        filteredTimelineEntries.length === 1
            ? "Displaying 1 out of 1 records"
            : `Displaying ${lowerPageStart} to ${upperPageEnd} out of ${filteredTimelineEntries.length} records`;

    enum FilterLabelType {
        Keyword = "Keyword",
        Type = "Record Type",
        Date = "Date",
    }
    const filterLabels: [FilterLabelType, string][] = [];
    if (filter.keyword)
        filterLabels.push([FilterLabelType.Keyword, `"${filter.keyword}"`]);
    for (const entryType of selectedEntryTypes) {
        const label = getEntryTypeDetails(entryType)?.name;
        if (label) filterLabels.push([FilterLabelType.Type, label]);
    }
    const startDateLabel = filter.startDate
        ? `From ${formatDate(fromIsoDate(filter.startDate))}`
        : "";
    const endDateLabel = filter.endDate
        ? `To ${formatDate(fromIsoDate(filter.endDate))}`
        : "";
    if (startDateLabel && endDateLabel)
        filterLabels.push([
            FilterLabelType.Date,
            `${startDateLabel} ${endDateLabel}`,
        ]);
    else if (startDateLabel)
        filterLabels.push([FilterLabelType.Date, startDateLabel]);
    else if (endDateLabel)
        filterLabels.push([FilterLabelType.Date, endDateLabel]);

    function clearFilter(label: FilterLabelType, value: string) {
        let keyword = filter.keyword;
        let startDate = filter.startDate;
        let endDate = filter.endDate;
        let types = [...selectedEntryTypes];
        if (label === FilterLabelType.Keyword) keyword = "";
        else if (label === FilterLabelType.Date) {
            startDate = "";
            endDate = "";
        } else if (label === FilterLabelType.Type)
            types = types.filter((e) => getEntryTypeDetails(e)?.name !== value);
        useTimelineStore
            .getState()
            .setFilter(
                TimelineFilterBuilder.create()
                    .withKeyword(keyword)
                    .withStartDate(startDate)
                    .withEndDate(endDate)
                    .withEntryTypes(types)
            );
    }
    const clearFilters = () =>
        useTimelineStore.getState().setFilter(TimelineFilterBuilder.create());

    /** FilterComponent getFilterCount */
    function getCount(entryType: EntryType): number | undefined {
        switch (entryType) {
            case EntryType.Note:
                return notes.length;
            case EntryType.ClinicalDocument:
                return clinicalDocuments.length;
            case EntryType.Covid19TestResult:
                return covid19TestResults.length;
            case EntryType.HealthVisit:
                return healthVisits.length;
            case EntryType.HospitalVisit:
                return hospitalVisits.length;
            case EntryType.Immunization:
                return immunizations.length;
            case EntryType.LabResult:
                return labResults.length;
            case EntryType.Medication:
                return medications.length;
            case EntryType.SpecialAuthorityRequest:
                return specialAuthorityRequests.length;
            case EntryType.DiagnosticImaging:
                return patientDataRecords(patientDataState, [
                    PatientDataType.DiagnosticImaging,
                ]).length;
            case EntryType.BcCancerScreening:
                return patientDataRecords(patientDataState, [
                    PatientDataType.BcCancerScreening,
                ]).length;
            default:
                return undefined;
        }
    }

    // fetchTimelineData()
    function fetchTimelineData(): Promise<unknown> {
        const blockedEntryTypes: EntryType[] = [];
        const promises: Promise<unknown>[] = [];
        const patientDataTypesToRequest: PatientDataType[] = [];
        for (const entryType of entryTypes) {
            const canAccess = !blockedDataSources.includes(
                getDataSource(entryType)
            );
            logger.debug(
                `Can access data source for ${getDataSource(entryType)}: ${canAccess}`
            );
            if (!canAccess) blockedEntryTypes.push(entryType);
            else if (entryTypeToPatientDataTypeMap.has(entryType))
                patientDataTypesToRequest.push(
                    entryTypeToPatientDataTypeMap.get(
                        entryType
                    ) as PatientDataType
                );
            else if (entryType === EntryType.Immunization)
                promises.push(retrieveImmunizations(hdid));
            else if (entryType === EntryType.Medication)
                promises.push(retrieveMedications(hdid));
            else if (entryType === EntryType.LabResult)
                promises.push(retrieveLabResults(hdid));
            else if (entryType === EntryType.Covid19TestResult)
                promises.push(retrieveCovid19TestResults(hdid));
            else if (entryType === EntryType.HealthVisit)
                promises.push(retrieveHealthVisits(hdid));
            else if (entryType === EntryType.Note)
                promises.push(retrieveNotes(hdid));
            else if (entryType === EntryType.SpecialAuthorityRequest)
                promises.push(retrieveSpecialAuthorityRequests(hdid));
            else if (entryType === EntryType.ClinicalDocument)
                promises.push(retrieveClinicalDocuments(hdid));
            else if (entryType === EntryType.HospitalVisit)
                promises.push(retrieveHospitalVisits(hdid));
        }
        if (patientDataTypesToRequest.length > 0)
            promises.push(retrievePatientData(hdid, patientDataTypesToRequest));
        if (commentsAreEnabled) promises.push(retrieveComments(hdid));
        // showDatasetWarning
        if (blockedEntryTypes.length > 0) {
            let warningMessage: string;
            if (blockedEntryTypes.length > 1)
                warningMessage =
                    "Multiple records are unavailable at this time. Please try again later.";
            else {
                const name = getEntryTypeDetails(blockedEntryTypes[0])?.name;
                warningMessage = name
                    ? `${name} are unavailable at this time. Please try again later.`
                    : "Some records may be unavailable at this time, please try again later.";
            }
            useErrorStore
                .getState()
                .addCustomError(warningMessage, ErrorSourceType.User);
        }
        return Promise.all(promises).catch((error: unknown) =>
            logger.error(`Error loading timeline data: ${describeError(error)}`)
        );
    }

    const [pulling, setPulling] = useState(false);
    const focused = useIsFocused();
    const locked = useAppLockStore((s) => s.locked);
    const lockEnabled = useAppLockStore((s) => s.enabled);
    const syncing = useConnectivityStore((s) => s.syncing);
    const canFetch = useConnectivityStore(selectCanFetch);
    const syncRequests = useConnectivityStore((s) => s.syncRequests);

    // once per hdid
    useEffect(() => {
        if (hdid) void fetchTimelineData();
        // eslint-disable-next-line react-hooks/exhaustive-deps -- the web client fetches once, on creation
    }, [hdid]);

    /**
     * Fetches everything again. `fromEmpty` (a pull with no saved copy) clears the list first, so it and its count build
     * up again as each source answers; a source still loading carries on, as the stores share a request in flight.
     * Otherwise (a pull with device security on, or a requested sync) each source keeps its records on screen until its
     * own fetch succeeds, so a failed one keeps the saved records; the requested sync also waits for a first load.
     */
    async function syncRecords({
        pulled,
        fromEmpty,
    }: {
        pulled: boolean;
        fromEmpty: boolean;
    }) {
        if (!hdid || (!pulled && !isFullyLoaded)) return;
        const errors = useErrorStore.getState();
        errors.clearErrors();
        errors.clearTooManyRequestsWarning();
        if (fromEmpty)
            clearTimelineDatasets(hdid, useUserStore.getState().user.hdid);
        else markTimelineDatasetsStale(hdid);
        // the page's own refresh indicator says this already
        setPulling(true);
        let timer: ReturnType<typeof setTimeout> | undefined;
        await Promise.race([
            fetchTimelineData(),
            new Promise((resolve) => {
                timer = setTimeout(resolve, REFRESH_SPINNER_MAX_MS);
            }),
        ]);
        clearTimeout(timer);
        setPulling(false);
    }
    // with no saved copy a pull starts from empty; with one, nothing saved is lost until its source syncs (owner, 2026-11-05)
    usePageRefresh(
        refreshable && canFetch
            ? () => {
                  // the list starts again from the first page, as it does on a first load
                  if (!lockEnabled) setCurrentPage(1);
                  return syncRecords({ pulled: true, fromEmpty: !lockEnabled });
              }
            : undefined
    );

    // Native, no saved copy: records are fetched on the first load, by pull to refresh, and when a sync is requested
    // (logged in again after the session ended), once they are in view. Not on a timer or each time the tab is shown:
    // that volume of queries is unwanted (owner, 2026-11-05).
    const handledSyncRequests = useRef(syncRequests);
    useEffect(() => {
        // Device security: the saved copy stays until it is old enough (firstRecordSync) or refreshed by hand
        if (!refreshable || lockEnabled) return;
        if (handledSyncRequests.current === syncRequests) return;
        if (!focused || locked || !canFetch) return;
        handledSyncRequests.current = syncRequests;
        // reacting to an outside request (a new login): starting the fetch, and saying so, is the point of this effect
        // eslint-disable-next-line react-hooks/set-state-in-effect
        void syncRecords({ pulled: false, fromEmpty: false });
        // only when a request arrives or the records come back into view, not when what they show changes
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [focused, locked, canFetch, syncRequests]);

    // watch(selectedDate): after a note is added, show the page that holds it
    const [prevSelectedDate, setPrevSelectedDate] = useState(selectedDate);
    if (prevSelectedDate !== selectedDate) {
        setPrevSelectedDate(selectedDate);
        if (selectedDate) {
            const index = filteredTimelineEntries.findIndex((entry) =>
                entry.date.hasSame(selectedDate, "day")
            );
            if (index >= 0) setCurrentPage(Math.floor(index / pageSize) + 1);
        }
    }

    // onBeforeUnmount(clearFilters)
    useEffect(
        () => () =>
            useTimelineStore
                .getState()
                .setFilter(TimelineFilterBuilder.create()),
        []
    );

    function changePage(next: number) {
        setCurrentPage(next);
        scrollToTop();
        const first = filteredTimelineEntries[(next - 1) * pageSize];
        if (first) useTimelineStore.getState().setLinearDate(first.date);
    }

    const showSkeleton =
        selectedEntryTypes.size > 0
            ? selectedDatasetsAreLoading
            : !isFullyLoaded && filteredTimelineEntries.length === 0;

    return (
        <View>
            {!isFullyLoaded && !pulling && !syncing && Platform.OS !== "web" ? (
                <LoadingToast />
            ) : null}
            {labResultsAreQueued ? (
                <HgAlert
                    type="info"
                    closable
                    testID="laboratory-orders-queued-alert-message"
                >
                    <AppText>
                        Your lab results are still being processed.
                    </AppText>
                </HgAlert>
            ) : null}
            {unfilteredTimelineEntries.length > 0 ? (
                <View>
                    {/* an outline for the screen reader (stated deviation: the web client shows no such heading) */}
                    <VisuallyHidden headingLevel={2}>Filters</VisuallyHidden>
                    {/* v-banner sticky: the Filter button, the active filter labels and Clear All */}
                    <View
                        className="flex-row items-center px-4 py-2 -mt-2 mb-2 -mx-4 bg-background"
                        style={
                            Platform.OS === "web"
                                ? ({
                                      position: "sticky",
                                      top: 0,
                                      zIndex: 2,
                                  } as object)
                                : undefined
                        }
                    >
                        <FilterDialog
                            entryTypes={entryTypes}
                            getCount={getCount}
                        />
                        <View className="flex-1 flex-row flex-wrap items-center ml-3">
                            {filterLabels.map(([label, value]) => (
                                <View
                                    key={`${label}-${value}`}
                                    className="mr-1 mb-1"
                                >
                                    <HgChip
                                        testID="filter-label"
                                        // the name holds the visible value (WCAG 2.5.3); the × stays "Close"
                                        // (Timeline.test.tsx finds it by that name; a fuller name is the owner's call)
                                        accessibilityLabel={`${label} filter: ${value}`}
                                        size={isMobile ? "small" : "default"}
                                        onClose={() =>
                                            clearFilter(label, value)
                                        }
                                    >
                                        {value}
                                    </HgChip>
                                </View>
                            ))}
                        </View>
                        {filterLabels.length > 0 ? (
                            <HgButton
                                testID="clear-filters-button"
                                variant="link"
                                text="Clear All"
                                onPress={clearFilters}
                            />
                        ) : null}
                    </View>
                    {visibleTimelineEntries.length > 0 ? (
                        // a status region: a filter or page change announces the new count
                        <LiveRegion politeness="polite">
                            <AppText
                                testID="timeline-record-count"
                                className="mb-4"
                            >
                                {recordCountMessage}
                            </AppText>
                        </LiveRegion>
                    ) : null}
                    {isOnlyClinicalDocumentSelected ? (
                        <HgAlert
                            type="info"
                            closable
                            testID="timeline-clinical-document-alert"
                        >
                            <AppText>
                                Only documents shared by your provider at some
                                sites are available.{" "}
                                <ExternalLink href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide#clindocs">
                                    Learn more
                                </ExternalLink>
                                .
                            </AppText>
                        </HgAlert>
                    ) : isOnlyHealthVisitSelected ? (
                        <HgAlert
                            type="info"
                            closable
                            testID="timeline-health-visit-alert"
                        >
                            <HealthVisitBillingInfo />
                        </HgAlert>
                    ) : isOnlyImmunizationSelected ? (
                        <HgAlert
                            type="info"
                            closable
                            testID="timeline-immunization-alert"
                        >
                            <AppText>
                                If any of your immunizations are missing or
                                incorrect,{" "}
                                <ExternalLink
                                    href={
                                        TrackedExternalUrl.ImmunizationRecordBC
                                    }
                                    onPress={() =>
                                        services.tracking.trackEvent({
                                            action: Action.ExternalLink,
                                            text: Text.ImmunizationUpdateForm,
                                            origin: Origin.Timeline,
                                            destination:
                                                Destination.ImmunizationRecordBC,
                                            type: Type.Immunizations,
                                            url: TrackedExternalUrl.ImmunizationRecordBC,
                                        })
                                    }
                                >
                                    fill in this online form
                                </ExternalLink>
                                .
                            </AppText>
                        </HgAlert>
                    ) : isOnlyDiagnosticImagingSelected ? (
                        <HgAlert
                            type="info"
                            closable
                            testID="timeline-diagnostic-imaging-alert"
                        >
                            <AppText>
                                Most reports are available 10-14 days after your
                                procedure.{" "}
                                <ExternalLink href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords#medicalimaging">
                                    Learn more
                                </ExternalLink>
                                .
                            </AppText>
                        </HgAlert>
                    ) : isOnlyBcCancerScreeningSelected ? (
                        <HgAlert
                            type="info"
                            closable
                            testID="timeline-cancer-screening-alert"
                        >
                            <AppText>
                                BC Cancer screening letters for Breast, Colon,
                                Lung, and Cervix are available here. Your Health
                                Gateway timeline may include these and other
                                screening test results in lab or imaging
                                reports.{" "}
                                <ExternalLink href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords">
                                    Learn more
                                </ExternalLink>
                                .
                            </AppText>
                        </HgAlert>
                    ) : null}
                    {visibleTimelineEntries.length > 0 ? (
                        <View className="mb-4">
                            <VisuallyHidden headingLevel={2}>
                                Records
                            </VisuallyHidden>
                            {dateGroups.map((group) =>
                                group.entries.map((entry) => (
                                    <TimelineEntryView
                                        // a new filter renders the cards again, collapsed, as v-expansion-panels does
                                        key={`${entry.type}-${entry.id}-${filterKey}`}
                                        testID="timelineCard"
                                        entry={entry}
                                        hdid={hdid}
                                        commentsAreEnabled={commentsAreEnabled}
                                    />
                                ))
                            )}
                        </View>
                    ) : null}
                </View>
            ) : null}
            {showSkeleton ? <TimelineSkeleton /> : null}
            {filteredTimelineEntries.length > 0 ? (
                <HgPagination
                    testID="pagination"
                    length={numberOfPages}
                    page={page}
                    onChange={changePage}
                    totalVisible={isMobile ? 3 : 8}
                    density={isMobile ? "comfortable" : "default"}
                />
            ) : null}
            {filteredTimelineEntries.length === 0 &&
            !selectedDatasetsAreLoading &&
            isFullyLoaded ? (
                <View className="items-center">
                    <Image
                        source={EmptyState}
                        style={{
                            width: 200,
                            height: 200 * (294 / 207),
                            marginBottom: 16,
                        }}
                        // decorative: the text under it says it all (the web client's alt is "...")
                        accessible={false}
                        importantForAccessibility="no"
                    />
                    <LiveRegion politeness="polite">
                        <AppText
                            testID="noTimelineEntriesText"
                            className="text-bc-grey"
                        >
                            {hasActiveFilter
                                ? "No records found with the selected filters"
                                : "No records found"}
                        </AppText>
                    </LiveRegion>
                </View>
            ) : null}
            {!isFullyLoaded && Platform.OS === "web" ? <LoadingToast /> : null}
            <ProtectiveWordDialog hdid={hdid} />
            <FullscreenTimelineEntry
                hdid={hdid}
                commentsAreEnabled={commentsAreEnabled}
            />
        </View>
    );
}
