<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgDatePickerComponent from "@/components/common/HgDatePickerComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import SectionHeaderComponent from "@/components/common/SectionHeaderComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import { PatientDataType } from "@/models/patientDataResponse";
import { TimelineFilterBuilder } from "@/models/timeline/timelineFilter";
import { Action, Text, Type } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useClinicalDocumentStore } from "@/stores/clinicalDocument";
import { useCovid19TestResultStore } from "@/stores/covid19TestResult";
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

interface EntryTypeFilter {
    type: EntryType;
    display: string;
}

interface Props {
    hdid: string;
    entryTypes?: EntryType[];
}
const props = withDefaults(defineProps<Props>(), {
    entryTypes: () => [],
});

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const layoutStore = useLayoutStore();
const timelineStore = useTimelineStore();
const clinicalDocumentStore = useClinicalDocumentStore();
const covid19TestResultStore = useCovid19TestResultStore();
const healthVisitStore = useHealthVisitStore();
const hospitalVisitStore = useHospitalVisitStore();
const immunizationStore = useImmunizationStore();
const labResultStore = useLabResultStore();
const medicationStore = useMedicationStore();
const noteStore = useNoteStore();
const specialAuthorityRequestStore = useSpecialAuthorityRequestStore();
const patientDataStore = usePatientDataStore();

const activeFilter = computed(() => timelineStore.filter);
const activeEntryTypes = computed(() => timelineStore.selectedEntryTypes);

const isVisible = ref(false);
const isFilterStartDateValidDate = ref(true);
const isFilterEndDateValidDate = ref(true);
const startDate = ref("");
const endDate = ref("");
const selectedEntryTypes = ref<EntryType[]>([]);
const keywordInputText = ref("");

const enabledEntryTypes = computed(() =>
    props.entryTypes
        .map<EntryTypeFilter>((entryType) => ({
            type: entryType,
            display: entryTypeMap.get(entryType)?.name ?? "",
        }))
        .filter((entryTypeFilter) => entryTypeFilter.display !== "")
);

function showDialog(): void {
    keywordInputText.value = activeFilter.value.keyword;
    startDate.value = activeFilter.value.startDate;
    endDate.value = activeFilter.value.endDate;
    selectedEntryTypes.value = [...activeEntryTypes.value];

    isVisible.value = true;

    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.FilterHealthRecords,
        type: Type.Filter,
    });
}

function hideDialog(): void {
    isVisible.value = false;
}

function apply(): void {
    const builder = TimelineFilterBuilder.create()
        .withKeyword(keywordInputText.value)
        .withStartDate(startDate.value)
        .withEndDate(endDate.value)
        .withEntryTypes(selectedEntryTypes.value);

    timelineStore.setFilter(builder);
    hideDialog();
}

function getFilterCount(entryType: EntryType): number | undefined {
    switch (entryType) {
        case EntryType.Note:
            return noteStore.notesCount;
        case EntryType.ClinicalDocument:
            return clinicalDocumentStore.clinicalDocumentsCount(props.hdid);
        case EntryType.Covid19TestResult:
            return covid19TestResultStore.covid19TestResultsCount(props.hdid);
        case EntryType.HealthVisit:
            return healthVisitStore.healthVisitsCount(props.hdid);
        case EntryType.HospitalVisit:
            return hospitalVisitStore.hospitalVisitsCount(props.hdid);
        case EntryType.Immunization:
            return immunizationStore.immunizationsCount(props.hdid);
        case EntryType.LabResult:
            return labResultStore.labResultsCount(props.hdid);
        case EntryType.Medication:
            return medicationStore.medicationsCount(props.hdid);
        case EntryType.SpecialAuthorityRequest:
            return specialAuthorityRequestStore.specialAuthorityRequestsCount(
                props.hdid
            );
        case EntryType.DiagnosticImaging:
            return patientDataStore.patientDataCount(props.hdid, [
                PatientDataType.DiagnosticImaging,
            ]);
        case EntryType.BcCancerScreening:
            return patientDataStore.patientDataCount(props.hdid, [
                PatientDataType.BcCancerScreening,
            ]);
        default:
            return undefined;
    }
}

