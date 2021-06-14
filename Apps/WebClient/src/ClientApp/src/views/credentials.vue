<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import CredentialInstructionsView from "@/components/credential/credentialInstructions.vue";
import CredentialManagementView from "@/components/credential/credentialManagement.vue";
import LoadingComponent from "@/components/loading.vue";
import ResourceCentreComponent from "@/components/resourceCentre.vue";
import { ImmunizationEvent } from "@/models/immunizationModel";
import User from "@/models/user";
import { WalletConnection } from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        LoadingComponent,
        "credential-instructions": CredentialInstructionsView,
        "credential-management": CredentialManagementView,
        "resource-centre": ResourceCentreComponent,
    },
})
export default class CredentialsView extends Vue {
    @Getter("user", { namespace: "user" })
    private user!: User;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("connection", { namespace: "credential" })
    connection!: WalletConnection | undefined;

    @Getter("isLoading", { namespace: "user" })
    isPatientLoading!: boolean;

    @Getter("isLoading", { namespace: "immunization" })
    isImmunizationLoading!: boolean;

    @Action("getPatientData", { namespace: "user" })
    getPatientData!: () => Promise<void>;

    @Action("retrieve", { namespace: "immunization" })
    retrieveImmunizations!: (params: { hdid: string }) => Promise<void>;

    @Action("retrieveConnection", { namespace: "credential" })
    retrieveConnection!: (params: { hdid: string }) => Promise<boolean>;

    private get hasCredentialConnection(): boolean {
        return this.connection !== undefined && this.connection !== null;
    }

    private get hasCovidImmunizations(): boolean {
        return this.covidImmunizations.length > 0;
    }

    private get isLoading(): boolean {
        return this.isPatientLoading || this.isImmunizationLoading;
    }

    private logger!: ILogger;

    private acknowledgedInstructions = false;

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.getPatientData().catch((err) => {
            this.logger.error(`Error loading patient data: ${err}`);
        });

        Promise.all([
            this.retrieveImmunizations({ hdid: this.user.hdid }),
            this.retrieveConnection({ hdid: this.user.hdid }),
        ]).catch((err) => {
            this.logger.error(`Error loading credential data: ${err}`);
        });
    }

    private acknowledgeInstructions() {
        this.acknowledgedInstructions = true;
    }
}
</script>

<template>
    <div class="m-3 flex-grow-1 d-flex flex-column">
        <b-row>
            <b-col class="col-12 col-xl-9 column-wrapper">
                <credential-instructions
                    v-if="!hasCredentialConnection && !acknowledgedInstructions"
                    :has-covid-immunizations="hasCovidImmunizations"
                    :is-loading="isLoading"
                    data-testid="credentialInstructions"
                    @started="acknowledgeInstructions"
                />
                <credential-management
                    v-else
                    data-testid="credentialManagement"
                />
            </b-col>
        </b-row>
        <resource-centre />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
