<script lang="ts">
import {
    faEdit,
    faEllipsisV,
    faFlask,
    faLongArrowAltLeft,
    faPills,
    faQuestion,
    faSyringe,
    faUserMd,
    IconDefinition,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { required } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import DatePickerComponent from "@/components/datePicker.vue";
import LoadingComponent from "@/components/loading.vue";
import EventBus, { EventMessageName } from "@/eventbus";
import { DateWrapper } from "@/models/dateWrapper";
import EncounterTimelineEntry from "@/models/encounterTimelineEntry";
import ImmunizationTimelineEntry from "@/models/immunizationTimelineEntry";
import LaboratoryTimelineEntry from "@/models/laboratoryTimelineEntry";
import MedicationTimelineEntry from "@/models/medicationTimelineEntry";
import NoteTimelineEntry from "@/models/noteTimelineEntry";
import TimelineEntry, { EntryType } from "@/models/timelineEntry";
import User from "@/models/user";
import UserNote from "@/models/userNote";

import EncounterTimelineComponent from "./encounter.vue";
import ImmunizationTimelineComponent from "./immunization.vue";
import LaboratoryTimelineComponent from "./laboratory.vue";
import MedicationTimelineComponent from "./medication.vue";
import NoteTimelineComponent from "./note.vue";

@Component({
    components: {
        LoadingComponent,
        DatePickerComponent,
        MedicationComponent: MedicationTimelineComponent,
        ImmunizationComponent: ImmunizationTimelineComponent,
        LaboratoryComponent: LaboratoryTimelineComponent,
        EncounterComponent: EncounterTimelineComponent,
        NoteComponent: NoteTimelineComponent,
    },
})
export default class EntryDetailsComponent extends Vue {
    @Action("createNote", { namespace: "note" }) createNote!: (params: {
        hdid: string;
        note: UserNote;
    }) => Promise<UserNote>;
    @Action("updateNote", { namespace: "note" }) updateNote!: (params: {
        hdid: string;
        note: UserNote;
    }) => Promise<UserNote>;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isVisible", { namespace: "idle" }) isIdleWarningVisible!: boolean;

    private windowWidth = 0;

    private get isMobileView(): boolean {
        return this.windowWidth < 576;
    }

    private entry?: TimelineEntry;
    private entryTitle = "";
    private entrySubTitle = "";
    private entryDate = "";
    private entryIcon: IconDefinition = faQuestion;

    private errorMessage = "";
    private eventBus = EventBus;

    private isVisible = false;

    private get backButtonIcon(): IconDefinition {
        return faLongArrowAltLeft;
    }

    private get modalTitle(): string {
        return "";
    }

    private get menuIcon(): IconDefinition {
        return faEllipsisV;
    }

    private mounted() {
        this.clear();
        this.eventBus.$on(EventMessageName.ViewEntryDetails, this.viewDetails);
    }

    private validations() {
        return {
            title: {
                required: required,
            },
        };
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private dateString(entryDate: DateWrapper): string {
        const today = new DateWrapper();
        if (entryDate.isSame(today, "day")) {
            return "Today";
        } else if (entryDate.year() === today.year()) {
            return entryDate.format("MMM d");
        } else {
            return entryDate.format("yyyy-MM-dd");
        }
    }

    public viewDetails(entry: TimelineEntry): void {
        this.clear();
        this.entry = entry;
        this.entryDate = this.dateString(entry.date);
        this.windowWidth = window.innerWidth;
        if (this.entry.type == EntryType.Medication) {
            this.entryIcon = faPills;
            const med: MedicationTimelineEntry = this
                .entry as MedicationTimelineEntry;
            this.entryTitle = med.medication.brandName;
            this.entrySubTitle =
                med.medication.genericName != null
                    ? med.medication.genericName
                    : "";
        }
        if (this.entry.type == EntryType.Immunization) {
            this.entryIcon = faSyringe;
            const immunization: ImmunizationTimelineEntry = this
                .entry as ImmunizationTimelineEntry;
            this.entryTitle = immunization.immunization.name;
        }
        if (this.entry.type == EntryType.Laboratory) {
            this.entryIcon = faFlask;
            const lab: LaboratoryTimelineEntry = this
                .entry as LaboratoryTimelineEntry;
            this.entryTitle = lab.summaryTitle;
            this.entrySubTitle =
                lab.labResultOutcome != null ? lab.labResultOutcome : "";
        }
        if (this.entry.type == EntryType.Note) {
            this.entryIcon = faEdit;
            const note: NoteTimelineEntry = this.entry as NoteTimelineEntry;
            this.entryTitle = note.title;
        }
        if (this.entry.type == EntryType.Encounter) {
            this.entryIcon = faUserMd;
            const encounter: EncounterTimelineEntry = this
                .entry as EncounterTimelineEntry;
            this.entryTitle = encounter.practitionerName;
            this.entrySubTitle = encounter.specialtyDescription;
        }
        this.isVisible = true;
    }

    private getComponentForEntry(): string {
        switch (this.entry?.type) {
            case EntryType.Medication:
                return "MedicationComponent";

            case EntryType.Immunization:
                return "ImmunizationComponent";

            case EntryType.Laboratory:
                return "LaboratoryComponent";

            case EntryType.Encounter:
                return "EncounterComponent";

            case EntryType.Note:
                return "NoteComponent";
            default:
                return "";
        }
    }

    public hideModal(): void {
        this.$v.$reset();
        this.isVisible = false;
        this.clear();
    }

    private clear() {
        this.entry = undefined;
        this.entryTitle = "";
        this.entrySubTitle = "";
        this.entryDate = "";
    }
}
</script>

<template>
    <b-modal
        id="entry-details-modal"
        v-model="isVisible"
        data-testid="entryDetailsModal"
        content-class="mt-0 mobile-content"
        size="lg"
        header-class="entry-details-modal-header"
        header-text-variant="light"
        centered
        hide-footer
        @hidden="clear"
    >
        <b-alert
            data-testid="entryDetailsErrorBanner"
            variant="danger"
            dismissible
            class="no-print"
            :show="!!errorMessage"
        >
            <p data-testid="entryDetailsErrorText">{{ errorMessage }}</p>
            <span>
                If you continue to have issues, please contact
                HealthGateway@gov.bc.ca.
            </span>
        </b-alert>
        <template #modal-header>
            <b-row class="w-100 h-100">
                <b-col cols="auto">
                    <b-button
                        data-testid="noNeedBtn"
                        variant="link"
                        size="sm"
                        class="back-button-icon"
                        @click="hideModal"
                    >
                        <font-awesome-icon
                            :icon="backButtonIcon"
                            size="lg"
                        ></font-awesome-icon>
                    </b-button>
                </b-col>
                <b-col>
                    <h5>{{ modalTitle }}</h5>
                </b-col>
            </b-row>
        </template>
        <component
            :is="getComponentForEntry()"
            :datekey="entryDate"
            :entry="entry"
            :index="1"
            :view-details="true"
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

.icon,
.back-button-icon {
    text-align: center;
    border-radius: 50%;
    height: 60px;
    width: 60px;
    padding-top: 17px;
    font-size: 1.2em;
}

.back-button-icon {
    color: grey;
    background-color: white;
}

.icon {
    color: white;
    background-color: $primary;
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";
.mobile-content {
    position: relative;
    right: auto;
    height: 1400px;
    border: 0px;
    left: 0;
    top: 60px;
    border-radius: 0px;
    .modal-body {
        padding: 0em;
    }
}
.modal-dialog {
    margin: 0rem;
}

.edit-modal-header {
    background-color: white;
}
</style>
