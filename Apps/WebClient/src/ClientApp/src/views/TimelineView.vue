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
import LinearTimelineComponent from "@/components/timeline/LinearTimelineComponent.vue";
import { EntryType } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

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
        LinearTimelineComponent,
        NoteEditComponent,
    },
};

@Component(options)
export default class TimelineView extends Vue {
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
        return [
            EntryType.ClinicalDocument,
            EntryType.Covid19LaboratoryOrder,
            EntryType.Encounter,
            EntryType.HospitalVisit,
            EntryType.Immunization,
            EntryType.LaboratoryOrder,
            EntryType.Medication,
            EntryType.Note,
            EntryType.MedicationRequest,
        ].filter((entryType) => this.config.modules[entryType]);
    }

    get notesAreEnabled(): boolean {
        return this.config.modules["Note"];
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
        <LinearTimelineComponent
            :hdid="user.hdid"
            :entry-types="entryTypes"
            :comments-are-enabled="commentsAreEnabled"
        />
        <NoteEditComponent :is-loading="notesAreLoading" />
    </div>
</template>
