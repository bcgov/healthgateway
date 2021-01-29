<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSlidersH } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import Component from "vue-class-component";
import { Emit, Prop, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import { EntryType } from "@/models/timelineEntry";
import TimelineFilter, { EntryTypeFilter } from "@/models/timelineFilter";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
library.add(faSlidersH);

@Component({
    components: {
        DatePickerComponent,
    },
})
export default class FilterComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;
    @Getter("isOpen", { namespace: "sidebar" }) isSidebarOpen!: boolean;

    @Prop() private filter!: TimelineFilter;

    private logger!: ILogger;
    private eventBus = EventBus;
    private isVisible = false;
    private windowWidth = 0;

    private selectedEntryTypes: EntryType[] = [];

    private get isMobileView(): boolean {
        return this.windowWidth < 576;
    }

    private get enabledEntryTypes(): EntryTypeFilter[] {
        return this.filter.entryTypes.filter(
            (filter: EntryTypeFilter) => filter.isEnabled
        );
    }

    private get activeFilterCount(): number {
        return this.filter.getActiveFilterCount();
    }

    private get hasFilterSelected(): boolean {
        return this.filter.hasActiveFilter();
    }

    @Watch("selectedEntryTypes")
    private onSelectedEntryTypesChanged() {
        this.filter.entryTypes.forEach((et: EntryTypeFilter) => {
            et.isSelected = this.selectedEntryTypes.some(
                (set) => set == et.type
            );
        });
    }

    @Watch("isMobileView")
    private onIsMobileView() {
        this.isVisible = false;
    }

    @Watch("isSidebarOpen")
    private onIsSidebarOpen() {
        this.isVisible = false;
    }

    @Watch("filter", { deep: true })
    private onFilterUpdate() {
        this.filtersChanged();
    }

    @Emit()
    private filtersChanged(): TimelineFilter {
        return this.filter;
    }

    private toggleListView() {
        this.filter.isListView = true;
        window.location.hash = "linear";
    }
    private toggleMonthView() {
        this.filter.isListView = false;
        window.location.hash = "calendar";
    }

    private created() {
        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.eventBus.$on(
            EventMessageName.SelectedFilter,
            (filterType: EntryType) => {
                this.onExternalFilterSelection(filterType);
            }
        );
        this.eventBus.$on(EventMessageName.TimelineEntryAdded, () => {
            this.onEntryAdded();
        });
        this.eventBus.$on(EventMessageName.CalendarDateEventClick, () => {
            this.filter.isListView = true;
        });
    }

    private destroyed() {
        window.removeEventListener("handleResize", this.handleResize);
    }

    private handleResize() {
        this.windowWidth = window.innerWidth;
    }

    private toggleMobileView() {
        this.isVisible = !this.isVisible;
    }

    private clearFilters(): void {
        this.selectedEntryTypes = [];
        this.filter.clear();
    }

    private onExternalFilterSelection(filterType: EntryType) {
        this.clearFilters();
        this.selectedEntryTypes.push(filterType);
    }

    private formatFilterCount(num: number): string {
        return Math.abs(num) > 999
            ? parseFloat(
                  ((Math.round(num / 100) * 100) / 1000).toFixed(1)
              ).toString() + "K"
            : num.toString();
    }

    private onEntryAdded() {
        this.clearFilters();
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
            >
                Filter
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
                triggers="click"
                text="Filter"
                class="w-100"
                data-testid="filterContainer"
                placement="bottom"
                no-flip
                menu-class="z-index-large w-100"
            >
                <b-row class="px-4">
                    <b-col><strong>Type</strong> </b-col>
                    <b-col class="col-auto">
                        <b-button
                            variant="link"
                            class="p-0 m-0 btn-mobile"
                            @click="clearFilters()"
                        >
                            Clear
                        </b-button>
                    </b-col>
                </b-row>
                <div class="px-4">
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
                        <b-col
                            cols="4"
                            align-self="end"
                            class="text-right"
                            :data-testid="`${filter.type}Count`"
                        >
                            ({{ formatFilterCount(filter.numEntries) }})
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
                                v-model="filter.startDate"
                                data-testid="filterStartDateInput"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mt-1">
                        <b-col>
                            <DatePickerComponent
                                id="end-date"
                                v-model="filter.endDate"
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
                                data-testid="monthViewToggle"
                                class="month-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: !filter.isListView }"
                                @click.stop="toggleMonthView"
                            >
                                Date
                            </b-btn>
                        </b-col>
                        <b-col cols="auto" class="pl-0">
                            <b-btn
                                data-testid="listViewToggle"
                                class="list-view-btn btn-outline-primary px-2 m-0"
                                :class="{ active: filter.isListView }"
                                @click.stop="toggleListView"
                            >
                                List
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
            v-model="isVisible"
            title="Filter"
            content-class="filters-mobile-content"
            header-bg-variant="outline"
            :hide-footer="true"
            no-fade
        >
            <template #modal-header="{ close }">
                <b-row class="w-100 text-center p-0 m-0">
                    <b-col class="col-3">
                        <!-- Emulate built in modal header close button action -->
                        <b-button
                            variant="link"
                            class="m-0 p-0 btn-mobile btn-close"
                            @click="close()"
                        >
                            <font-awesome-icon
                                icon="times"
                                aria-hidden="true"
                                size="1x"
                                class="m-0"
                            />
                        </b-button> </b-col
                    ><b-col class="col-6 pt-1">
                        <h5>Filter</h5>
                    </b-col>
                    <b-col class="col-3 pt-1">
                        <b-button
                            variant="link"
                            class="p-0 m-0 btn-mobile"
                            @click="clearFilters()"
                        >
                            Clear
                        </b-button>
                    </b-col>
                </b-row>
            </template>
            <b-row class="justify-content-center py-2">
                <b-col class="col-10">
                    <h5>Type</h5>
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
                </b-col>
            </b-row>
            <b-row class="justify-content-center py-2">
                <b-col class="col-10">
                    <h5>Dates</h5>
                    <b-row>
                        <b-col>
                            <DatePickerComponent
                                id="start-date"
                                v-model="filter.startDate"
                                data-testid="filterStartDateInput"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mt-1">
                        <b-col>
                            <DatePickerComponent
                                id="end-date"
                                v-model="filter.endDate"
                                data-testid="filterEndDateInput"
                            />
                        </b-col>
                    </b-row>
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
    .month-view-btn {
        border-radius: 5px 0px 0px 5px;
        border-right: 0px;
    }
    .list-view-btn {
        border-radius: 0px 5px 5px 0px;
    }
}
</style>
