<script setup lang="ts">
import { computed, ref, watch } from "vue";

import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import ErrorCardComponent from "@/components/error/ErrorCardComponent.vue";
import { entryTypeMap } from "@/constants/entryType";
import TimelineEntry from "@/models/timeline/timelineEntry";
import { EventName, useEventStore } from "@/stores/event";
import { useLayoutStore } from "@/stores/layout";

interface Props {
    hdid: string;
    commentsAreEnabled?: boolean;
}
withDefaults(defineProps<Props>(), {
    commentsAreEnabled: false,
});

const layoutStore = useLayoutStore();
const eventStore = useEventStore();

const entry = ref<TimelineEntry>();
const entryDate = ref("");
const isVisible = ref(false);

const componentForEntry = computed(
    () => entryTypeMap.get(entry.value?.type)?.component ?? ""
);

function openDialog(incomingEntry: TimelineEntry): void {
    // Simulate a history push
    history.pushState({}, "Entry Details", "?details");

    entry.value = incomingEntry;
    entryDate.value = incomingEntry.date.toISO();

    isVisible.value = true;
    layoutStore.setHeaderState(false);
}

function closeDialog(): void {
    if (isVisible.value) {
        history.back();
    }
}

function handleEntryUpdate(entryId: string): void {
    if (entryId === entry.value?.id) {
        closeDialog();
    }
}

watch(
    () => layoutStore.isMobile,
    (value) => {
        if (!value) {
            closeDialog();
        }
    }
);

eventStore.subscribe(EventName.OpenFullscreenTimelineEntry, openDialog);
eventStore.subscribe(EventName.UpdateTimelineEntry, handleEntryUpdate);

window.onpopstate = (event: PopStateEvent) => {
    isVisible.value = false;
    event.preventDefault();
};
</script>

<template>
    <v-dialog
        id="entry-details-modal"
        v-model="isVisible"
        data-testid="entryDetailsModal"
        persistent
        fullscreen
        scrollable
    >
        <v-card>
            <v-card-title class="px-0">
                <v-toolbar density="compact" color="white">
                    <HgIconButtonComponent
                        data-testid="backBtn"
                        icon="arrow-left"
                        @click="closeDialog"
                    />
                </v-toolbar>
            </v-card-title>
            <v-divider />
            <v-card-text v-if="entry != null" class="pa-0">
                <div class="mx-2"><ErrorCardComponent /></div>
                <component
                    :is="componentForEntry"
                    data-testid="entryDetailsCard"
                    :datekey="entryDate"
                    :entry="entry"
                    :index="1"
                    :is-mobile-details="true"
                    :hdid="hdid"
                    :comments-are-enabled="commentsAreEnabled"
                />
            </v-card-text>
        </v-card>
    </v-dialog>
</template>
