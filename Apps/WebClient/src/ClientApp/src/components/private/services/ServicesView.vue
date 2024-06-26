<script setup lang="ts">
import { computed } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import OrganDonorDetailsCard from "@/components/private/services/OrganDonorDetailsCard.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { DataSource } from "@/constants/dataSource";
import { ServiceName } from "@/constants/serviceName";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { PatientDataType } from "@/models/patientDataResponse";
import { usePatientDataStore } from "@/stores/patientData";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";
import { useGrid } from "@/utility/useGrid";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Services",
        to: "/services",
        active: true,
        dataTestId: "breadcrumb-services",
    },
];

const userStore = useUserStore();
const patientDataStore = usePatientDataStore();
const { columns } = useGrid();

const patientDataAreLoading = computed(
    () =>
        isOrganDonorServiceEnabled.value &&
        patientDataStore.patientDataAreLoading(userStore.hdid)
);

const isOrganDonorServiceEnabled = computed(() =>
    ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration)
);

const isOrganDonorServiceBlocked = computed(() =>
    userStore.blockedDataSources.includes(DataSource.OrganDonorRegistration)
);

function retrievePatientData(): void {
    let dataTypes: PatientDataType[] = [];
    if (isOrganDonorServiceEnabled.value && !isOrganDonorServiceBlocked.value) {
        dataTypes = [
            ...dataTypes,
            PatientDataType.OrganDonorRegistrationStatus,
        ];
    }

    if (dataTypes.length > 0) {
        patientDataStore.retrievePatientData(userStore.hdid, dataTypes);
    }
}

retrievePatientData();
</script>

<template>
    <BreadcrumbComponent :items="breadcrumbItems" />
    <LoadingComponent :is-loading="patientDataAreLoading" />
    <PageTitleComponent title="Services" />
    <p>
        You can check and update your Organ Donor Registry information here.
        More health services will be added in future.
    </p>
    <v-row>
        <v-col
            v-if="!patientDataAreLoading && isOrganDonorServiceEnabled"
            :cols="columns"
            class="pa-4"
        >
            <OrganDonorDetailsCard :hdid="userStore.hdid" />
        </v-col>
    </v-row>
</template>
