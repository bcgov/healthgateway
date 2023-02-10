<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faBars,
    faChevronDown,
    faFilter,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import Component from "vue-class-component";
import { Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/DatePickerComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import TimelineFilter, { TimelineFilterBuilder } from "@/models/timelineFilter";
import User from "@/models/user";
import { UserPreference } from "@/models/userPreference";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(faBars, faChevronDown, faFilter);

// Entry filter model
interface EntryTypeFilter {
    type: EntryType;
    display: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        DatePickerComponent,
    },
};

@Component(options)
export default class FilterComponent extends Vue {
    @Action("setFilter", { namespace: "timeline" })
    setFilter!: (filterBuilder: TimelineFilterBuilder) => void;

    @Action("setUserPreference", { namespace: "user" })
    setUserPreference!: (params: { preference: UserPreference }) => void;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("isMobile")
    isMobileView!: boolean;

    @Getter("isSidebarOpen", { namespace: "navbar" })
    isSidebarOpen!: boolean;

    @Getter("medicationsCount", { namespace: "medication" })
    medicationsCount!: (hdid: string) => number;

    @Getter("specialAuthorityRequestsCount", { namespace: "medication" })
    specialAuthorityRequestsCount!: (hdid: string) => number;

    @Getter("immunizationsCount", { namespace: "immunization" })
    immunizationsCount!: (hdid: string) => number;

    @Getter("covid19LaboratoryOrdersCount", { namespace: "laboratory" })
    covid19LaboratoryOrdersCount!: (hdid: string) => number;

    @Getter("laboratoryOrdersCount", { namespace: "laboratory" })
    laboratoryOrdersCount!: (hdid: string) => number;

    @Getter("healthVisitsCount", { namespace: "encounter" })
    healthVisitsCount!: (hdid: string) => number;

    @Getter("hospitalVisitsCount", { namespace: "encounter" })
    hospitalVisitsCount!: (hdid: string) => number;

    @Getter("notesCount", { namespace: "note" })
    notesCount!: number;

    @Getter("clinicalDocumentsCount", { namespace: "clinicalDocument" })
    clinicalDocumentsCount!: (hdid: string) => number;

    @Getter("filter", { namespace: "timeline" })
    activeFilter!: TimelineFilter;

    @Getter("entryTypes", { namespace: "timeline" })
    activeEntryTypes!: Set<EntryType>;

    @Getter("user", { namespace: "user" })
    user!: User;

    private logger!: ILogger;
    private isModalVisible = false;
    private isMenuVisible = false;
    private isFilterTutorialHidden = false;
    private isFilterStartDateValidDate = true;
    private isFilterEndDateValidDate = true;

    private startDate = "";
    private endDate = "";
    private selectedEntryTypes: EntryType[] = [];
    private keywordInputText = "";

    private get enabledEntryTypes(): EntryTypeFilter[] {
        return [...entryTypeMap.values()]
            .filter((details) => this.config.modules[details.type])
            .map((details) => ({
                type: details.type,
                display: details.name,
            }));
    }

    private get hasFilterSelected(): boolean {
        return this.activeFilter.hasActiveFilter();
    }

