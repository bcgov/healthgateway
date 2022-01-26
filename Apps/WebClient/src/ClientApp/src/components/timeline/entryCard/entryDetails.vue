<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import EventBus, { EventMessageName } from "@/eventbus";
import { Operation } from "@/models/storeOperations";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import User from "@/models/user";

import Covid19LaboratoryOrderTimelineComponent from "./covid19LaboratoryOrder.vue";
import EncounterTimelineComponent from "./encounter.vue";
import ImmunizationTimelineComponent from "./immunization.vue";
import LaboratoryOrderTimelineComponent from "./laboratoryOrder.vue";
import MedicationTimelineComponent from "./medication.vue";
import MedicationRequestTimelineComponent from "./medicationRequest.vue";
import NoteTimelineComponent from "./note.vue";

library.add(faArrowLeft);

@Component({
    components: {
        MedicationRequestComponent: MedicationRequestTimelineComponent,
        MedicationComponent: MedicationTimelineComponent,
        ImmunizationComponent: ImmunizationTimelineComponent,
        Covid19LaboratoryOrderComponent:
            Covid19LaboratoryOrderTimelineComponent,
        LaboratoryOrderComponent: LaboratoryOrderTimelineComponent,
        EncounterComponent: EncounterTimelineComponent,
        NoteComponent: NoteTimelineComponent,
    },
})
export default class EntryDetailsComponent extends Vue {
    @Action("setHeaderState", { namespace: "navbar" }) setHeaderState!: (
        isOpen: boolean
    ) => void;

    @Getter("isMobile") isMobile!: boolean;
    @Getter("user", { namespace: "user" }) user!: User;
    @Getter("isVisible", { namespace: "idle" }) isIdleWarningVisible!: boolean;
    @Getter("lastOperation", { namespace: "note" })
    lastNoteOperation!: Operation | null;

    private entry: TimelineEntry | null = null;
    private entryDate = "";

    private eventBus = EventBus;

    private isVisible = false;

    private get modalTitle(): string {
        return "";
    }

    @Watch("isMobile")
    private onIsMobile() {
        if (this.isVisible && !this.isMobile) {
            this.handleClose();
        }
    }

    @Watch("lastNoteOperation")
    private onLastNoteOperation() {
        if (
            this.lastNoteOperation !== null &&
            this.entry !== null &&
            this.lastNoteOperation.id === this.entry.id
        ) {
            this.handleClose();
        }
    }
    private created() {
        window.onpopstate = (event: PopStateEvent) => {
            this.hideModal();
            event.preventDefault();
        };
    }

    private mounted() {
        this.entry = null;
        this.eventBus.$on(EventMessageName.ViewEntryDetails, this.viewDetails);
    }

    public viewDetails(entry: TimelineEntry): void {
        // Simulate a history push
        history.pushState({}, "Entry Details", "?details");
        this.entry = entry;
        this.entryDate = entry.date.toISO();
        this.isVisible = true;
        this.setHeaderState(false);
    }

    private getComponentForEntry(): string {
        switch (this.entry?.type) {
            case EntryType.MedicationRequest:
                return "MedicationRequestComponent";

            case EntryType.Medication:
                return "MedicationComponent";

            case EntryType.Immunization:
                return "ImmunizationComponent";

            case EntryType.Covid19LaboratoryOrder:
                return "Covid19LaboratoryOrderComponent";

            case EntryType.LaboratoryOrder:
                return "LaboratoryOrderComponent";

            case EntryType.Encounter:
                return "EncounterComponent";

            case EntryType.Note:
                return "NoteComponent";
            default:
                return "";
        }
    }

    public handleClose(): void {
        history.back();
    }

    public hideModal(): void {
        this.entry = null;
        this.isVisible = false;
    }

    private clear() {
        this.entry = null;
    }
}
</script>

<template>
    <b-modal
        id="entry-details-modal"
        v-model="isVisible"
        data-testid="entryDetailsModal"
        modal-class="entry-details-modal"
        header-class="entry-details-modal-header"
        dialog-class="entry-details-modal-dialog"
        content-class="entry-details-modal-content"
        size="lg"
        centered
        hide-footer
        scrollable
        @hidden="clear"
    >
        <template #modal-header>
            <b-row class="w-100 h-100">
                <b-col cols="auto">
                    <b-button
                        data-testid="backBtn"
                        variant="link"
                        size="sm"
                        class="back-button-icon mt-2 p-2"
                        @click="handleClose"
                    >
                        <hg-icon icon="arrow-left" size="medium" />
                    </b-button>
                </b-col>
                <b-col>
                    <h5>{{ modalTitle }}</h5>
                </b-col>
            </b-row>
        </template>
        <component
            :is="getComponentForEntry()"
            v-if="entry != null"
            :datekey="entryDate"
            :entry="entry"
            :index="1"
            :is-mobile-details="true"
            data-testid="entryDetailsCard"
        />
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.entryTitle {
    background-color: $soft_background;
    color: $primary;
    font-weight: bold;
    width: 100%;
    margin-right: -1px;
}

.entryDate {
    font-size: 0.8rem;
}

.back-button-icon {
    color: grey;
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.entry-details-modal-content {
    min-height: 100vh;
    border: 0px;
    border-radius: 0px;
    .modal-body {
        padding: 0em;
    }
}

.entry-details-modal-dialog {
    min-height: 100vh;
    min-width: 100%;
    margin: 0rem;
}

.entry-details-modal-header {
    background-color: white;
    padding-top: 0px;
    padding-bottom: 0px;
}
</style>
