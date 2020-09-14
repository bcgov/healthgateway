<style lang="scss" scoped>
.filters-wrapper {
    z-index: 3;
}
.filters-width {
    width: 175px;
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
<template>
    <div class="filters-wrapper">
        <div class="filters-width d-none d-sm-block">
            <b-dropdown
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
                    <b-form-checkbox
                        v-if="isMedicationEnabled"
                        id="medicationFilter"
                        v-model="filterTypes"
                        name="medicationFilter"
                        value="Medication"
                    >
                        Medications
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isImmunizationEnabled"
                        id="immunizationFilter"
                        v-model="filterTypes"
                        name="immunizationFilter"
                        value="Immunization"
                    >
                        Immunizations
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isLaboratoryEnabled"
                        id="laboratoryFilter"
                        v-model="filterTypes"
                        name="laboratoryFilter"
                        value="Laboratory"
                    >
                        Laboratory
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isNoteEnabled"
                        id="notesFilter"
                        v-model="filterTypes"
                        name="notesFilter"
                        value="Note"
                    >
                        My Notes
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isEncounterEnabled"
                        id="encounterFilter"
                        v-model="filterTypes"
                        name="encounterFilter"
                        value="Encounter"
                    >
                        MSP Visits
                    </b-form-checkbox>
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
            <template v-slot:modal-header="{ close }">
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
                    <b-form-checkbox
                        v-if="isMedicationEnabled"
                        id="medicationFilter"
                        v-model="filterTypes"
                        name="medicationFilter"
                        value="Medication"
                    >
                        Medications
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isImmunizationEnabled"
                        id="immunizationFilter"
                        v-model="filterTypes"
                        name="immunizationFilter"
                        value="Immunization"
                    >
                        Immunizations
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isLaboratoryEnabled"
                        id="laboratoryFilter"
                        v-model="filterTypes"
                        name="laboratoryFilter"
                        value="Laboratory"
                    >
                        Laboratory
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isNoteEnabled"
                        id="notesFilter"
                        v-model="filterTypes"
                        name="notesFilter"
                        value="Note"
                    >
                        My Notes
                    </b-form-checkbox>
                    <b-form-checkbox
                        v-if="isEncounterEnabled"
                        id="encounterFilter"
                        v-model="filterTypes"
                        name="encounterFilter"
                        value="Encounter"
                    >
                        MSP Visits
                    </b-form-checkbox>
                </b-col>
            </b-row>
        </b-modal>
    </div>
</template>
<script lang="ts">
import { WebClientConfiguration } from "@/models/configData";
import Vue from "vue";
import Component from "vue-class-component";
import { Getter } from "vuex-class";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSlidersH } from "@fortawesome/free-solid-svg-icons";
import { Emit, Watch } from "vue-property-decorator";
library.add(faSlidersH);

@Component
export default class FilterComponent extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;
    @Getter("isOpen", { namespace: "sidebar" }) isOpen!: boolean;

    private isVisible: boolean = false;
    private filterTypes: string[] = [];
    private windowWidth: number = 0;

    private get isMedicationEnabled(): boolean {
        return this.config.modules["Medication"];
    }

    private get isImmunizationEnabled(): boolean {
        return this.config.modules["Immunization"];
    }

    private get isLaboratoryEnabled(): boolean {
        return this.config.modules["Laboratory"];
    }

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }

    private get isEncounterEnabled(): boolean {
        return this.config.modules["Encounter"];
    }

    private get isMobileView(): boolean {
        return this.windowWidth < 576;
    }

    @Watch("isMobileView")
    private onIsMobileView() {
        this.isVisible = false;
    }

    @Watch("isOpen")
    private onIsOpen(newValue: boolean, oldValue: boolean) {
        this.isVisible = false;
    }

    @Watch("filterTypes")
    private onFilterUpdate() {
        this.filtersChanged();
    }

    @Emit()
    public filtersChanged() {
        return this.filterTypes;
    }

    private created() {
        window.addEventListener("resize", this.handleResize);
        this.handleResize();
    }

    private mounted() {
        this.initializeFilters();
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

    private initializeFilters(): void {
        if (this.isMedicationEnabled) {
            this.filterTypes.push("Medication");
        }
        if (this.isImmunizationEnabled) {
            this.filterTypes.push("Immunization");
        }
        if (this.isLaboratoryEnabled) {
            this.filterTypes.push("Laboratory");
        }
        if (this.isNoteEnabled) {
            this.filterTypes.push("Note");
        }
        if (this.isEncounterEnabled) {
            this.filterTypes.push("Encounter");
        }
    }

    private clearFilters(): void {
        this.filterTypes = [];
    }
}
</script>
