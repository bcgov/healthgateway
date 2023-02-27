<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import ReportsComponent from "@/components/report/ReportsComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        ReportsComponent,
    },
};

@Component(options)
export default class ReportsView extends Vue {
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" })
    user!: User;

    public breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Export Records",
            to: "/reports",
            active: true,
            dataTestId: "breadcrumb-export-records",
        },
    ];
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <page-title title="Export Records" />
        <ReportsComponent :hdid="user.hdid" />
    </div>
</template>
