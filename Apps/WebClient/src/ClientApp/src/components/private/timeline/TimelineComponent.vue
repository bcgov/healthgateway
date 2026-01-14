<script setup lang="ts">
import {
    ComponentPublicInstance,
    computed,
    nextTick,
    onBeforeUnmount,
    ref,
    watch,
} from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import FilterComponent from "@/components/private/timeline/FilterComponent.vue";
import FullscreenTimelineEntryComponent from "@/components/private/timeline/FullscreenTimelineEntryComponent.vue";
import ProtectiveWordComponent from "@/components/site/ProtectiveWordComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType } from "@/constants/errorType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import {
    BcCancerScreening,
    DiagnosticImagingExam,
    HealthDataType,
    PatientDataType,
} from "@/models/patientDataResponse";
import BcCancerScreeningTimelineEntry from "@/models/timeline/bcCancerScreeningTimelineEntry";
import ClinicalDocumentTimelineEntry from "@/models/timeline/clinicalDocumentTimelineEntry";
import Covid19TestResultTimelineEntry from "@/models/timeline/covid19TestResultTimelineEntry";
import DiagnosticImagingTimelineEntry from "@/models/timeline/diagnosticImagingTimelineEntry";
import HealthVisitTimelineEntry from "@/models/timeline/healthVisitTimelineEntry";
import HospitalVisitTimelineEntry from "@/models/timeline/hospitalVisitTimelineEntry";
import ImmunizationTimelineEntry from "@/models/timeline/immunizationTimelineEntry";
import LabResultTimelineEntry from "@/models/timeline/labResultTimelineEntry";
import MedicationTimelineEntry from "@/models/timeline/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/timeline/noteTimelineEntry";
import SpecialAuthorityRequestTimelineEntry from "@/models/timeline/specialAuthorityRequestTimelineEntry";
import TimelineEntry, { DateGroup } from "@/models/timeline/timelineEntry";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import {
    Action,
    Destination,
    ExternalUrl,
    Origin,
    Text,
    Type,
} from "@/plugins/extensions";
import { ILogger, ITrackingService } from "@/services/interfaces";
import { entryTypeToPatientDataTypeMap } from "@/services/restPatientDataService";
import { useClinicalDocumentStore } from "@/stores/clinicalDocument";
import { useCommentStore } from "@/stores/comment";
import { useCovid19TestResultStore } from "@/stores/covid19TestResult";
import { useErrorStore } from "@/stores/error";
import { useHealthVisitStore } from "@/stores/healthVisit";
import { useHospitalVisitStore } from "@/stores/hospitalVisit";
import { useImmunizationStore } from "@/stores/immunization";
import { useLabResultStore } from "@/stores/labResult";
import { useLayoutStore } from "@/stores/layout";
import { useMedicationStore } from "@/stores/medication";
import { useNoteStore } from "@/stores/note";
import { usePatientDataStore } from "@/stores/patientData";
import { useSpecialAuthorityRequestStore } from "@/stores/specialAuthorityRequest";
import { useTimelineStore } from "@/stores/timeline";
import { useUserStore } from "@/stores/user";
import dataSourceUtil from "@/utility/dataSourceUtil";
import DateSortUtility from "@/utility/dateSortUtility";

interface Props {
    hdid: string;
    entryTypes?: EntryType[];
    commentsAreEnabled?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    entryTypes: () => [],
    commentsAreEnabled: false,
});

enum FilterLabelType {
    Keyword = "Keyword",
    Type = "Record Type",
    Date = "Date",
}

const pageSize = 25;

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);
const layoutStore = useLayoutStore();
const clinicalDocumentStore = useClinicalDocumentStore();
const commentStore = useCommentStore();
const covid19TestResultStore = useCovid19TestResultStore();
const errorStore = useErrorStore();
const healthVisitStore = useHealthVisitStore();
const hospitalVisitStore = useHospitalVisitStore();
const immunizationStore = useImmunizationStore();
const labResultStore = useLabResultStore();
const medicationStore = useMedicationStore();
const noteStore = useNoteStore();
const patientDataStore = usePatientDataStore();
const specialAuthorityRequestStore = useSpecialAuthorityRequestStore();
const timelineStore = useTimelineStore();
const userStore = useUserStore();

