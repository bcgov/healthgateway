<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Getter } from "vuex-class";

import BreadcrumbItem from "@/models/breadcrumbItem";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

@Component
export default class BreadcrumbComponent extends Vue {
    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    isAuthenticated!: boolean;

    private logger!: ILogger;

    @Prop({ required: false, default: [] }) items!: BreadcrumbItem[];

    private baseBreadcrumbItem: BreadcrumbItem = {
        text: "Home",
        to: "/home",
        dataTestId: "breadcrumb-home",
    };

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private get allBreadcrumbItems(): BreadcrumbItem[] {
        return [this.baseBreadcrumbItem, ...this.items];
    }

    private visitLink(link: string) {
        window.open(link, "_self");
    }
}
</script>

<template>
    <b-breadcrumb v-if="isAuthenticated" data-testid="breadcrumbs" class="pt-0">
        <b-breadcrumb-item
            v-for="item in allBreadcrumbItems"
            :key="item.text"
            :to="item.to"
            :active="item.active"
            :data-testid="item.dataTestId"
            @click="visitLink(item.href)"
        >
            {{ item.text }}
        </b-breadcrumb-item>
    </b-breadcrumb>
</template>

<style scoped>
.breadcrumb {
    padding: 0.75rem 0rem;
    margin-bottom: 0rem;
    background-color: transparent;
}

a {
    cursor: pointer !important;
}
</style>
