<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

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

    private get isConnectionConnected(): boolean {
        return this.connection?.status === ConnectionStatus.Connected;
    }
}
</script>

<template>
    <div>
        <page-title title="Credentials" />
        <credential-collection-card />
        <credential-list v-if="isConnectionConnected" class="mt-3" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