const currentPage = ref(1);

const dateRefs = ref(new Map<string, Element | null>());

const clinicalDocuments = computed(() =>
    clinicalDocumentStore.clinicalDocuments(props.hdid)
);
const clinicalDocumentsAreLoading = computed(() =>
    clinicalDocumentStore.clinicalDocumentsAreLoading(props.hdid)
);
const healthVisits = computed(() => healthVisitStore.healthVisits(props.hdid));
const healthVisitsAreLoading = computed(() =>
    healthVisitStore.healthVisitsAreLoading(props.hdid)
);
const hospitalVisits = computed(() =>
    hospitalVisitStore.hospitalVisits(props.hdid)
);
const hospitalVisitsAreLoading = computed(() =>
    hospitalVisitStore.hospitalVisitsAreLoading(props.hdid)
);
const immunizations = computed(() =>
    immunizationStore.immunizations(props.hdid)
);
const immunizationsAreLoading = computed(() =>
    immunizationStore.immunizationsAreLoading(props.hdid)
);
const immunizationsAreDeferred = computed(() =>
    immunizationStore.immunizationsAreDeferred(props.hdid)
);
const covid19TestResults = computed(() =>
    covid19TestResultStore.covid19TestResults(props.hdid)
);
const covid19TestResultsAreLoading = computed(() =>
    covid19TestResultStore.covid19TestResultsAreLoading(props.hdid)
);
const labResults = computed(() => labResultStore.labResults(props.hdid));
const labResultsAreLoading = computed(() =>
    labResultStore.labResultsAreLoading(props.hdid)
);
const labResultsAreQueued = computed(() =>
    labResultStore.labResultsAreQueued(props.hdid)
);
const medications = computed(() => medicationStore.medications(props.hdid));
const medicationsAreLoading = computed(() =>
    medicationStore.medicationsAreLoading(props.hdid)
);
const specialAuthorityRequests = computed(() =>
    specialAuthorityRequestStore.specialAuthorityRequests(props.hdid)
);
const specialAuthorityRequestsAreLoading = computed(() =>
    specialAuthorityRequestStore.specialAuthorityRequestsAreLoading(props.hdid)
);
const userNotes = computed(() => noteStore.notes);
const notesAreLoading = computed(() => noteStore.notesAreLoading);
const patientData = computed(() => patientDataStore.patientData);
const patientDataAreLoading = computed(() =>
    patientDataStore.patientDataAreLoading(props.hdid)
);
const getEntryComments = computed(() => commentStore.getEntryComments);
const commentsAreLoading = computed(() => commentStore.commentsAreLoading);
const selectedEntryTypes = computed(() => timelineStore.selectedEntryTypes);
const filter = computed(() => timelineStore.filter);
const hasActiveFilter = computed(() => timelineStore.hasActiveFilter);
const linearDate = computed(() => timelineStore.linearDate);
const selectedDate = computed(() => timelineStore.selectedDate);
const blockedDataSources = computed(() => userStore.blockedDataSources);
const lowerPageStart = computed(() => (currentPage.value - 1) * pageSize + 1);
const upperPageEnd = computed(() =>
    Math.min(currentPage.value * pageSize, filteredTimelineEntries.value.length)
);

