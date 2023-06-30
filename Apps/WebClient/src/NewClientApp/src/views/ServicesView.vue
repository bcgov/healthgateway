<script setup lang="ts">
import { computed } from "vue";

import LoadingComponent from "@/components/shared/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import OrganDonorDetailsCard from "@/components/services/OrganDonorDetailsCard.vue";
import { ServiceName } from "@/constants/serviceName";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { PatientData, PatientDataType } from "@/models/patientDataResponse";
import ConfigUtil from "@/utility/configUtil";
import { useUserStore } from "@/stores/user";
import { usePatientDataStore } from "@/stores/patientData";
import PageTitleComponent from "@/components/shared/PageTitleComponent.vue";
import { useDisplay } from "vuetify";
import { getGridCols } from "@/utility/gridUtilty";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Services",
        to: "/services",
        active: true,
        dataTestId: "breadcrumb-services",
    },
];

const display = useDisplay();
const userStore = useUserStore();
const patientDataStore = usePatientDataStore();

const hdid = computed(() => userStore.user.hdid);

const patientDataAreLoading = computed(
    () =>
        isOrganDonorServiceEnabled.value &&
        patientDataStore.patientDataAreLoading(hdid.value)
);

const serviceGridCols = computed(() => getGridCols(display));

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
    <v-container>
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
                :cols="serviceGridCols"
                class="pa-3"
            >
                <OrganDonorDetailsCard :hdid="hdid" />
            </v-col>
        </v-row>
    </v-container>
</template>
