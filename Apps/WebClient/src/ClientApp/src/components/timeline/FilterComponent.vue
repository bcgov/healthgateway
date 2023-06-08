<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faBars,
    faChevronDown,
    faFilter,
} from "@fortawesome/free-solid-svg-icons";
import { computed, onMounted, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { PatientDataType } from "@/models/patientDataResponse";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";

library.add(faBars, faChevronDown, faFilter);

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

const store = useStore();

const isMobileView = computed<boolean>(() => store.getters["isMobile"]);
const isSidebarOpen = computed<boolean>(
    () => store.getters["navbar/isSidebarOpen"]
);
const activeFilter = computed<TimelineFilter>(
    () => store.getters["timeline/filter"]
);
const activeEntryTypes = computed<Set<EntryType>>(
    () => store.getters["timeline/selectedEntryTypes"]
);

const notesCount = computed<number>(() => store.getters["note/notesCount"]);
const clinicalDocumentsCount = computed<(hdid: string) => number>(
    () => store.getters["clinicalDocument/clinicalDocumentsCount"]
);
const covid19LaboratoryOrdersCount = computed<(hdid: string) => number>(
    () => store.getters["laboratory/covid19LaboratoryOrdersCount"]
);
const healthVisitsCount = computed<(hdid: string) => number>(
    () => store.getters["encounter/healthVisitsCount"]
);
const hospitalVisitsCount = computed<(hdid: string) => number>(
    () => store.getters["encounter/hospitalVisitsCount"]
);
const immunizationsCount = computed<(hdid: string) => number>(
    () => store.getters["immunization/immunizationsCount"]
);
const laboratoryOrdersCount = computed<(hdid: string) => number>(
    () => store.getters["laboratory/laboratoryOrdersCount"]
);
const medicationsCount = computed<(hdid: string) => number>(
    () => store.getters["medication/medicationsCount"]
);
const specialAuthorityRequestsCount = computed<(hdid: string) => number>(
    () => store.getters["medication/specialAuthorityRequestsCount"]
);
const patientDataCount = computed<
    (hdid: string, patientDataTypes: PatientDataType[]) => number
>(() => store.getters["patientData/patientDataCount"]);

const isModalVisible = ref(false);
const isMenuVisible = ref(false);
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

const hasFilterSelected = computed(() => activeFilter.value.hasActiveFilter());

function setFilter(filterBuilder: TimelineFilterBuilder): void {
    store.dispatch("timeline/setFilter", filterBuilder);
}

function toggleMenu(): void {
    isMenuVisible.value = !isMenuVisible.value;
}

function toggleMobileView(): void {
    isModalVisible.value = !isModalVisible.value;
}

function apply(): void {
    const builder = TimelineFilterBuilder.create()
        .withKeyword(keywordInputText.value)
        .withStartDate(startDate.value)
        .withEndDate(endDate.value)
        .withEntryTypes(selectedEntryTypes.value);

    setFilter(builder);
    closeMenu();
}

function cancel(): void {
    syncWithFilter();
    closeMenu();
}

function closeMenu(): void {
    isMenuVisible.value = false;
    isModalVisible.value = false;
}

function getFilterCount(entryType: EntryType): number | undefined {
    switch (entryType) {
        case EntryType.Note:
            return notesCount.value;
        case EntryType.ClinicalDocument:
            return clinicalDocumentsCount.value(props.hdid);
        case EntryType.Covid19TestResult:
            return covid19LaboratoryOrdersCount.value(props.hdid);
        case EntryType.HealthVisit:
            return healthVisitsCount.value(props.hdid);
        case EntryType.HospitalVisit:
            return hospitalVisitsCount.value(props.hdid);
        case EntryType.Immunization:
            return immunizationsCount.value(props.hdid);
        case EntryType.LabResult:
            return laboratoryOrdersCount.value(props.hdid);
        case EntryType.Medication:
            return medicationsCount.value(props.hdid);
        case EntryType.SpecialAuthorityRequest:
            return specialAuthorityRequestsCount.value(props.hdid);
        case EntryType.DiagnosticImaging:
            return patientDataCount.value(props.hdid, [
                PatientDataType.DiagnosticImaging,
            ]);
        default:
            return undefined;
    }
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

function syncWithFilter(): void {
    keywordInputText.value = activeFilter.value.keyword;
    startDate.value = activeFilter.value.startDate;
    endDate.value = activeFilter.value.endDate;
    selectedEntryTypes.value = [...activeEntryTypes.value];
}

watch(isMobileView, () => {
    isModalVisible.value = false;
});

watch(isSidebarOpen, () => {
    isModalVisible.value = false;
});

watch(
    activeFilter,
    () => {
        syncWithFilter();
    },
    { deep: true }
);

onMounted(() => {
    syncWithFilter();
});
</script>

<template>
    <div class="filters-wrapper">
        <div id="filter-button-container">
            <hg-button
                id="filterBtn"
                data-testid="filterDropdown"
                :class="{ selected: hasFilterSelected }"
                class="d-none d-md-block"
                variant="secondary"
                tabindex="0"
                @click="toggleMenu"
            >
                <hg-icon
                    icon="filter"
                    size="medium"
                    square
                    aria-hidden="true"
                    class="mr-2"
                />
                <span>Filter</span>
                <hg-icon
                    icon="chevron-down"
                    size="small"
                    aria-hidden="true"
                    class="ml-2"
                />
            </hg-button>
            <hg-button
                data-testid="mobileFilterDropdown"
                class="d-inline d-md-none"
                :class="{ selected: hasFilterSelected }"
                variant="secondary"
                @click.stop="toggleMobileView"
            >
                <hg-icon
                    icon="filter"
                    size="medium"
                    square
                    aria-hidden="true"
                />
            </hg-button>
        </div>
        <b-popover
            target="filterBtn"
            :show.sync="isMenuVisible"
            triggers="click"
            text="Filter"
            class="w-100"
            data-testid="filterContainer"
            placement="bottom"
            fallback-placement="clockwise"
            boundary="viewport"
        >
            <div class="px-1">
                <b-row class="mt-2">
                    <b-col><strong>Keywords</strong></b-col>
                </b-row>
                <div class="mt-1 has-filter">
                    <hg-icon
                        icon="search"
                        size="medium"
                        class="form-control-feedback"
                    />
                    <b-form-input
                        v-model="keywordInputText"
                        data-testid="filterTextInput"
                        type="text"
                        placeholder=""
                        maxlength="50"
                        debounce="250"
                    />
                </div>
                <b-row class="mt-2 mb-1">
                    <b-col><strong>Type</strong></b-col>
                </b-row>
                <b-form-checkbox-group v-model="selectedEntryTypes">
                    <b-row
                        v-for="(entryType, index) in enabledEntryTypes"
                        :key="index"
                    >
                        <b-col cols="8" align-self="start">
                            <b-form-checkbox
                                :id="entryType.type + '-filter'"
                                :data-testid="`${entryType.type}-filter`"
                                :name="entryType.type + '-filter'"
                                :value="entryType.type"
                            >
                                {{ entryType.display }}
                            </b-form-checkbox>
                        </b-col>
                        <b-col
                            cols="4"
                            align-self="end"
                            class="text-right"
                            :data-testid="`${entryType.type}Count`"
                        >
                            ({{ getFormattedFilterCount(entryType.type) }})
                        </b-col>
                    </b-row>
                </b-form-checkbox-group>
                <b-row class="mt-2">
                    <b-col><strong>Dates</strong></b-col>
                </b-row>
                <b-row class="mt-1">
                    <b-col>
                        <DatePickerComponent
                            id="start-date"
                            :value="startDate"
                            data-testid="filterStartDateInput"
                            @is-date-valid="isFilterStartDateValidDate = $event"
                            @update:value="(value) => (startDate = value)"
                        />
                    </b-col>
                </b-row>
                <b-row class="mt-1">
                    <b-col>
                        <DatePickerComponent
                            id="end-date"
                            :value="endDate"
                            data-testid="filterEndDateInput"
                            @is-date-valid="isFilterEndDateValidDate = $event"
                            @update:value="(value) => (endDate = value)"
                        />
                    </b-col>
                </b-row>
                <b-row class="mt-3 mb-2" no-gutters align-h="end">
                    <b-col cols="auto">
                        <hg-button
                            data-testid="btnFilterCancel"
                            class="px-2"
                            variant="secondary"
                            @click.stop="cancel"
                        >
                            Cancel
                        </hg-button>
                    </b-col>
                    <b-col cols="auto" class="ml-2">
                        <hg-button
                            data-testid="btnFilterApply"
                            class="btn-primary px-2"
                            variant="primary"
                            :disabled="
                                !isFilterStartDateValidDate ||
                                !isFilterEndDateValidDate
                            "
                            @click.stop="apply"
                        >
                            Apply
                        </hg-button>
                    </b-col>
                </b-row>
            </div>
        </b-popover>
        <b-modal
            id="generic-message"
            v-model="isModalVisible"
            title="Options2"
            content-class="filters-mobile-content"
            header-class="m-0 py-3"
            header-bg-variant="outline"
            :no-close-on-backdrop="true"
            :hide-header-close="true"
            :hide-footer="true"
            no-fade
        >
            <template #modal-header>
                <h5 class="ml-auto mr-auto my-0">Filter</h5>
            </template>

            <div class="filter-section mb-3">
                <div class="mb-1"><strong>Keywords</strong></div>
                <div class="has-filter">
                    <hg-icon
                        icon="search"
                        size="medium"
                        class="form-control-feedback"
                    />
                    <b-form-input
                        v-model="keywordInputText"
                        data-testid="filterTextInput"
                        type="text"
                        placeholder=""
                        maxlength="50"
                        debounce="250"
                    />
                </div>
            </div>

            <div class="filter-section mb-3">
                <div class="mb-1"><strong>Type</strong></div>
                <b-form-checkbox-group v-model="selectedEntryTypes">
                    <b-row
                        v-for="(filter, index) in enabledEntryTypes"
                        :key="index"
                    >
                        <b-col cols="8" align-self="start">
                            <b-form-checkbox
                                :id="filter.type + '-filter'"
                                :data-testid="`${filter.type}-filter`"
                                :name="filter.type + '-filter'"
                                :value="filter.type"
                            >
                                {{ filter.display }}
                            </b-form-checkbox>
                        </b-col>
                        <b-col cols="4" align-self="end" class="text-right">
                            ({{ getFormattedFilterCount(filter.type) }})
                        </b-col>
                    </b-row>
                </b-form-checkbox-group>
            </div>

            <div class="filter-section mb-3">
                <div class="mb-1"><strong>Dates</strong></div>
                <b-row class="mb-1">
                    <b-col>
                        <DatePickerComponent
                            id="start-date"
                            :value="startDate"
                            data-testid="filterStartDateInput"
                            @is-date-valid="isFilterStartDateValidDate = $event"
                            @update:value="(value) => (startDate = value)"
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <DatePickerComponent
                            id="end-date"
                            :value="endDate"
                            data-testid="filterEndDateInput"
                            @is-date-valid="isFilterEndDateValidDate = $event"
                            @update:value="(value) => (endDate = value)"
                        />
                    </b-col>
                </b-row>
            </div>
            <b-row no-gutters align-h="end">
                <b-col cols="auto">
                    <hg-button
                        data-testid="btnFilterCancel"
                        class="px-2"
                        variant="secondary"
                        @click.stop="cancel"
                    >
                        Cancel
                    </hg-button>
                </b-col>
                <b-col cols="auto" class="ml-2">
                    <hg-button
                        data-testid="btnFilterApply"
                        class="px-2"
                        variant="primary"
                        :disabled="
                            !isFilterStartDateValidDate ||
                            !isFilterEndDateValidDate
                        "
                        @click.stop="apply"
                    >
                        Apply
                    </hg-button>
                </b-col>
            </b-row>
        </b-modal>
    </div>
</template>

<style lang="scss" scoped>
.filters-wrapper {
    z-index: 3;
}
</style>
<style lang="scss" scoped>
@use "sass:math";
@import "@/assets/scss/_variables.scss";

.filter-section {
    div[class^="col"],
    div[class*=" col"] {
        padding: 0px;
        margin: 0px;
    }

    div[class^="row"],
    div[class*=" row"] {
        padding: 0px;
        margin: 0px;
    }
}

.filters-mobile-content {
    position: fixed;
    top: auto;
    right: auto;
    border: 0px;
    left: 0;
    bottom: 0;
    border-radius: 0px;

    .btn-mobile {
        color: #494949;
        border: none;
    }

    .btn-close {
        font-size: 1.5em;
    }
}

.has-filter {
    $icon-size: 1rem;
    $icon-size-padded: 2.375rem;
    $icon-padding: math.div($icon-size-padded - $icon-size, 2);

    .form-control {
        padding-left: $icon-size-padded;
    }

    .form-control-feedback {
        position: absolute;
        z-index: 5;
        display: block;
        text-align: center;
        pointer-events: none;
        color: #aaa;
        padding: $icon-padding;
    }
}

.view-selector {
    min-width: 170px;

    .list-view-btn {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }

    .month-view-btn {
        border-radius: 0px 5px 5px 0px;
    }
}
</style>
