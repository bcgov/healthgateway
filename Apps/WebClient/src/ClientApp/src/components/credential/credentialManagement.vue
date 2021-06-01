<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import { ImmunizationEvent } from "@/models/immunizationModel";

import CredentialCollectionCard from "./credentialCollectionCard.vue";
import CredentialList from "./credentialList.vue";

@Component({
    components: {
        "credential-collection-card": CredentialCollectionCard,
        "credential-list": CredentialList,
    },
})
export default class CredentialManagementView extends Vue {
    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    private get pendingCredentials(): string[] {
        return this.covidImmunizations.map((i) => i.id);
    }
}
</script>

<template>
    <div>
        <page-title title="Credentials" />
        <b-row>
            <b-col lg="4">
                <p>
                    In efforts to provide British Columbians with required
                    credentials by sd0ejlkfs, you can access your
                    <strong>covid immunization and tests</strong> here and store
                    them in a digital wallet to use as proof if required.
                </p>
                <p>
                    A <strong>smart phone or tablet</strong> is required to
                    securely complete this process and store your credentials.
                </p>
                <p>
                    If you have any questions please
                    <strong>contact us</strong> at email@email.com
                </p>
            </b-col>
            <b-col lg="8">
                <credential-collection-card
                    :pending-credentials="pendingCredentials"
                />
            </b-col>
        </b-row>
        <credential-list />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
