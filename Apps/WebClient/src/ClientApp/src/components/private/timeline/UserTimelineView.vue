<script setup lang="ts">
import { computed } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import NoteDialogComponent from "@/components/private/timeline/NoteDialogComponent.vue";
import TimelineComponent from "@/components/private/timeline/TimelineComponent.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import { container } from "@/ioc/container";
import { SERVICE_IDENTIFIER } from "@/ioc/identifier";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { Action, Text, Type } from "@/plugins/extensions";
import { ITrackingService } from "@/services/interfaces";
import { useConfigStore } from "@/stores/config";
import { EventName, useEventStore } from "@/stores/event";
import { useNoteStore } from "@/stores/note";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Health Records",
        to: "/timeline",
        active: true,
        dataTestId: "breadcrumb-timeline",
    },
];

const configStore = useConfigStore();
const eventStore = useEventStore();
const noteStore = useNoteStore();
const userStore = useUserStore();

const trackingService = container.get<ITrackingService>(
    SERVICE_IDENTIFIER.TrackingService
);

const commentsAreEnabled = computed(
    () => configStore.webConfig.featureToggleConfiguration.timeline.comment
);
const notesAreLoading = computed(() => noteStore.notesAreLoading);
const notesAreEnabled = computed(() =>
    ConfigUtil.isDatasetEnabled(EntryType.Note)
);
const entryTypes = computed(() =>
    [...entryTypeMap.values()]
        .filter((d) => ConfigUtil.isDatasetEnabled(d.type))
        .map((d) => d.type)
);

function openNoteDialog(): void {
    trackingService.trackEvent({
        action: Action.ButtonClick,
        text: Text.AddNote,
        type: Type.Notes,
    });
    eventStore.emit(EventName.OpenNoteDialog);
}
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <PageTitleComponent title="Health Records">
        <template #append>
            <HgButtonComponent
                v-if="notesAreEnabled && !notesAreLoading"
                data-testid="addNoteBtn"
                variant="secondary"
                prepend-icon="edit"
                @click="openNoteDialog"
            >
                <span class="d-none d-sm-inline">Add a Note</span>
            </HgButtonComponent>
        </template>
    </PageTitleComponent>
    <TimelineComponent
        :hdid="userStore.hdid"
        :entry-types="entryTypes"
        :comments-are-enabled="commentsAreEnabled"
    />
    <NoteDialogComponent :is-loading="notesAreLoading" />
</template>