const filterLabels = computed(() => {
    const labels: [string, string][] = [];

    if (filter.value.keyword) {
        labels.push([FilterLabelType.Keyword, `"${filter.value.keyword}"`]);
    }

    for (const entryType of selectedEntryTypes.value) {
        const label = entryTypeMap.get(entryType)?.name;
        if (label) {
            labels.push([FilterLabelType.Type, label]);
        }
    }

    const startDate = filter.value.startDate
        ? `From ${DateWrapper.fromIsoDate(filter.value.startDate).format()}`
        : "";
    const endDate = filter.value.endDate
        ? `To ${DateWrapper.fromIsoDate(filter.value.endDate).format()}`
        : "";
    if (startDate && endDate) {
        labels.push([FilterLabelType.Date, `${startDate} ${endDate}`]);
    } else if (startDate) {
        labels.push([FilterLabelType.Date, startDate]);
    } else if (endDate) {
        labels.push([FilterLabelType.Date, endDate]);
    }

    return labels;
});
const unfilteredTimelineEntries = computed(() => {
    const getComments = getEntryComments.value;
    const entries: TimelineEntry[] = [];

    // Add the Special Authority request entries to the timeline list
    for (const request of specialAuthorityRequests.value) {
        entries.push(
            new SpecialAuthorityRequestTimelineEntry(request, getComments)
        );
    }

    // Add the medication entries to the timeline list
    for (const medication of medications.value) {
        entries.push(new MedicationTimelineEntry(medication, getComments));
    }

    // Add the COVID-19 test result entries to the timeline list
    for (const result of covid19TestResults.value) {
        entries.push(new Covid19TestResultTimelineEntry(result, getComments));
    }

    // Add the lab result entries to the timeline list
    for (const order of labResults.value) {
        entries.push(new LabResultTimelineEntry(order, getComments));
    }

    // Add the health visit entries to the timeline list
    for (const healthVisit of healthVisits.value) {
        entries.push(new HealthVisitTimelineEntry(healthVisit, getComments));
    }

    // Add the hospital visit entries to the timeline list
    for (const visit of hospitalVisits.value) {
        entries.push(new HospitalVisitTimelineEntry(visit, getComments));
    }

    // Add the clinical document entries to the timeline list
    for (const doc of clinicalDocuments.value) {
        entries.push(new ClinicalDocumentTimelineEntry(doc, getComments));
    }

    // Add the note entries to the timeline list
    for (const note of userNotes.value) {
        entries.push(new NoteTimelineEntry(note));
    }

    // Add the immunization entries to the timeline list
    for (const immunization of immunizations.value) {
        entries.push(new ImmunizationTimelineEntry(immunization));
    }

    // Add patient data entries to the timeline list
    for (const exam of patientData.value(props.hdid, [
        PatientDataType.DiagnosticImaging,
        PatientDataType.BcCancerScreening,
    ])) {
        switch (exam.type) {
            case HealthDataType.DiagnosticImagingExam:
                entries.push(
                    new DiagnosticImagingTimelineEntry(
                        exam as DiagnosticImagingExam,
                        getComments
                    )
                );
                break;
            case HealthDataType.BcCancerScreening:
                entries.push(
                    new BcCancerScreeningTimelineEntry(
                        exam as BcCancerScreening,
                        getComments
                    )
                );
                break;
        }
    }

    // Sort entries with newest first
    entries.sort((a, b) => DateSortUtility.descending(a.date, b.date));

    return entries;
});
const filteredTimelineEntries = computed(() => {
    let entries = unfilteredTimelineEntries.value;
    if (filter.value.hasActiveFilter()) {
        entries = entries.filter((e) => e.filterApplies(filter.value));
    }
    return entries;
});
const numberOfPages = computed(() => {
    let pageCount = 1;
    if (filteredTimelineEntries.value.length > pageSize) {
        pageCount = Math.ceil(filteredTimelineEntries.value.length / pageSize);
    }
    return pageCount;
});
const visibleTimelineEntries = computed(() => {
    if (filteredTimelineEntries.value.length === 0) {
        return [];
    }

    // Get the section of the array that contains the paginated section
    const lowerIndex = (currentPage.value - 1) * pageSize;
    const upperIndex = Math.min(
        currentPage.value * pageSize,
        filteredTimelineEntries.value.length
    );

    return filteredTimelineEntries.value.slice(lowerIndex, upperIndex);
});
const dateGroups = computed(() => {
    const newGroupArray = DateGroup.createGroups(visibleTimelineEntries.value);

    return DateGroup.sortGroups(newGroupArray);
});
const selectedDatasetsAreLoading = computed(() => {
    return props.entryTypes.some(
        (entryType) =>
            selectedEntryTypes.value.has(entryType) &&
            datasetIsLoading(entryType)
    );
});
const isFullyLoaded = computed(() => {
    const loadingDatasets = props.entryTypes.some((entryType) =>
        datasetIsLoading(entryType)
    );

    const loadingComments =
        props.commentsAreEnabled && commentsAreLoading.value;

    const loadingMoreImmunizations =
        props.entryTypes.includes(EntryType.Immunization) &&
        immunizationsAreDeferred.value;

    return !loadingDatasets && !loadingComments && !loadingMoreImmunizations;
});
const isOnlyClinicalDocumentSelected = computed(
    () =>
        selectedEntryTypes.value.size === 1 &&
        selectedEntryTypes.value.has(EntryType.ClinicalDocument)
);
const isOnlyHealthVisitSelected = computed(
    () =>
        selectedEntryTypes.value.size === 1 &&
        selectedEntryTypes.value.has(EntryType.HealthVisit)
);
const isOnlyImmunizationSelected = computed(
    () =>
        selectedEntryTypes.value.size === 1 &&
        selectedEntryTypes.value.has(EntryType.Immunization)
);
const isOnlyDiagnosticImagingSelected = computed(
    () =>
        selectedEntryTypes.value.size === 1 &&
        selectedEntryTypes.value.has(EntryType.DiagnosticImaging)
);
const isOnlyBcCancerScreeningSelected = computed(
    () =>
        selectedEntryTypes.value.size === 1 &&
        selectedEntryTypes.value.has(EntryType.BcCancerScreening)
);
const recordCountMessage = computed(() =>
    filteredTimelineEntries.value.length === 1
        ? "Displaying 1 out of 1 records"
        : `Displaying ${lowerPageStart.value} to ${upperPageEnd.value} out of ${filteredTimelineEntries.value.length} records`
);