function isSelected(entryType: EntryType) {
    return selectedEntryTypes.value.includes(entryType);
}

function getFormattedFilterCount(entryType: EntryType): string {
    const num = getFilterCount(entryType);

    if (num === undefined) {
        return "";
    }

    return Math.abs(num) > 999
        ? parseFloat(
              ((Math.round(num / 100) * 100) / 1000).toFixed(1)
          ).toString() + "K"
        : num.toString();
}
</script>

<template>
    <HgButtonComponent
        data-testid="filterDropdown"
        variant="secondary"
        prepend-icon="filter"
        @click="showDialog"
    >
        <span class="d-none d-sm-inline">Filter</span>
    </HgButtonComponent>
    <div class="d-flex justify-center">
        <v-dialog
            v-model="isVisible"
            data-testid="filterContainer"
            persistent
            no-click-animation
            scrollable
            :fullscreen="layoutStore.isMobile"
            :width="layoutStore.isMobile ? 'auto' : 550"
        >
            <v-card>
                <v-card-title class="px-0">
                    <v-toolbar title="Filter" density="compact" color="white">
                        <HgIconButtonComponent
                            icon="close"
                            @click="hideDialog"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-divider />
                <v-card-text class="pa-4">
                    <SectionHeaderComponent title="Search" />
                    <v-text-field
                        v-model="keywordInputText"
                        data-testid="filterTextInput"
                        maxlength="50"
                        placeholder="i.e. Medication Name"
                        prepend-inner-icon="search"
                    />
                    <SectionHeaderComponent title="Type" class="mb-3" />
                    <div class="mb-3">
                        <v-chip-group
                            v-model="selectedEntryTypes"
                            column
                            multiple
                        >
                            <!-- loop through selectedEntryTypes and display -->
                            <v-chip
                                v-for="(entryType, index) in enabledEntryTypes"
                                :key="index"
                                :data-testid="`${entryType.type}-filter`"
                                :name="entryType.type + '-filter'"
                                :value="entryType.type"
                                hide-details
                                :variant="
                                    isSelected(entryType.type)
                                        ? 'elevated'
                                        : 'outlined'
                                "
                                color="primary"
                            >
                                <p>
                                    {{ entryType.display }}
                                    <span
                                        :data-testid="`${entryType.type}Count`"
                                    >
                                        ({{
                                            getFormattedFilterCount(
                                                entryType.type
                                            )
                                        }})
                                    </span>
                                </p>
                            </v-chip>
                        </v-chip-group>
                    </div>
                    <SectionHeaderComponent title="Dates" />
                    <v-row>
                        <v-col cols="12" sm="6">
                            <HgDatePickerComponent
                                id="start-date"
                                v-model="startDate"
                                data-testid="filterStartDateInput"
                                label="Start Date"
                                hide-details
                                @validity-updated="
                                    isFilterStartDateValidDate = $event
                                "
                            />
                        </v-col>
                        <v-col cols="12" sm="6">
                            <HgDatePickerComponent
                                id="end-date"
                                v-model="endDate"
                                data-testid="filterEndDateInput"
                                label="End Date"
                                hide-details
                                @validity-updated="
                                    isFilterEndDateValidDate = $event
                                "
                            />
                        </v-col>
                    </v-row>
                </v-card-text>
                <v-card-actions class="pa-4">
                    <v-spacer />
                    <HgButtonComponent
                        data-testid="btnFilterCancel"
                        variant="secondary"
                        text="Cancel"
                        @click="hideDialog"
                    />
                    <HgButtonComponent
                        data-testid="btnFilterApply"
                        text="Apply"
                        @click="apply"
                    />
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>
