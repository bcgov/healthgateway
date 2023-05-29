<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faArrowLeft } from "@fortawesome/free-solid-svg-icons";
import { Component, computed, onMounted, ref, watch } from "vue";
import { useStore } from "vue-composition-wrapper";

import { entryTypeMap } from "@/constants/entryType";
import EventBus, { EventMessageName } from "@/eventbus";
import { Operation } from "@/models/storeOperations";
import TimelineEntry from "@/models/timelineEntry";

import ClinicalDocumentTimelineComponent from "./ClinicalDocumentTimelineComponent.vue";
import Covid19LaboratoryOrderTimelineComponent from "./Covid19LaboratoryOrderTimelineComponent.vue";
import EncounterTimelineComponent from "./EncounterTimelineComponent.vue";
import HospitalVisitTimelineComponent from "./HospitalVisitTimelineComponent.vue";
import ImmunizationTimelineComponent from "./ImmunizationTimelineComponent.vue";
import LaboratoryOrderTimelineComponent from "./LaboratoryOrderTimelineComponent.vue";
import MedicationRequestTimelineComponent from "./MedicationRequestTimelineComponent.vue";
import MedicationTimelineComponent from "./MedicationTimelineComponent.vue";
import NoteTimelineComponent from "./NoteTimelineComponent.vue";

library.add(faArrowLeft);

interface Props {
    hdid: string;
    commentsAreEnabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    commentsAreEnabled: false,
});

const eventBus = EventBus;
const modalTitle = "";

const store = useStore();

const entry = ref<TimelineEntry | null>(null);
const entryDate = ref("");
const isVisible = ref(false);

const isMobile = computed<boolean>(() => store.getters["isMobile"]);

const lastNoteOperation = computed<Operation | null>(
    () => store.getters["note/lastOperation"]
);

const componentForEntry = computed<Component | string>(() => {
    switch (entryTypeMap.get(entry.value?.type)?.component) {
        case "ClinicalDocumentTimelineComponent":
            return ClinicalDocumentTimelineComponent;
        case "Covid19LaboratoryOrderTimelineComponent":
            return Covid19LaboratoryOrderTimelineComponent;
        case "EncounterTimelineComponent":
            return EncounterTimelineComponent;
        case "HospitalVisitTimelineComponent":
            return HospitalVisitTimelineComponent;
        case "ImmunizationTimelineComponent":
            return ImmunizationTimelineComponent;
        case "LaboratoryOrderTimelineComponent":
            return LaboratoryOrderTimelineComponent;
        case "MedicationRequestTimelineComponent":
            return MedicationRequestTimelineComponent;
        case "MedicationTimelineComponent":
            return MedicationTimelineComponent;
        case "NoteTimelineComponent":
            return NoteTimelineComponent;
        default:
            return "";
    }
});

function setHeaderState(isOpen: boolean): void {
    store.dispatch("navbar/setHeaderState", isOpen);
}

function viewDetails(incomingEntry: TimelineEntry): void {
    // Simulate a history push
    history.pushState({}, "Entry Details", "?details");
    entry.value = incomingEntry;
    entryDate.value = incomingEntry.date.toISO();
    isVisible.value = true;
    setHeaderState(false);
}

function handleClose(): void {
    history.back();
}

function hideModal(): void {
    entry.value = null;
    isVisible.value = false;
}

function clear(): void {
    entry.value = null;
}

watch(isMobile, () => {
    if (isVisible.value && isMobile.value) {
        handleClose();
    }
});

watch(lastNoteOperation, () => {
    if (
        lastNoteOperation.value !== null &&
        entry.value !== null &&
        lastNoteOperation.value.id === entry.value.id
    ) {
        handleClose();
    }
});

onMounted(() => {
    entry.value = null;
    eventBus.$on(EventMessageName.ViewEntryDetails, viewDetails);
});

// Created Hook
window.onpopstate = (event: PopStateEvent) => {
    hideModal();
    event.preventDefault();
};
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
            :is="componentForEntry"
            v-if="entry != null"
            :datekey="entryDate"
            :entry="entry"
            :index="1"
            :is-mobile-details="true"
            :hdid="hdid"
            :comments-are-enabled="commentsAreEnabled"
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
    border: 0;
    border-radius: 0;

    .modal-body {
        padding: 0;
    }
}

.entry-details-modal-dialog {
    min-height: 100vh;
    min-width: 100%;
    margin: 0;
}

.entry-details-modal-header {
    background-color: white;
    padding-top: 0;
    padding-bottom: 0;
}
</style>
