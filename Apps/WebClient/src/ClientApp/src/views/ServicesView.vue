<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import OrganDonorDetailsCard from "@/components/services/OrganDonorDetailsCard.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";

@Component({
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        OrganDonorDetailsCard,
    },
})
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
        <p class="mb-4">
            You can check and update your Organ Donor Registry information here.
            More health services will be added in future.
        </p>
        <div>
            <b-row cols="1" cols-lg="2" cols-xl="3">
                <b-col class="pb-3"> <OrganDonorDetailsCard /></b-col>
            </b-row>
        </div>
    </div>
</template>
