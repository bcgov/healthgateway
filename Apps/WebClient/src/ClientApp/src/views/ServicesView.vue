<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import OrganDonorDetailsCard from "@/components/services/OrganDonorDetailsCard.vue";
import { ServiceName } from "@/constants/serviceName";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { PatientDataType } from "@/models/patientData";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import ConfigUtil from "@/utility/configUtil";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        OrganDonorDetailsCard,
    },
};

@Component(options)
export default class ServicesView extends Vue {
    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("isPatientDataLoading", { namespace: "patientData" })
    isPatientDataLoading!: (hdid: string) => boolean;

    @Action("retrievePatientData", { namespace: "patientData" })
    retrievePatientData!: (params: {
        hdid: string;
        patientDataType: PatientDataType;
    }) => Promise<void>;

    logger!: ILogger;

    breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Services",
            to: "/services",
            active: true,
            dataTestId: "breadcrumb-services",
        },
    ];

    get isLoading(): boolean {
        return this.isPatientDataLoading(this.user.hdid);
    }

    get hdid(): string {
        return this.user.hdid;
    }

    get isOrganDonorServiceEnabled(): boolean {
        return ConfigUtil.isServiceEnabled(ServiceName.OrganDonorRegistration);
    }

    async created(): Promise<void> {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        if (this.isOrganDonorServiceEnabled) {
            await this.retrievePatientData({
                hdid: this.user.hdid,
                patientDataType: PatientDataType.OrganDonorRegistrationStatus,
            });
        }
    }
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <page-title title="Services" />
        <p>
            You can check and update your Organ Donor Registry information here.
            More health services will be added in future.
        </p>
        <div>
            <b-row cols="1" cols-lg="2" cols-xl="3">
                <b-col
                    v-if="!isLoading && isOrganDonorServiceEnabled"
                    class="p-3"
                >
                    <OrganDonorDetailsCard :hdid="hdid" />
                </b-col>
            </b-row>
        </div>
    </div>
</template>
