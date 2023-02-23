<script lang="ts">
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
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import NoteEditComponent from "@/components/modal/NoteEditComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import AddNoteButtonComponent from "@/components/timeline/AddNoteButtonComponent.vue";
import TimelineComponent from "@/components/timeline/TimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
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
    faVial
);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        AddNoteButtonComponent,
        BreadcrumbComponent,
        NoteEditComponent,
        TimelineComponent,
    },
};

@Component(options)
export default class UserTimelineView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("notesAreLoading", { namespace: "note" })
    notesAreLoading!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    logger!: ILogger;

    breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Timeline",
            to: "/timeline",
            active: true,
            dataTestId: "breadcrumb-timeline",
        },
    ];

    get commentsAreEnabled(): boolean {
        return this.config.modules["Comment"];
    }

    get entryTypes(): EntryType[] {
        return [...entryTypeMap.values()]
            .filter((d) => ConfigUtil.isDatasetEnabled(d.type))
            .map((d) => d.type);
    }

    get notesAreEnabled(): boolean {
        return ConfigUtil.isDatasetEnabled(EntryType.Note);
    }

    created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }
}
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
