<script setup lang="ts">
import { BToast } from "bootstrap-vue";
import {
    Component,
    ComponentPublicInstance,
    computed,
    nextTick,
    onBeforeUnmount,
    onMounted,
    ref,
    watch,
} from "vue";
import { useStore } from "vue-composition-wrapper";

import ProtectiveWordComponent from "@/components/modal/ProtectiveWordComponent.vue";
import EntryDetailsComponent from "@/components/timeline/entryCard/EntryDetailsComponent.vue";
import FilterComponent from "@/components/timeline/FilterComponent.vue";
import { DataSource } from "@/constants/dataSource";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { ErrorSourceType } from "@/constants/errorType";
import { ClinicalDocument } from "@/models/clinicalDocument";
import ClinicalDocumentTimelineEntry from "@/models/clinicalDocumentTimelineEntry";
import Covid19LaboratoryOrderTimelineEntry from "@/models/covid19LaboratoryOrderTimelineEntry";
import { DateWrapper, IDateWrapper } from "@/models/dateWrapper";
import DiagnosticImagingTimelineEntry from "@/models/diagnosticImagingTimelineEntry";
import { Encounter, HospitalVisit } from "@/models/encounter";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import HospitalVisitTimelineEntry from "@/models/hospitalVisitTimelineEntry";
import { ImmunizationEvent } from "@/models/immunizationModel";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import { Covid19LaboratoryOrder, LaboratoryOrder } from "@/models/laboratory";
import LaboratoryOrderTimelineEntry from "@/models/laboratoryOrderTimelineEntry";
import MedicationRequest from "@/models/medicationRequest";
import MedicationRequestTimelineEntry from "@/models/medicationRequestTimelineEntry";
import MedicationStatementHistory from "@/models/medicationStatementHistory";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import {
    DiagnosticImagingExam,
    PatientData,
    PatientDataType,
} from "@/models/patientDataResponse";
import TimelineEntry, { DateGroup } from "@/models/timelineEntry";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { UserComment } from "@/models/userComment";
import UserNote from "@/models/userNote";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import dataSourceUtil from "@/utility/dataSourceUtil";

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
const store = useStore();

const currentPage = ref(1);

const dateRefs = ref(new Map<string, Element | null>());

const clinicalDocuments = computed<ClinicalDocument[]>(() =>
    store.getters["clinicalDocument/clinicalDocuments"](props.hdid)
);
const clinicalDocumentsAreLoading = computed<boolean>(() =>
    store.getters["clinicalDocument/clinicalDocumentsAreLoading"](props.hdid)
);
const healthVisits = computed<Encounter[]>(() =>
    store.getters["encounter/healthVisits"](props.hdid)
);
const healthVisitsAreLoading = computed<boolean>(() =>
    store.getters["encounter/healthVisitsAreLoading"](props.hdid)
);
const hospitalVisits = computed<HospitalVisit[]>(() =>
    store.getters["encounter/hospitalVisits"](props.hdid)
);
const hospitalVisitsAreLoading = computed<boolean>(() =>
    store.getters["encounter/hospitalVisitsAreLoading"](props.hdid)
);
const immunizations = computed<ImmunizationEvent[]>(() =>
    store.getters["immunization/immunizations"](props.hdid)
);
const immunizationsAreLoading = computed<boolean>(() =>
    store.getters["immunization/immunizationsAreLoading"](props.hdid)
);
const immunizationsAreDeferred = computed<boolean>(() =>
    store.getters["immunization/immunizationsAreDeferred"](props.hdid)
);
const covid19LaboratoryOrders = computed<Covid19LaboratoryOrder[]>(() =>
    store.getters["laboratory/covid19LaboratoryOrders"](props.hdid)
);
const covid19LaboratoryOrdersAreLoading = computed<boolean>(() =>
    store.getters["laboratory/covid19LaboratoryOrdersAreLoading"](props.hdid)
);
const laboratoryOrders = computed<LaboratoryOrder[]>(() =>
    store.getters["laboratory/laboratoryOrders"](props.hdid)
);
const laboratoryOrdersAreLoading = computed<boolean>(() =>
    store.getters["laboratory/laboratoryOrdersAreLoading"](props.hdid)
);
const laboratoryOrdersAreQueued = computed<boolean>(() =>
    store.getters["laboratory/laboratoryOrdersAreQueued"](props.hdid)
);
const medications = computed<MedicationStatementHistory[]>(() =>
    store.getters["medication/medications"](props.hdid)
);
const medicationsAreLoading = computed<boolean>(() =>
    store.getters["medication/medicationsAreLoading"](props.hdid)
);
const specialAuthorityRequests = computed<MedicationRequest[]>(() =>
    store.getters["medication/specialAuthorityRequests"](props.hdid)
);
const specialAuthorityRequestsAreLoading = computed<boolean>(() =>
    store.getters["medication/specialAuthorityRequestsAreLoading"](props.hdid)
);
const userNotes = computed<UserNote[]>(() => store.getters["note/notes"]);
const notesAreLoading = computed<boolean>(
    () => store.getters["note/notesAreLoading"]
);
const patientData = computed<
    (hdid: string, patientDataTypes: PatientDataType[]) => PatientData[]