    private get showFilterTutorial(): boolean {
        const preferenceType = UserPreferenceType.TutorialTimelineFilter;
        return (
            this.user.preferences[preferenceType]?.value === "true" &&
            !this.isFilterTutorialHidden
        );
    }

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.syncWithFilter();
    }

    @Watch("isMobileView")
    private onIsMobileView(): void {
        this.isModalVisible = false;
    }

    @Watch("isSidebarOpen")
    private onIsSidebarOpen(): void {
        this.isModalVisible = false;
    }

    @Watch("activeFilter", { deep: true })
    private syncWithFilter(): void {
        this.keywordInputText = this.activeFilter.keyword;
        this.startDate = this.activeFilter.startDate;
        this.endDate = this.activeFilter.endDate;
        this.selectedEntryTypes = Array.from(this.activeEntryTypes);
    }

    private toggleMenu(): void {
        this.isMenuVisible = !this.isMenuVisible;
    }

    private toggleMobileView(): void {
        this.isModalVisible = !this.isModalVisible;
    }

    private apply(): void {
        let builder = TimelineFilterBuilder.create()
            .withKeyword(this.keywordInputText)
            .withStartDate(this.startDate)
            .withEndDate(this.endDate)
            .withEntryTypes(this.selectedEntryTypes);

        this.setFilter(builder);

        this.closeMenu();
    }

    private cancel(): void {
        this.syncWithFilter();
        this.closeMenu();
    }

    private closeMenu(): void {
        this.isMenuVisible = false;
        this.isModalVisible = false;
    }

    private getFilterCount(entryType: EntryType): number | undefined {
        switch (entryType) {
            case EntryType.Immunization:
                return this.immunizationsCount(this.user.hdid);
            case EntryType.Medication:
                return this.medicationsCount(this.user.hdid);
            case EntryType.LaboratoryOrder:
                return this.laboratoryOrdersCount(this.user.hdid);
            case EntryType.Covid19LaboratoryOrder:
                return this.covid19LaboratoryOrdersCount(this.user.hdid);
            case EntryType.Encounter:
                return this.healthVisitsCount(this.user.hdid);
            case EntryType.HospitalVisit:
                return this.hospitalVisitsCount(this.user.hdid);
            case EntryType.Note:
                return this.notesCount;
            case EntryType.MedicationRequest:
                return this.specialAuthorityRequestsCount(this.user.hdid);
            case EntryType.ClinicalDocument:
                return this.clinicalDocumentsCount(this.user.hdid);
            default:
                return undefined;
        }
    }

    private formatFilterCount(entryType: EntryType): string {
        const num = this.getFilterCount(entryType);

        if (num === undefined) {
            return "";
        }

        return Math.abs(num) > 999
            ? parseFloat(
                  ((Math.round(num / 100) * 100) / 1000).toFixed(1)
              ).toString() + "K"
            : num.toString();
    }

    private dismissFilterTutorial(): void {
        this.logger.debug("Dismissing timeline filter tutorial");
        this.isFilterTutorialHidden = true;

        const preference = {
            ...this.user.preferences[UserPreferenceType.TutorialTimelineFilter],
            value: "false",
        };
        this.setUserPreference({ preference });
    }
}
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
            triggers="manual"
            :show="showFilterTutorial"
            target="filter-button-container"
            placement="bottom"
            boundary="viewport"
        >
            <div>
                <hg-button
                    class="float-right text-dark p-0 ml-2"
                    variant="icon"
                    @click="dismissFilterTutorial()"
                    >×</hg-button
                >
            </div>
            <div data-testid="filter-tutorial-popover">
                Filter by health record type, date or keyword to find what you
                need.
            </div>
        </b-popover>
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
                        ({{ formatFilterCount(entryType.type) }})
                    </b-col>
                </b-row>
                <b-row class="mt-2">
                    <b-col><strong>Dates</strong></b-col>
                </b-row>
                <b-row class="mt-1">
                    <b-col>
                        <DatePickerComponent
                            id="start-date"
                            v-model="startDate"
                            data-testid="filterStartDateInput"
                            @is-date-valid="isFilterStartDateValidDate = $event"
                        />
                    </b-col>
                </b-row>
                <b-row class="mt-1">
                    <b-col>
                        <DatePickerComponent
                            id="end-date"
                            v-model="endDate"
                            data-testid="filterEndDateInput"
                            @is-date-valid="isFilterEndDateValidDate = $event"
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
                        ({{ formatFilterCount(filter.type) }})
                    </b-col>
                </b-row>
            </div>

            <div class="filter-section mb-3">
                <div class="mb-1"><strong>Dates</strong></div>
                <b-row class="mb-1">
                    <b-col>
                        <DatePickerComponent
                            id="start-date"
                            v-model="startDate"
                            data-testid="filterStartDateInput"
                            @is-date-valid="isFilterStartDateValidDate = $event"
                        />
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <DatePickerComponent
                            id="end-date"
                            v-model="endDate"
                            data-testid="filterEndDateInput"
                            @is-date-valid="isFilterEndDateValidDate = $event"
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
