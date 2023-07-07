<script setup lang="ts">
import { computed } from "vue";

import LoadingComponent from "@/components/common/LoadingComponent.vue";
import PageTitleComponent from "@/components/common/PageTitleComponent.vue";
import OrganDonorDetailsCard from "@/components/private/services/OrganDonorDetailsCard.vue";
import BreadcrumbComponent from "@/components/site/BreadcrumbComponent.vue";
import { ServiceName } from "@/constants/serviceName";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { PatientData, PatientDataType } from "@/models/patientDataResponse";
import { usePatientDataStore } from "@/stores/patientData";
import { useUserStore } from "@/stores/user";
import ConfigUtil from "@/utility/configUtil";
import { getGridCols } from "@/utility/gridUtilty";

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

const hdid = computed(() => userStore.user.hdid);

const patientDataAreLoading = computed(
    () =>
        isOrganDonorServiceEnabled.value &&
        patientDataStore.patientDataAreLoading(hdid.value)
);

const isOrganDonorServiceEnabled = computed(() =>
    ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration)
);

function retrievePatientData(): Promise<PatientData[]> {
    return patientDataStore.retrievePatientData(hdid.value, [
        PatientDataType.OrganDonorRegistrationStatus,
    ]);
}

if (isOrganDonorServiceEnabled.value) {
    retrievePatientData();
}
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
            :cols="getGridCols"
            class="pa-3"
        >
            <OrganDonorDetailsCard :hdid="hdid" />
        </v-col>
    </v-row>
</template>
