<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faBars,
    faCalendarDay,
    faSlidersH,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import Component from "vue-class-component";
import { Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { StringISODate } from "@/models/dateWrapper";
import { EntryType } from "@/models/timelineEntry";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
library.add(faBars, faCalendarDay, faSlidersH);

// Entry filter model
interface EntryTypeFilter {
    type: EntryType;
    display: string;
    isEnabled: boolean;
    numEntries: number;
}

@Component({
    components: {
        DatePickerComponent,
    },
})
export default class FilterComponent extends Vue {
    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("setLinearView", { namespace: "timeline" })
    setLinearView!: (isLinearView: boolean) => void;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("isSidebarOpen", { namespace: "navbar" }) isSidebarOpen!: boolean;

    @Getter("medicationCount", { namespace: "medication" })
    medicationCount!: number;

    @Getter("immunizationCount", { namespace: "immunization" })
    immunizationCount!: number;

    @Getter("laboratoryCount", { namespace: "laboratory" })
    laboratoryCount!: number;

    @Getter("encounterCount", { namespace: "encounter" })
    encounterCount!: number;

    @Getter("noteCount", { namespace: "note" }) noteCount!: number;

    @Getter("isLinearView", { namespace: "timeline" }) isLinearView!: boolean;

    @Getter("filter", { namespace: "timeline" }) activeFilter!: TimelineFilter;

    private logger!: ILogger;
    private isModalVisible = false;
    private windowWidth = 0;
    private isMenuVisible = false;

    private isListViewToggle = true;
    private startDate: StringISODate = "";
    private endDate: StringISODate = "";
    private selectedEntryTypes: EntryType[] = [];

    private entryTypes: EntryTypeFilter[] = [];

    private get isMobileView(): boolean {
        return this.windowWidth < 576;
    }

    private get enabledEntryTypes(): EntryTypeFilter[] {
        return this.entryTypes.filter(
            (filter: EntryTypeFilter) => filter.isEnabled
        );
    }

    private get activeFilterCount(): number {
        return this.activeFilter.getActiveFilterCount();
    }

    private get hasFilterSelected(): boolean {
        return this.activeFilter.hasActiveFilter();
    }

    @Watch("isMobileView")
    private onIsMobileView() {
        this.isModalVisible = false;
    }

    @Watch("isSidebarOpen")
    private onIsSidebarOpen() {
        this.isModalVisible = false;
    }

    private created() {
        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.syncWithFilter();

        this.entryTypes = [
            {
                type: EntryType.Immunization,
                display: "Immunizations",
                isEnabled: this.config.modules[EntryType.Immunization],
                numEntries: this.immunizationCount,
            },
            {
                type: EntryType.Medication,
                display: "Medications",
                isEnabled: this.config.modules[EntryType.Medication],
                numEntries: this.medicationCount,
            },
            {
                type: EntryType.Laboratory,
                display: "COVID-19 Tests",
                isEnabled: this.config.modules[EntryType.Laboratory],
                numEntries: this.laboratoryCount,
            },
            {
                type: EntryType.Encounter,
                display: "MSP Visits",
                isEnabled: this.config.modules[EntryType.Encounter],
                numEntries: this.encounterCount,
            },
            {
                type: EntryType.Note,
                display: "My Notes",
                isEnabled: this.config.modules[EntryType.Note],
                numEntries: this.noteCount,
            },
        ];
    }

    private destroyed() {
        window.removeEventListener("handleResize", this.handleResize);
    }

    @Watch("filter", { deep: true })
    @Watch("isLinearView")
    private syncWithFilter() {
        this.startDate = this.activeFilter.startDate;
        this.endDate = this.activeFilter.endDate;
        this.selectedEntryTypes = Array.from(this.activeFilter.entryTypes);
        this.isListViewToggle = this.isLinearView;
    }

    private handleResize() {
        this.windowWidth = window.innerWidth;
    }

    private toggleMenu() {
        this.isMenuVisible = !this.isMenuVisible;
    }

    private toggleMobileView() {
        this.isModalVisible = !this.isModalVisible;
    }

    private toggleListView() {
        this.isListViewToggle = true;
    }

    private toggleMonthView() {
        this.isListViewToggle = false;
    }

    private clearOptions(): void {
        this.startDate = "";
        this.endDate = "";
        this.selectedEntryTypes = [];
        this.isListViewToggle = true;
    }

    private apply() {
        let builder = TimelineFilterBuilder.create()
            .withStartDate(this.startDate)
            .withEndDate(this.endDate)
            .withEntryTypes(this.selectedEntryTypes);

        this.setFilter(builder);
        this.setLinearView(this.isListViewToggle);

        this.closeMenu();
    }

    private cancel() {
        this.syncWithFilter();
        this.closeMenu();
    }

    private closeMenu() {
        this.isMenuVisible = false;
        this.isModalVisible = false;
    }

    private formatFilterCount(num: number): string {
        return Math.abs(num) > 999
            ? parseFloat(
                  ((Math.round(num / 100) * 100) / 1000).toFixed(1)
              ).toString() + "K"
            : num.toString();
    }
}
</script>

<template>
    <div class="filters-wrapper">
        <div class="filters-width d-none d-sm-block">
            <b-button
                id="filterBtn"
                class="w-100"
                :class="{ 'filter-selected': hasFilterSelected }"
                data-testid="filterDropdown"
                variant="outline-primary"
                tabindex="0"
                @click="toggleMenu"
            >
                Options
                <b-badge
                    v-show="hasFilterSelected"
                    variant="light"
                    class="badge-style"
                >
                    {{ activeFilterCount }}
                    <span class="sr-only">filters applied</span>
                </b-badge>
                <font-awesome-icon
                    icon="chevron-down"
                    size="xs"
                    aria-hidden="true"
                    class="ml-1"
                />
            </b-button>
            <b-popover
                target="filterBtn"
                :show.sync="isMenuVisible"
                triggers="click"
                text="Filter"
                class="w-100"
                data-testid="filterContainer"
                placement="bottom"
                fallback-placement="clockwise"
                menu-class="z-index-large w-100"
            >
                <b-row class="px-4">
                    <b-col><strong>Filter</strong> </b-col>
                    <b-col class="col-auto">
                        <b-button
                            variant="link"
                            class="btn-mobile"
                            @click="clearOptions()"
                        >
                            Clear
                        </b-button>
                    </b-col>
                </b-row>

                <div class="px-4">
                    <b-row class="mt-2">
                        <b-col><strong>Type</strong> </b-col>
                        <b-col class="col-auto"></b-col>
                    </b-row>
                    <b-row
                        v-for="(entryType, index) in enabledEntryTypes"
                        :key="index"
                    >
                        <b-col cols="8" align-self="start">
                            <b-form-checkbox
                                :id="entryType.type + '-filter'"
                                v-model="selectedEntryTypes"
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
                            ({{ formatFilterCount(entryType.numEntries) }})
                        </b-col>
                    </b-row>
                    <b-row class="mt-2">
                        <b-col><strong>Dates</strong> </b-col>
                        <b-col class="col-auto"></b-col>
                    </b-row>
                    <b-row class="mt-1">
                        <b-col>
                            <DatePickerComponent
                                id="start-date"
                                v-model="startDate"
                                data-testid="filterStartDateInput"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mt-1">
                        <b-col>
                            <DatePickerComponent
                                id="end-date"
                                v-model="endDate"
                                data-testid="filterEndDateInput"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mt-2">
                        <b-col><strong>View</strong> </b-col>
                        <b-col class="col-auto"></b-col>
                    </b-row>

                    <b-row class="view-selector mt-1">
                        <b-col cols="auto" class="pr-0">
                            <b-btn
                                data-testid="listViewToggle"
                                class="list-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: isListViewToggle }"
                                @click.stop="toggleListView"
                            >
                                <font-awesome-icon
                                    icon="bars"
                                    size="1x"
                                ></font-awesome-icon>
                            </b-btn>
                        </b-col>
                        <b-col cols="auto" class="pl-0">
                            <b-btn
                                data-testid="monthViewToggle"
                                class="month-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: !isListViewToggle }"
                                @click.stop="toggleMonthView"
                            >
                                <font-awesome-icon
                                    icon="calendar-day"
                                    size="1x"
                                ></font-awesome-icon>
                            </b-btn>
                        </b-col>
                    </b-row>

                    <b-row class="mt-1" align-h="end">
                        <b-col cols="auto" class="pr-0">
                            <b-btn
                                data-testid="btnFilterCancel"
                                class="btn-outline-primary px-2 m-0"
                                @click.stop="cancel"
                            >
                                Cancel
                            </b-btn>
                        </b-col>
                        <b-col cols="auto" class="pl-2">
                            <b-btn
                                data-testid="btnFilterApply"
                                class="btn-primary px-2 m-0"
                                @click.stop="apply"
                            >
                                Apply
                            </b-btn>
                        </b-col>
                    </b-row>
                </div>
            </b-popover>
        </div>

        <!-- Mobile view specific modal-->
        <b-button
            data-testid="mobileFilterDropdown"
            class="d-d-sm-inline d-sm-none"
            :class="{ 'filter-selected': hasFilterSelected }"
            variant="outline-primary"
            @click.stop="toggleMobileView"
        >
            <font-awesome-icon icon="sliders-h" aria-hidden="true" size="1x" />
        </b-button>
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
                <h5 class="ml-auto mr-auto my-0">Options</h5>
            </template>
            <div class="filter-section">
                <b-row class="w-100 text-center" align-h="between">
                    <b-col cols="auto"><h5>Filter</h5> </b-col>
                    <b-col class="col-auto">
                        <b-button
                            variant="link"
                            class="btn-mobile"
                            @click="clearOptions()"
                        >
                            Clear
                        </b-button>
                    </b-col>
                </b-row>
            </div>

            <div class="filter-section mb-3">
                <strong>Type</strong>
                <b-row
                    v-for="(filter, index) in enabledEntryTypes"
                    :key="index"
                >
                    <b-col cols="8" align-self="start">
                        <b-form-checkbox
                            :id="filter.type + '-filter'"
                            v-model="selectedEntryTypes"
                            :data-testid="`${filter.type}-filter`"
                            :name="filter.type + '-filter'"
                            :value="filter.type"
                        >
                            {{ filter.display }}
                        </b-form-checkbox>
                    </b-col>
                    <b-col cols="4" align-self="end" class="text-right">
                        ({{ formatFilterCount(filter.numEntries) }})
                    </b-col>
                </b-row>
            </div>

            <div class="filter-section mb-3">
                <strong>Dates</strong>
                <b-row class="mb-1">
                    <b-col>
                        <DatePickerComponent
                            id="start-date"
                            v-model="startDate"
                            data-testid="filterStartDateInput"
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <DatePickerComponent
                            id="end-date"
                            v-model="endDate"
                            data-testid="filterEndDateInput"
                        />
                    </b-col>
                </b-row>
            </div>

            <div class="filter-section mb-3">
                <strong>View Type</strong>
                <b-row class="view-selector">
                    <b-col cols="auto" class="pr-0">
                        <b-btn
                            data-testid="listViewToggle"
                            class="list-view-btn btn-outline-primary px-2 m-0"
                            :class="{ active: isListViewToggle }"
                            @click.stop="toggleListView"
                        >
                            <font-awesome-icon
                                icon="bars"
                                size="1x"
                            ></font-awesome-icon>
                        </b-btn>
                    </b-col>
                    <b-col cols="auto" class="pl-0">
                        <b-btn
                            data-testid="monthViewToggle"
                            class="month-view-btn btn-outline-primary px-2 m-0"
                            :class="{ active: !isListViewToggle }"
                            @click.stop="toggleMonthView"
                        >
                            <font-awesome-icon
                                icon="calendar-day"
                                size="1x"
                            ></font-awesome-icon>
                        </b-btn>
                    </b-col>
                </b-row>
            </div>

            <b-row class="mt-1" align-h="end">
                <b-col cols="auto" class="pr-0">
                    <b-btn
                        data-testid="btnFilterCancel"
                        class="btn-outline-primary px-2 m-0"
                        @click.stop="cancel"
                    >
                        Cancel
                    </b-btn>
                </b-col>
                <b-col cols="auto" class="pl-2">
                    <b-btn
                        data-testid="btnFilterApply"
                        class="btn-primary px-2 m-0"
                        @click.stop="apply"
                    >
                        Apply
                    </b-btn>
                </b-col>
            </b-row>
        </b-modal>
    </div>
</template>

<style lang="scss" scoped>
.filters-wrapper {
    z-index: 3;
}
.filters-width {
    width: 225px;
}
</style>
<style lang="scss">
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

.filters-wrapper {
    .filter-selected {
        border-color: $aquaBlue;
        background-color: $aquaBlue;
        color: white;
    }
}

.view-selector {
    min-width: 170px;
    .btn-outline-primary {
        font-size: 1em;
        background-color: white;
    }
    .btn-outline-primary:focus {
        color: white;
        background-color: $primary;
    }
    .btn-outline-primary:hover {
        color: white;
        background-color: $primary;
    }
    .list-view-btn {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .month-view-btn {
        border-radius: 0px 5px 5px 0px;
    }
}

.btn-primary {
    font-size: 1em;
    color: white;
    background-color: $primary;
}
.btn-primary:focus {
    color: $primary;
    background-color: white;
}
.btn-primary:hover {
    color: $primary;
    background-color: white;
}

.btn-outline-primary {
    font-size: 1em;
    background-color: white;
}
.btn-outline-primary:focus {
    color: white;
    background-color: $primary;
}
.btn-outline-primary:hover {
    color: white;
    background-color: $primary;
}
</style>