function clearFilter(label: string, value: string | undefined): void {
    let keyword = filter.value.keyword;
    let startDate = filter.value.startDate;
    let endDate = filter.value.endDate;
    let entryTypes = [...selectedEntryTypes.value];

    switch (label) {
        case FilterLabelType.Keyword:
            keyword = "";
            break;
        case FilterLabelType.Date:
            startDate = "";
            endDate = "";
            break;
        case FilterLabelType.Type:
            entryTypes = entryTypes.filter(
                (e) => entryTypeMap.get(e)?.name !== value
            );
            break;
    }

    timelineStore.setFilter(
        TimelineFilterBuilder.create()
            .withKeyword(keyword)
            .withStartDate(startDate)
            .withEndDate(endDate)
            .withEntryTypes(entryTypes)
    );
}

function clearFilters(): void {
    timelineStore.setFilter(TimelineFilterBuilder.create());
}

function datasetIsLoading(entryType: EntryType): boolean {
    switch (entryType) {
        case EntryType.ClinicalDocument:
            return clinicalDocumentsAreLoading.value;
        case EntryType.Covid19TestResult:
            return covid19TestResultsAreLoading.value;
        case EntryType.HealthVisit:
            return healthVisitsAreLoading.value;
        case EntryType.HospitalVisit:
            return hospitalVisitsAreLoading.value;
        case EntryType.Immunization:
            return immunizationsAreLoading.value;
        case EntryType.LabResult:
            return labResultsAreLoading.value;
        case EntryType.Medication:
            return medicationsAreLoading.value;
        case EntryType.Note:
            return notesAreLoading.value;
        case EntryType.SpecialAuthorityRequest:
            return specialAuthorityRequestsAreLoading.value;
        case EntryType.DiagnosticImaging:
        case EntryType.BcCancerScreening:
            return patientDataAreLoading.value;
        default:
            throw new Error(`Unknown dataset "${entryType}"`);
    }
}

