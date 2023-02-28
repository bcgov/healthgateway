<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import DependentCardComponent from "@/components/dependent/DependentCardComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { Dependent } from "@/models/dependent";
import User from "@/models/user";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        DependentCardComponent,
        LoadingComponent,
    },
};

@Component(options)
export default class DependentManagementView extends Vue {
    @Action("retrieveDependents", { namespace: "dependent" })
    private retrieveDependents!: (params: {
        hdid: string;
        bypassCache: boolean;
    }) => Promise<void>;

    @Getter("dependents", { namespace: "dependent" })
    private dependents!: Dependent[];

    @Getter("dependentsAreLoading", { namespace: "dependent" })
    private dependentsAreLoading!: boolean;

    @Getter("user", { namespace: "user" })
    private user!: User;

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Dependents",
            to: "/dependents",
            active: true,
            dataTestId: "breadcrumb-dependents",
        },
    ];

    private get isLoading(): boolean {
        return this.dependentsAreLoading;
    }

    private async created(): Promise<void> {
        await this.retrieveDependents({
            hdid: this.user.hdid,
            bypassCache: false,
        });
    }
}
</script>
<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <page-title title="Dependents" />
        <DependentCardComponent
            v-for="dependent in dependents"
            :key="dependent.ownerId"
            :dependent="dependent"
        />
    </div>
</template>
