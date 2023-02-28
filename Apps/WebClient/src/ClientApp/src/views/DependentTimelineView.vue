<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faArrowLeft,
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
import { Dependent, DependentInformation } from "@/models/dependent";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";

library.add(
    faArrowLeft,
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

    get dependentInfo(): DependentInformation | undefined {
        return this.dependent?.dependentInformation;
    }

    get entryTypes(): EntryType[] {
        return [...entryTypeMap.values()]
            .filter((d) => ConfigUtil.isDependentDatasetEnabled(d.type))
            .map((d) => d.type);
    }

    get formattedName(): string {
        const firstName = this.dependentInfo?.firstname;
        const lastInitial = this.dependentInfo?.lastname?.slice(0, 1);
        return [firstName, lastInitial].filter((s) => Boolean(s)).join(" ");
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

    handleBack(): void {
        this.$router.push({ path: "/dependents" });
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="dependentsAreLoading" />
        <div v-if="!dependentsAreLoading && dependent !== undefined">
            <BreadcrumbComponent :items="breadcrumbItems" />
            <b-row class="w-100 h-100">
                <b-col cols="auto">
                    <b-button
                        data-testid="backBtn"
                        variant="link"
                        size="sm"
                        class="back-button-icon mt-2 p-2"
                        @click="handleBack"
                    >
                        <hg-icon icon="arrow-left" size="large" />
                    </b-button>
                </b-col>
                <b-col>
                    <page-title :title="title" />
                </b-col>
            </b-row>
            <TimelineComponent :hdid="hdid" :entry-types="entryTypes" />
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.back-button-icon {
    color: grey;
}
</style>
