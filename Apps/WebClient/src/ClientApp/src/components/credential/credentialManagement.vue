<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import LoadingComponent from "@/components/loading.vue";
import type { WebClientConfiguration } from "@/models/configData";
import {
    ConnectionStatus,
    CredentialStatus,
    WalletConnection,
} from "@/models/wallet";

import CredentialCollectionCard from "./credentialCollectionCard.vue";
import CredentialList from "./credentialList.vue";

@Component({
    components: {
        LoadingComponent,
        "credential-collection-card": CredentialCollectionCard,
        "credential-list": CredentialList,
    },
})
export default class CredentialManagementView extends Vue {
    @Getter("connection", { namespace: "credential" })
    connection!: WalletConnection | undefined;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("isLoading", { namespace: "credential" })
    isLoading!: boolean;

    private get isConnectionConnected(): boolean {
        return this.connection?.status === ConnectionStatus.Connected;
    }

    private get verifierUrl(): string {
        return this.config.externalURLs["CredentialVerification"];
    }

    private get hasAddedCredential(): boolean {
        const credentials = this.connection?.credentials ?? [];
        return credentials.some((c) => c.status === CredentialStatus.Added);
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <page-title title="Credentials" />
        <credential-collection-card data-testid="credentialCollectionCard" />
        <credential-list
            v-if="isConnectionConnected"
            class="mt-3"
            data-testid="credentialList"
        />
        <b-row
            v-if="isConnectionConnected && hasAddedCredential"
            class="mt-3"
            data-testid="credentialVerification"
            ><b-col class="text-center">
                <div class="my-3">
                    Test your credentials to see how verification would work
                </div>
                <div>
                    <hg-button
                        variant="secondary"
                        :href="verifierUrl"
                        target="blank_"
                        >Test my Credentials</hg-button
                    >
                </div>
            </b-col></b-row
        >
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