function fetchDataset(entryType: EntryType): Promise<unknown> {
    switch (entryType) {
        case EntryType.ClinicalDocument:
            return clinicalDocumentStore.retrieveClinicalDocuments(props.hdid);
        case EntryType.Covid19TestResult:
            return covid19TestResultStore.retrieveCovid19TestResults(
                props.hdid
            );
        case EntryType.HealthVisit:
            return healthVisitStore.retrieveHealthVisits(props.hdid);
        case EntryType.HospitalVisit:
            return hospitalVisitStore.retrieveHospitalVisits(props.hdid);
        case EntryType.Immunization:
            return immunizationStore.retrieveImmunizations(props.hdid);
        case EntryType.LabResult:
            return labResultStore.retrieveLabResults(props.hdid);
        case EntryType.Medication:
            return medicationStore.retrieveMedications(props.hdid);
        case EntryType.Note:
            return noteStore.retrieveNotes(props.hdid);
        case EntryType.SpecialAuthorityRequest:
            return specialAuthorityRequestStore.retrieveSpecialAuthorityRequests(
                props.hdid
            );
        default:
            return Promise.reject(new Error(`Unknown dataset "${entryType}"`));
    }
}

function fetchTimelineData(): Promise<unknown> {
    const blockedEntryTypes: EntryType[] = [];
    const patientDataEntryTypes = [...entryTypeToPatientDataTypeMap.keys()];
    const patientDataEntryTypesToRequest = [];
    const promises: Promise<unknown>[] = [];
    for (const entryType of props.entryTypes) {
        if (canAccessDataset(entryType)) {
            if (patientDataEntryTypes.includes(entryType)) {
                patientDataEntryTypesToRequest.push(
                    entryTypeToPatientDataTypeMap.get(
                        entryType
                    ) as PatientDataType
                );
            } else {
                promises.push(fetchDataset(entryType));
            }
        } else {
            blockedEntryTypes.push(entryType);
        }
    }

    if (patientDataEntryTypesToRequest.length > 0) {
        promises.push(
            patientDataStore.retrievePatientData(
                props.hdid,
                patientDataEntryTypesToRequest
            )
        );
    }

    if (props.commentsAreEnabled) {
        promises.push(commentStore.retrieveComments(props.hdid));
    }

    showDatasetWarning(blockedEntryTypes);

    return Promise.all(promises).catch((err) =>
        logger.error(`Error loading timeline data: ${JSON.stringify(err)}`)
    );
}

function canAccessDataset(entryType: EntryType): boolean {
    const dataSource = dataSourceUtil.getDataSource(entryType);
    const canAccess = !blockedDataSources.value.includes(dataSource);
    logger.debug(`Can access data source for ${dataSource}: ${canAccess}`);
    return canAccess;
}

function showDatasetWarning(entryTypes: EntryType[]): void {
    const entryTypesLength = entryTypes.length;
    logger.debug(
        `Cannot access dataset entry types length: ${entryTypesLength}`
    );

    if (entryTypesLength === 0) {
        return;
    }

    let warningMessage = "";
    if (entryTypesLength > 1) {
        warningMessage =
            "Multiple records are unavailable at this time. Please try again later.";
    } else {
        const entryType: EntryType = entryTypes[0];
        logger.debug(`Setting up warning message for: ${entryType}`);
        const name = entryTypeMap.get(entryType)?.name;
        warningMessage = name
            ? `${name} are unavailable at this time. Please try again later.`
            : `Some records may be unavailable at this time, please try again later.`;
    }
    errorStore.addCustomError(warningMessage, ErrorSourceType.User, undefined);
}

function setDateGroupRef(
    key: string,
    el: Element | ComponentPublicInstance | null
): void {
    dateRefs.value.set(key, el as Element);
}

function focusOnDate(date: IDateWrapper): void {
    const dateEpoch = date.fromEpoch().toString();
    const container = dateRefs.value.get(dateEpoch);
    container?.querySelector("button")?.focus();
}

function getComponentForEntry(entryType: EntryType) {
    return entryTypeMap.get(entryType)?.component ?? "";
}

