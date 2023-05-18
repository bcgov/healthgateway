<script setup lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial,
    faXRay,
} from "@fortawesome/free-solid-svg-icons";
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

import NoteEditComponent from "@/components/modal/NoteEditComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import AddNoteButtonComponent from "@/components/timeline/AddNoteButtonComponent.vue";
import TimelineComponent from "@/components/timeline/TimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import ConfigUtil from "@/utility/configUtil";

library.add(
    faCheckCircle,
    faEdit,
    faFileMedical,
    faFileWaveform,
    faHouseMedical,
    faMicroscope,
    faPills,
    faQuestion,
    faSearch,
    faStethoscope,
    faSyringe,
    faVial,
    faXRay
);

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Timeline",
        to: "/timeline",
        active: true,
        dataTestId: "breadcrumb-timeline",
    },
];

const store = useStore();

const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);

const notesAreLoading = computed<boolean>(
    () => store.getters["note/notesAreLoading"]
);

const user = computed<User>(() => store.getters["user/user"]);

const commentsAreEnabled = computed(
    () => config.value.featureToggleConfiguration.timeline.comment
);

const notesAreEnabled = computed(() =>
    ConfigUtil.isDatasetEnabled(EntryType.Note)
);

const entryTypes = computed(() =>
    [...entryTypeMap.values()]
        .filter((d) => ConfigUtil.isDatasetEnabled(d.type))
        .map((d) => d.type)
);
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Timeline">
            <div class="float-right">
                <AddNoteButtonComponent
                    v-if="notesAreEnabled && !notesAreLoading"
                />
            </div>
        </page-title>
        <TimelineComponent
            :hdid="user.hdid"
            :entry-types="entryTypes"
            :comments-are-enabled="commentsAreEnabled"
        />
        <NoteEditComponent :is-loading="notesAreLoading" />
    </div>
</template>
