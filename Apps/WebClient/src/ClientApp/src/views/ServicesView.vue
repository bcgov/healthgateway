<script setup lang="ts">
import { computed } from "vue";
import { useStore } from "vue-composition-wrapper";

import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import OrganDonorDetailsCard from "@/components/services/OrganDonorDetailsCard.vue";
import { ServiceName } from "@/constants/serviceName";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { PatientDataType } from "@/models/patientDataResponse";
import ConfigUtil from "@/utility/configUtil";

const breadcrumbItems: BreadcrumbItem[] = [
    {
        text: "Services",
        to: "/services",
        active: true,
        dataTestId: "breadcrumb-services",
    },
];

const store = useStore();

const hdid = computed<string>(() => store.getters["user/user"].hdid);

const patientDataAreLoading = computed<boolean>(() =>
    store.getters["patientData/patientDataAreLoading"](hdid.value)
);

const isOrganDonorServiceEnabled = computed(() =>
    ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration)
);

function retrievePatientData(
    hdid: string,
    patientDataTypes: PatientDataType[]
): Promise<void> {
    return store.dispatch("patientData/retrievePatientData", {
        hdid,
        patientDataTypes,
    });
}

if (isOrganDonorServiceEnabled.value) {
    retrievePatientData(hdid.value, [
        PatientDataType.OrganDonorRegistrationStatus,
    ]);
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="patientDataAreLoading" />
        <page-title title="Services" />
        <p>
            You can check and update your Organ Donor Registry information here.
            More health services will be added in future.
        </p>
        <div>
            <b-row cols="1" cols-lg="2" cols-xl="3">
                <b-col
                    v-if="!patientDataAreLoading && isOrganDonorServiceEnabled"
                    class="p-3"
                >
                    <OrganDonorDetailsCard :hdid="hdid" />
                </b-col>
            </b-row>
        </div>
    </div>
</template>