function setPageFromDate(eventDate: IDateWrapper): boolean {
    const index = filteredTimelineEntries.value.findIndex((entry) =>
        eventDate.isSame(entry.date)
    );

    if (index >= 0) {
        currentPage.value = Math.floor(index / pageSize) + 1;
        return true;
    } else {
        return false;
    }
}

watch(currentPage, () => {
    window.scrollTo(0, 0);
    // Handle the current page being beyond the max number of pages
    if (currentPage.value > numberOfPages.value) {
        currentPage.value = numberOfPages.value;
        return;
    }

    if (visibleTimelineEntries.value.length > 0) {
        // Update the store
        timelineStore.setLinearDate(visibleTimelineEntries.value[0].date);
    }
});

watch(selectedDate, async () => {
    if (selectedDate.value && setPageFromDate(selectedDate.value)) {
        // Wait for next render cycle until the pages have been calculated and displayed
        await nextTick();

        focusOnDate(selectedDate.value);
    }
});

onBeforeUnmount(clearFilters);

fetchTimelineData();
setPageFromDate(linearDate.value);
</script>

<template>
    <div>
        <HgAlertComponent
            v-if="labResultsAreQueued"
            type="info"
            data-testid="laboratory-orders-queued-alert-message"
            closable
            variant="outlined"
        >
            Your lab results are still being processed.
        </HgAlertComponent>
        <template v-if="unfilteredTimelineEntries.length > 0">
            <v-banner
                class="timeline-filter-banner d-print-none px-4 py-2 mt-n2 mb-2 mx-n4 overflow-visible"
                sticky
                border="0"
            >
                <v-row class="align-center" dense>
                    <v-col cols="auto">
                        <FilterComponent
                            :hdid="hdid"
                            :entry-types="entryTypes"
                        />
                    </v-col>
                    <v-col>
                        <v-chip
                            v-for="[label, value] in filterLabels"
                            :key="`${label}-${value}`"
                            data-testid="filter-label"
                            class="mr-1 mb-1"
                            :title="`${label} Filter`"
                            closable
                            :size="layoutStore.isMobile ? 'small' : 'default'"
                            @click:close="clearFilter(label, value)"
                        >
                            {{ value }}
                        </v-chip>
                    </v-col>
                    <v-col cols="auto">
                        <HgButtonComponent
                            v-if="filterLabels.length > 0"
                            data-testid="clear-filters-button"
                            variant="link"
                            text="Clear All"
                            @click="clearFilters"
                        />
                    </v-col>
                </v-row>
            </v-banner>
            <p
                v-if="visibleTimelineEntries.length > 0"
                data-testid="timeline-record-count"
                class="text-body-1"
            >
                {{ recordCountMessage }}
            </p>
            <HgAlertComponent
                v-if="isOnlyClinicalDocumentSelected"
                type="info"
                data-testid="timeline-clinical-document-alert"
                closable
                variant="outlined"
            >
                Only documents shared by your provider at some sites are
                available.
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide#clindocs"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >Learn more</a
                >.
            </HgAlertComponent>
            <HgAlertComponent
                v-else-if="isOnlyHealthVisitSelected"
                type="info"
                data-testid="timeline-health-visit-alert"
                closable
                variant="outlined"
            >
                <p class="text-body-1">
                    Information is from the billing claim and may show a
                    different practitioner or clinic from the one you visited.
                    For more information, visit the
                    <a
                        href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80#healthandhospital"
                        target="_blank"
                        rel="noopener"
                        class="text-link"
                        >FAQ</a
                    >
                    page.
                </p>
                <p class="text-body-1">
                    Health Gateway shows your health visits billed to the BC
                    Medical Services Plan (MSP) for the past seven years. You
                    can also get your hospital visits since 2021 for everywhere
                    except Interior Health. We are working on adding Interior
                    visits in the future.
                </p>
            </HgAlertComponent>
            <HgAlertComponent
                v-else-if="isOnlyImmunizationSelected"
                type="info"
                data-testid="timeline-immunization-alert"
                closable
                variant="outlined"
            >
                If any of your immunizations are missing or incorrect,
                <a
                    :href="ExternalUrl.ImmunizationRecordBC"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    @click="
                        trackingService.trackEvent({
                            action: Action.ExternalLink,
                            text: Text.ImmunizationUpdateForm,
                            origin: Origin.Timeline,
                            destination: Destination.ImmunizationRecordBC,
                            type: Type.Immunizations,
                            url: ExternalUrl.ImmunizationRecordBC,
                        })
                    "
                    >fill in this online form</a
                >.
            </HgAlertComponent>
            <HgAlertComponent
                v-else-if="isOnlyDiagnosticImagingSelected"
                type="info"
                data-testid="timeline-diagnostic-imaging-alert"
                closable
                variant="outlined"
            >
                Most reports are available 10-14 days after your procedure.
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords#medicalimaging"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >Learn more</a
                >.
            </HgAlertComponent>
            <HgAlertComponent
                v-else-if="isOnlyBcCancerScreeningSelected"
                type="info"
                data-testid="timeline-cancer-screening-alert"
                closable
                variant="outlined"
            >
                BC Cancer screening letters for Breast, Colon, Lung, and Cervix
                are available here. Your Health Gateway timeline may include
                these and other screening test results in lab or imaging
                reports.
                <a
                    href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide/healthrecords"
                    target="_blank"
                    rel="noopener"
                    class="text-link"
                    >Learn more</a
                >.
            </HgAlertComponent>
            <div v-if="visibleTimelineEntries.length > 0" class="mb-4">
                <template v-for="dateGroup in dateGroups" :key="dateGroup.key">
                    <component
                        :is="getComponentForEntry(entry.type)"
                        v-for="(entry, index) in dateGroup.entries"
                        :ref="
                            (el: Element) =>
                                index === 0
                                    ? setDateGroupRef(dateGroup.key, el)
                                    : undefined
                        "
                        :key="entry.type + '-' + entry.id"
                        :datekey="dateGroup.key"
                        :entry="entry"
                        :index="index"
                        :hdid="hdid"
                        :comments-are-enabled="commentsAreEnabled"
                        data-testid="timelineCard"
                    />
                </template>
            </div>
        </template>
        <v-skeleton-loader
            v-if="
                selectedEntryTypes.size > 0
                    ? selectedDatasetsAreLoading
                    : !isFullyLoaded && filteredTimelineEntries.length === 0
            "
            data-testid="content-placeholders"
            class="py-2 mb-4"
            type="divider, avatar, heading, divider, avatar, heading, divider, avatar, heading, divider"
        />
        <div v-if="filteredTimelineEntries.length > 0" class="text-center mb-4">
            <v-pagination
                v-model="currentPage"
                data-testid="pagination"
                :length="numberOfPages"
                :total-visible="layoutStore.isMobile ? 3 : 8"
                :density="layoutStore.isMobile ? 'comfortable' : 'default'"
            />
        </div>
        <div
            v-if="
                filteredTimelineEntries.length === 0 &&
                !selectedDatasetsAreLoading &&
                isFullyLoaded
            "
            class="d-flex flex-column align-center mb-4"
        >
            <v-img
                src="@/assets/images/timeline/empty-state.png"
                class="mb-4"
                width="200"
                alt="..."
            />
            <p
                class="text-body-1 text-medium-emphasis"
                data-testid="noTimelineEntriesText"
            >
                {{
                    hasActiveFilter
                        ? "No records found with the selected filters"
                        : "No records found"
                }}
            </p>
        </div>
        <v-snackbar data-testid="loading-toast" :model-value="!isFullyLoaded">
            Retrieving your health records
        </v-snackbar>
        <ProtectiveWordComponent :hdid="hdid" />
        <FullscreenTimelineEntryComponent
            :hdid="hdid"
            :comments-are-enabled="commentsAreEnabled"
        />
    </div>
</template>

<style lang="scss" scoped>
.timeline-filter-banner {
    z-index: 2;
    width: calc(100% + 32px);
    top: var(--v-layout-top);
    transition: 0.2s cubic-bezier(0.4, 0, 0.2, 1);
}
</style>
