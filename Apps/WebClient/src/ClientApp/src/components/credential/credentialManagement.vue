<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";
import { ConnectionStatus, WalletConnection } from "@/models/wallet";

import CredentialCollectionCard from "./credentialCollectionCard.vue";
import CredentialList from "./credentialList.vue";

@Component({
    components: {
        "credential-collection-card": CredentialCollectionCard,
        "credential-list": CredentialList,
    },
})
export default class CredentialManagementView extends Vue {
    @Getter("connection", { namespace: "credential" })
    connection!: WalletConnection | undefined;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private get isConnectionConnected(): boolean {
        return this.connection?.status === ConnectionStatus.Connected;
    }

    private get verifierUrl(): string {
        return this.config.externalURLs["CredentialVerification"];
    }
}
</script>

<template>
    <div>
        <page-title title="Credentials" />
        <credential-collection-card />
        <credential-list v-if="isConnectionConnected" class="mt-3" />
        <b-row v-if="isConnectionConnected" class="mt-3"
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
