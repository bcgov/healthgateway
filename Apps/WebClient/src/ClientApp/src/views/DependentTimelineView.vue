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
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import NoteEditComponent from "@/components/modal/NoteEditComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import AddNoteButtonComponent from "@/components/timeline/AddNoteButtonComponent.vue";
import TimelineComponent from "@/components/timeline/TimelineComponent.vue";
import { EntryType, entryTypeMap } from "@/constants/entryType";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import { Dependent } from "@/models/dependent";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";
import DependentUtil from "@/utility/dependentUtil";

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
        LoadingComponent,
        NoteEditComponent,
        TimelineComponent,
    },
};

@Component(options)
export default class DependentTimelineView extends Vue {
    @Prop({ required: true })
    id!: string;

    @Action("retrieveDependents", { namespace: "dependent" })
    retrieveDependents!: (params: {
        hdid: string;
        bypassCache: boolean;
    }) => Promise<void>;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("dependents", { namespace: "dependent" })
    dependents!: Dependent[];

    @Getter("dependentsAreLoading", { namespace: "dependent" })
    dependentsAreLoading!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    logger!: ILogger;

    get breadcrumbItems(): BreadcrumbItem[] {
        return [
            {
                text: "Dependents",
                to: "/dependents",
                active: false,
                dataTestId: "breadcrumb-dependents",
            },
            {
                text: this.formattedName,
                active: true,
                dataTestId: "breadcrumb-dependent-name",
            },
        ];
    }

    get dependent(): Dependent | undefined {
        return this.dependents.find((d) => d.ownerId === this.hdid);
    }

    get entryTypes(): EntryType[] {
        return [...entryTypeMap.values()]
            .filter((d) => ConfigUtil.isDependentDatasetEnabled(d.type))
            .map((d) => d.type);
    }

    get formattedName(): string {
        return DependentUtil.formatName(this.dependent?.dependentInformation);
    }

    get hdid(): string {
        return this.id;
    }

    get title(): string {
        return [this.formattedName, "Timeline"]
            .filter((s) => Boolean(s))
            .join(" ");
    }

    async created(): Promise<void> {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        await this.retrieveDependents({
            hdid: this.user.hdid,
            bypassCache: false,
        });
        if (this.dependent === undefined) {
            await this.$router.push({ path: "/unauthorized" });
        }
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="dependentsAreLoading" />
        <div v-if="!dependentsAreLoading && dependent !== undefined">
            <BreadcrumbComponent :items="breadcrumbItems" />
            <page-title :title="title" />
            <TimelineComponent :hdid="hdid" :entry-types="entryTypes" />
        </div>
    </div>
</template>
