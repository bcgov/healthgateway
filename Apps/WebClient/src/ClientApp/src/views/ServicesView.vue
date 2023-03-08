<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        LoadingComponent,
    },
};
@Component(options)
export default class ServicesView extends Vue {
    breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Services",
            to: "/services",
            active: true,
            dataTestId: "breadcrumb-services",
        },
    ];

    get isLoading() {
        return false;
    }
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <page-title title="Services" />
        <h5 class="my-3">
            You can check and update your Organ Donor Registry information here.
            More health services will be added in future.
        </h5>
    </div>
</template>