>(() => store.getters["patientData/patientData"]);
const patientDataAreLoading = computed<boolean>(() =>
    store.getters["patientData/patientDataAreLoading"](props.hdid)
);
const getEntryComments = computed<(entryId: string) => UserComment[] | null>(
    () => store.getters["comment/getEntryComments"]
);
const commentsAreLoading = computed<boolean>(
    () => store.getters["comment/commentsAreLoading"]
);
const isHeaderShown = computed<boolean>(
    () => store.getters["navbar/isHeaderShown"]
);
const selectedEntryTypes = computed<Set<EntryType>>(
    () => store.getters["timeline/selectedEntryTypes"]
);
const filter = computed<TimelineFilter>(() => store.getters["timeline/filter"]);
const hasActiveFilter = computed<boolean>(
    () => store.getters["timeline/hasActiveFilter"]
);
const linearDate = computed<IDateWrapper>(
    () => store.getters["timeline/linearDate"]
);
const selectedDate = computed<IDateWrapper | null>(
    () => store.getters["timeline/selectedDate"]
);
const blockedDataSources = computed<DataSource[]>(
    () => store.getters["user/blockedDataSources"]
);

const filterLabels = computed(() => {
    const labels: [string, string][] = [];

    if (filter.value.keyword) {
        labels.push([FilterLabelType.Keyword, `"${filter.value.keyword}"`]);
    }

    selectedEntryTypes.value.forEach((entryType) => {
        const label = entryTypeMap.get(entryType)?.name;
        if (label) {
            labels.push([FilterLabelType.Type, label]);
        }
    });

    const startDate = filter.value.startDate
        ? `From ${new DateWrapper(filter.value.startDate).format()}`
        : "";
    const endDate = filter.value.endDate
        ? `To ${new DateWrapper(filter.value.endDate).format()}`
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
        entries.push(new MedicationRequestTimelineEntry(request, getComments));
    }

    // Add the medication entries to the timeline list
    for (const medication of medications.value) {
        entries.push(new MedicationTimelineEntry(medication, getComments));
    }

    // Add the COVID-19 test result entries to the timeline list
    for (const result of covid19LaboratoryOrders.value) {
        entries.push(
            new Covid19LaboratoryOrderTimelineEntry(result, getComments)
        );
    }

    // Add the lab result entries to the timeline list
    for (const order of laboratoryOrders.value) {
        entries.push(new LaboratoryOrderTimelineEntry(order, getComments));
    }

    // Add the health visit entries to the timeline list
    for (const healthVisit of healthVisits.value) {
        entries.push(new EncounterTimelineEntry(healthVisit, getComments));
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

    // Add the diagnostic imaging entries to the timeline list
    for (const exam of patientData.value(props.hdid, [
        PatientDataType.DiagnosticImaging,
    ]) as DiagnosticImagingExam[]) {
        entries.push(new DiagnosticImagingTimelineEntry(exam, getComments));
    }

    // Sort entries with newest first
    entries.sort((a, b) => {
        if (a.date.isBefore(b.date)) {
            return 1;
        }
        if (a.date.isAfter(b.date)) {
            return -1;
        }
        return 0;
    });

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
const showContentPlaceholders = computed(() => {
    if (selectedEntryTypes.value.size > 0) {
        return selectedDatasetsAreLoading.value;
    }
    return !isFullyLoaded.value && filteredTimelineEntries.value.length === 0;
});
const showDisplayCount = computed(
    () => visibleTimelineEntries.value.length > 0
);
const showEmptyState = computed(
    () =>
        filteredTimelineEntries.value.length === 0 &&
        !selectedDatasetsAreLoading.value
);
const showTimelineEntries = computed(
    () => unfilteredTimelineEntries.value.length > 0 || isFullyLoaded.value
);

function addCustomError(
    title: string,
    source: ErrorSourceType,
    traceId: string | undefined
): void {
    store.dispatch("errorBanner/addCustomError", { title, source, traceId });
}

function retrieveClinicalDocuments(hdid: string): Promise<void> {
    return store.dispatch("clinicalDocument/retrieveClinicalDocuments", {
        hdid,
    });
}

function retrieveHealthVisits(hdid: string): Promise<void> {
    return store.dispatch("encounter/retrieveHealthVisits", { hdid });
}

function retrieveHospitalVisits(hdid: string): Promise<void> {
    return store.dispatch("encounter/retrieveHospitalVisits", { hdid });
}

function retrieveImmunizations(hdid: string): Promise<void> {
    return store.dispatch("immunization/retrieveImmunizations", { hdid });
}

function retrieveCovid19LaboratoryOrders(hdid: string): Promise<void> {
    return store.dispatch("laboratory/retrieveCovid19LaboratoryOrders", {
        hdid,
    });
}

function retrieveLaboratoryOrders(hdid: string): Promise<void> {
    return store.dispatch("laboratory/retrieveLaboratoryOrders", { hdid });
}

function retrieveNotes(hdid: string): Promise<void> {
    return store.dispatch("note/retrieveNotes", { hdid });
}

function retrieveMedications(
    hdid: string,
    protectiveWord?: string
): Promise<void> {
    return store.dispatch("medication/retrieveMedications", {
        hdid,
        protectiveWord,
    });
}

function retrieveSpecialAuthorityRequests(hdid: string): Promise<void> {
    return store.dispatch("medication/retrieveSpecialAuthorityRequests", {
        hdid,
    });
}

function retrievePatientData(
    hdid: string,
    patientDataTypes: PatientDataType[]
): Promise<void> {
    return store.dispatch("patientData/retrievePatientData", {
        hdid,
        patientDataTypes,
    });
}

function retrieveComments(hdid: string): Promise<void> {
    return store.dispatch("comment/retrieveComments", { hdid });
}

function setFilter(filterBuilder: TimelineFilterBuilder): void {
    store.dispatch("timeline/setFilter", filterBuilder);
}

function setLinearDate(linearDate: DateWrapper): void {
    store.dispatch("timeline/setLinearDate", linearDate);
}

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

    const builder = TimelineFilterBuilder.create()
        .withKeyword(keyword)
        .withStartDate(startDate)
        .withEndDate(endDate)
        .withEntryTypes(entryTypes);

    setFilter(builder);
}

function clearFilters(): void {
    const builder = TimelineFilterBuilder.create();
    setFilter(builder);
}

function datasetIsLoading(entryType: EntryType): boolean {
    switch (entryType) {
        case EntryType.ClinicalDocument:
            return clinicalDocumentsAreLoading.value;
        case EntryType.Covid19TestResult:
            return covid19LaboratoryOrdersAreLoading.value;
        case EntryType.HealthVisit:
            return healthVisitsAreLoading.value;
        case EntryType.HospitalVisit:
            return hospitalVisitsAreLoading.value;
        case EntryType.Immunization:
            return immunizationsAreLoading.value;
        case EntryType.LabResult:
            return laboratoryOrdersAreLoading.value;
        case EntryType.Medication:
            return medicationsAreLoading.value;
        case EntryType.Note:
            return notesAreLoading.value;
        case EntryType.SpecialAuthorityRequest:
            return specialAuthorityRequestsAreLoading.value;
        case EntryType.DiagnosticImaging:
            return patientDataAreLoading.value;
        default:
            throw new Error(`Unknown dataset "${entryType}"`);
    }
}

function fetchDataset(entryType: EntryType): Promise<void> {
    switch (entryType) {
        case EntryType.ClinicalDocument:
            return retrieveClinicalDocuments(props.hdid);
        case EntryType.Covid19TestResult:
            return retrieveCovid19LaboratoryOrders(props.hdid);
        case EntryType.HealthVisit:
            return retrieveHealthVisits(props.hdid);
        case EntryType.HospitalVisit:
            return retrieveHospitalVisits(props.hdid);
        case EntryType.Immunization:
            return retrieveImmunizations(props.hdid);
        case EntryType.LabResult:
            return retrieveLaboratoryOrders(props.hdid);
        case EntryType.Medication:
            return retrieveMedications(props.hdid);
        case EntryType.Note:
            return retrieveNotes(props.hdid);
        case EntryType.SpecialAuthorityRequest:
            return retrieveSpecialAuthorityRequests(props.hdid);
        case EntryType.DiagnosticImaging:
            return retrievePatientData(props.hdid, [
                PatientDataType.DiagnosticImaging,
            ]);
        default:
            return Promise.reject(`Unknown dataset "${entryType}"`);
    }
}

function fetchTimelineData(): Promise<void | void[]> {
    const blockedEntryTypes: EntryType[] = [];

    const promises: Promise<void>[] = [];
    props.entryTypes.forEach((entryType) => {
        {
            if (canAccessDataset(entryType)) {
                promises.push(fetchDataset(entryType));
            } else {
                blockedEntryTypes.push(entryType);
            }
        }
    });

    if (props.commentsAreEnabled) {
        promises.push(retrieveComments(props.hdid));
    }

    showDatasetWarning(blockedEntryTypes);

    return Promise.all(promises).catch((err) =>
        logger.error(`Error loading timeline data: ${err}`)
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
    addCustomError(warningMessage, ErrorSourceType.User, undefined);
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

function getComponentForEntry(entryType: EntryType): Component | "" {
    return entryTypeMap.get(entryType)?.component ?? "";
}

function linkGen(pageNum: number): string {
    return `?page=${pageNum}`;
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
    // Handle the current page being beyond the max number of pages
    if (currentPage.value > numberOfPages.value) {
        currentPage.value = numberOfPages.value;
    }

    if (visibleTimelineEntries.value.length > 0) {
        // Update the store
        setLinearDate(visibleTimelineEntries.value[0].date);
    }
});

watch(selectedDate, async () => {
    if (selectedDate.value !== null && setPageFromDate(selectedDate.value)) {
        // Wait for next render cycle until the pages have been calculated and displayed
        await nextTick();

        focusOnDate(selectedDate.value as IDateWrapper);
    }
});

onBeforeUnmount(() => clearFilters());
onMounted(() => setPageFromDate(linearDate.value));

fetchTimelineData();
</script>

<template>
    <div>
        <b-toast
            v-if="!isFullyLoaded"
            id="loading-toast"
            visible
            toaster="b-toaster-top-center"
            variant="info"
            solid
            no-auto-hide
            no-close-button
            is-status
            data-testid="loading-toast"
        >
            <div class="text-center">Retrieving your health records</div>
        </b-toast>
        <div
            v-if="!isFullyLoaded"
            v-show="false"
            data-testid="loading-in-progress"
        />
        <b-alert
            v-if="laboratoryOrdersAreQueued"
            show
            dismissible
            variant="info"
            class="no-print"
            data-testid="laboratory-orders-queued-alert-message"
        >
            <span>
                We are getting your lab results. It may take up to 48 hours
                until you can see them.
            </span>
        </b-alert>
        <div
            v-if="showTimelineEntries"
            class="sticky-top sticky-offset"
            :class="{ 'header-offset': isHeaderShown }"
        >
            <b-row
                class="no-print justify-content-between py-2"
                align-v="start"
            >
                <b-col class="col-auto">
                    <FilterComponent
                        class="my-1"
                        :hdid="hdid"
                        :entry-types="entryTypes"
                    />
                </b-col>
                <b-col class="mx-2">
                    <b-form-tag
                        v-for="[label, value] in filterLabels"
                        :key="`${label}-${value}`"
                        variant="light"
                        class="filter-label p-1 mr-2 my-1"
                        :title="`${label} Filter`"
                        data-testid="filter-label"
                        @remove="clearFilter(label, value)"
                        >{{ value }}</b-form-tag
                    >
                    <hg-button
                        v-if="filterLabels.length > 0"
                        class="p-1 mt-n1"
                        data-testid="clear-filters-button"
                        variant="link"
                        @click="clearFilters"
                    >
                        Clear All
                    </hg-button>
                </b-col>
            </b-row>
        </div>
        <div v-show="showTimelineEntries">
            <b-row
                v-if="showDisplayCount"
                id="listControls"
                class="no-print"
                data-testid="displayCountText"
            >
                <b-col class="py-2" data-testid="timeline-record-count">
                    Displaying {{ visibleTimelineEntries.length }} out of
                    {{ filteredTimelineEntries.length }} records
                </b-col>
            </b-row>
            <div
                v-show="isOnlyClinicalDocumentSelected"
                id="timeline-clinical-document-disclaimer"
                class="pb-2"
            >
                <b-alert
                    show
                    variant="info"
                    class="mt-0 mb-1"
                    data-testid="timeline-clinical-document-disclaimer-alert"
                >
                    <span>
                        Only documents shared by your provider at some sites are
                        available.
                        <a
                            href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide#clindocs"
                            target="_blank"
                            rel="noopener"
                            >Learn more</a
                        >.
                    </span>
                </b-alert>
            </div>
            <div
                v-show="isOnlyImmunizationSelected"
                id="linear-timeline-immunization-disclaimer"
                class="pb-2"
            >
                <b-alert
                    show
                    variant="info"
                    class="mt-0 mb-1"
                    data-testid="linear-timeline-immunization-disclaimer-alert"
                >
                    <span>
                        If any of your immunizations are missing or incorrect,
                        <a
                            href="https://www.immunizationrecord.gov.bc.ca/"
                            target="_blank"
                            rel="noopener"
                            >fill in this online form</a
                        >.
                    </span>
                </b-alert>
            </div>
            <div
                v-show="isOnlyDiagnosticImagingSelected"
                id="linear-timeline-diagnostic-imaging-disclaimer"
                class="pb-2"
            >
                <b-alert
                    show
                    variant="info"
                    class="mt-0 mb-1"
                    data-testid="linear-timeline-diagnostic-imaging-disclaimer-alert"
                >
                    <span>
                        Most reports are available 10-14 days after your
                        procedure.
                        <a
                            href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway/guide#medicalimaging"
                            target="_blank"
                            rel="noopener"
                            >Learn more</a
                        >.
                    </span>
                </b-alert>
            </div>
            <div id="timeData" data-testid="linearTimelineData">
                <div
                    v-for="dateGroup in dateGroups"
                    :key="dateGroup.key"
                    :ref="(el) => setDateGroupRef(dateGroup.key, el)"
                >
                    <component
                        :is="getComponentForEntry(entry.type)"
                        v-for="(entry, index) in dateGroup.entries"
                        :key="entry.type + '-' + entry.id"
                        :datekey="dateGroup.key"
                        :entry="entry"
                        :index="index"
                        :hdid="hdid"
                        :comments-are-enabled="commentsAreEnabled"
                        data-testid="timelineCard"
                    />
                </div>
            </div>
            <b-row align-h="center">
                <b-col cols="auto">
                    <b-pagination-nav
                        v-if="filteredTimelineEntries.length > 0"
                        v-model="currentPage"
                        :link-gen="linkGen"
                        :number-of-pages="numberOfPages"
                        data-testid="pagination"
                        limit="4"
                        first-number
                        last-number
                        next-text="Next"
                        prev-text="Prev"
                        use-router
                        class="mt-3"
                    />
                </b-col>
            </b-row>
            <div v-if="showEmptyState" class="text-center pt-2">
                <b-row>
                    <b-col>
                        <img
                            class="mx-auto d-block"
                            src="@/assets/images/timeline/empty-state.png"
                            width="200"
                            height="auto"
                            alt="..."
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <p
                            class="text-center pt-2 noTimelineEntriesText"
                            data-testid="noTimelineEntriesText"
                        >
                            <span v-if="hasActiveFilter"
                                >No records found with the selected
                                filters</span
                            >
                            <span v-else>No records found</span>
                        </p>
                    </b-col>
                </b-row>
            </div>
        </div>
        <content-placeholders
            v-if="showContentPlaceholders"
            data-testid="content-placeholders"
        >
            <content-placeholders-heading :img="true" />
            <content-placeholders-text :lines="3" />
        </content-placeholders>
        <ProtectiveWordComponent :hdid="hdid" />
        <EntryDetailsComponent
            :hdid="hdid"
            :comments-are-enabled="commentsAreEnabled"
        />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.row {
    margin: 0;
    padding: 0;
}

.col {
    margin: 0;
    padding: 0;
}

.form-group {
    margin-bottom: 0;
}

.sticky-top {
    transition: all 0.3s;
    z-index: 49 !important;
}

.sticky-offset {
    background-color: white;
    z-index: 2;

    &.header-offset {
        top: $header-height;
    }
}

.noTimelineEntriesText {
    font-size: 1.5rem;
    color: #6c757d;
}

.filter-label {
    font-size: 1rem;
}
</style>
<style lang="scss">
ul.pagination {
    margin-bottom: 8px;
}
</style>
