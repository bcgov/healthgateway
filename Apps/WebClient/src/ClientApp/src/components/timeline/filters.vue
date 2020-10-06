<script lang="ts">
import Vue from "vue";
import Component from "vue-class-component";
import { Getter } from "vuex-class";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSlidersH } from "@fortawesome/free-solid-svg-icons";
import { Emit, Prop, Watch } from "vue-property-decorator";
import { ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
library.add(faSlidersH);

interface Filter {
    name: string;
    value: string;
    display: string;
    isEnabled: boolean;
    numEntries: number;
}

@Component
export default class FilterComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;
    @Getter("isOpen", { namespace: "sidebar" }) isSidebarOpen!: boolean;

    @Prop() private medicationCount!: number;
    @Prop() private immunizationCount!: number;
    @Prop() private encounterCount!: number;
    @Prop() private laboratoryCount!: number;
    @Prop() private noteCount!: number;
    @Prop() private isListView!: boolean;

    private logger!: ILogger;
    private eventBus = EventBus;
    private isVisible = false;
    private selectedFilters: string[] = [];
    private windowWidth = 0;
    private showSlider = true;
    private steps = [25, 50, 100, 500];
    private stepIndex = 0;

    private filters: Filter[] = [
        {
            name: "immunization",
            value: "Immunization",
            display: "Immunizations",
            isEnabled: false,
            numEntries: this.immunizationCount,
        },
        {
            name: "medication",
            value: "Medication",
            display: "Medications",
            isEnabled: false,
            numEntries: this.medicationCount,
        },

        {
            name: "laboratory",
            value: "Laboratory",
            display: "Laboratory",
            isEnabled: false,
            numEntries: this.laboratoryCount,
        },
        {
            name: "encounter",
            value: "Encounter",
            display: "MSP Visits",
            isEnabled: false,
            numEntries: this.encounterCount,
        },
        {
            name: "note",
            value: "Note",
            display: "My Notes",
            isEnabled: false,
            numEntries: this.noteCount,
        },
    ];

    private get isMobileView(): boolean {
        return this.windowWidth < 576;
    }

    @Watch("isMobileView")
    private onIsMobileView() {
        this.isVisible = false;
    }

    @Watch("isListView")
    private onIsListView(isList: boolean) {
        isList ? (this.showSlider = false) : (this.showSlider = true);
    }

    @Watch("isSidebarOpen")
    private onIsSidebarOpen() {
        this.isVisible = false;
    }

    @Watch("selectedFilters")
    private onFilterUpdate() {
        this.filtersChanged();
    }

    @Watch("noteCount")
    private noteCountUpdate(newCount: number) {
        this.filters[4].numEntries = newCount;
    }

    @Emit()
    private filtersChanged() {
        if (this.selectedFilters.length > 0) {
            return this.selectedFilters;
        } else {
            return this.getAllFilters();
        }
    }

    @Emit()
    private sliderChanged(): number {
        return this.steps[this.stepIndex];
    }

    private created() {
        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.filters[0].isEnabled = this.config.modules["Immunization"];
        this.filters[1].isEnabled = this.config.modules["Medication"];
        this.filters[2].isEnabled = this.config.modules["Laboratory"];
        this.filters[3].isEnabled = this.config.modules["Encounter"];
        this.filters[4].isEnabled = this.config.modules["Note"];
        this.selectedFilters = [];

        this.eventBus.$on(
            EventMessageName.SelectedFilter,
            (filterName: string) => {
                this.onExternalFilterSelection(filterName);
            }
        );
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
        this.selectedFilters = [];
    }

    private onExternalFilterSelection(filterName: string) {
        var externalFilter = this.filters.find((x) => x.name === filterName);
        if (externalFilter) {
            this.selectedFilters = [];
            this.selectedFilters.push(externalFilter.value);
        } else {
            this.logger.error("Invalid filter attempted to be selected");
        }
    }

    private getAllFilters(): string[] {
        return this.filters.reduce<string[]>((groups, entry) => {
            if (entry.isEnabled) {
                groups.push(entry.value);
            }
            return groups;
        }, []);
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
            <b-dropdown
                data-testid="filterDropdown"
                text="Filter"
                class="w-100"
                toggle-class="w-100"
                menu-class="z-index-large w-100"
                variant="outline-primary"
                right
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
                    <b-row v-for="(filter, index) in filters" :key="index">
                        <b-col cols="8" align-self="start">
                            <b-form-checkbox
                                v-show="filter.isEnabled"
                                :id="filter.name + '-filter'"
                                v-model="selectedFilters"
                                :data-testid="`${filter.name}-filter`"
                                :name="filter.name + '-filter'"
                                :value="filter.value"
                            >
                                {{ filter.display }}
                            </b-form-checkbox>
                        </b-col>
                        <b-col cols="4" align-self="end" class="text-right">
                            ({{ formatFilterCount(filter.numEntries) }})
                        </b-col>
                    </b-row>
                    <b-row v-if="showSlider" class="mt-2">
                        <b-col>
                            <label for="entries-per-page"
                                >Items per page: {{ steps[stepIndex] }}</label
                            >
                            <b-form-input
                                @change="sliderChanged()"
                                id="entries-per-page"
                                v-model="stepIndex"
                                type="range"
                                min="0"
                                max="3"
                            ></b-form-input>
                        </b-col>
                    </b-row>
                </div>
            </b-dropdown>
        </div>
        <b-button
            class="d-d-sm-inline d-sm-none"
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
                    <b-row v-for="(filter, index) in filters" :key="index">
                        <b-col cols="8" align-self="start">
                            <b-form-checkbox
                                v-show="filter.isEnabled"
                                :id="filter.name + '-filter'"
                                v-model="selectedFilters"
                                :data-testid="`${filter.name}-filter`"
                                :name="filter.name + '-filter'"
                                :value="filter.value"
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
            <b-row v-if="showSlider" class="justify-content-center mt-2 mx-3">
                <b-col>
                    <label for="entries-per-page"
                        >Items per page: {{ steps[stepIndex] }}</label
                    >
                    <b-form-input
                        @change="sliderChanged()"
                        id="entries-per-page"
                        v-model="stepIndex"
                        type="range"
                        min="0"
                        max="3"
                    ></b-form-input>
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
</style>
