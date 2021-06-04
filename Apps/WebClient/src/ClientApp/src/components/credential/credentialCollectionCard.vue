<script lang="ts">
import QRCode from "qrcode";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import User from "@/models/user";
import {
    ConnectionStatus,
    CredentialStatus,
    WalletConnection,
    WalletCredential,
} from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

@Component({
    components: {
        "message-modal": MessageModalComponent,
    },
})
export default class CredentialCollectionCard extends Vue {
    @Getter("user", { namespace: "user" })
    private user!: User;

    @Getter("connection", { namespace: "credential" })
    connection!: WalletConnection | undefined;

    @Getter("credentials", { namespace: "credential" })
    credentials!: WalletCredential[];

    @Action("createConnection", { namespace: "credential" })
    createConnection!: (params: { hdid: string }) => Promise<boolean>;

    @Action("retrieveConnection", { namespace: "credential" })
    retrieveConnection!: (params: { hdid: string }) => Promise<boolean>;

    @Watch("connection")
    private updateQrCode(): void {
        const data = this.connection?.invitationEndpoint || "";
        if (data.length === 0) {
            this.qrCodeDataUrl = null;
        }
        QRCode.toDataURL(data, {}, (err: unknown, url: string) => {
            if (err) {
                this.logger.error(`Error generating QR Code: ${err}`);
                this.qrCodeDataUrl = null;
            } else {
                this.qrCodeDataUrl = url;
            }
        });
    }

    private get addedCredentials(): WalletCredential[] {
        return this.credentials.filter(
            (c) => c.status === CredentialStatus.Added
        );
    }

    private get connectionStatusLabel(): string {
        if (this.connection === undefined || this.connection === null) {
            return "Not Connected";
        }
        switch (this.connection.status) {
            case ConnectionStatus.Pending:
                return "Pending";
            case ConnectionStatus.Connected:
                return "Connected";
            case ConnectionStatus.Disconnected:
                return "Disconnected";
            default:
                return "Unknown";
        }
    }

    private get connectionStatusVariant(): string | undefined {
        if (this.connection?.status === ConnectionStatus.Connected) {
            return "success";
        }
        return undefined;
    }

    private get isConnectionUndefined(): boolean {
        return this.connection === undefined || this.connection === null;
    }

    private get isConnectionPending(): boolean {
        return this.connection?.status === ConnectionStatus.Pending;
    }

    private get isConnectionConnected(): boolean {
        return this.connection?.status === ConnectionStatus.Connected;
    }

    private get isConnectionDisconnected(): boolean {
        return this.connection?.status === ConnectionStatus.Disconnected;
    }

    private logger!: ILogger;

    private qrCodeDataUrl: string | null = null;

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.updateQrCode();
    }

    private handleCreateConnection(): void {
        this.createConnection({
            hdid: this.user.hdid,
        }).catch((err) => {
            this.logger.error(`Error creating connection: ${err}`);
        });
    }

    private refreshConnection(): void {
        this.retrieveConnection({
            hdid: this.user.hdid,
        }).catch((err) => {
            this.logger.error(`Error retrieving connection: ${err}`);
        });
    }
}
</script>

<template>
    <div>
        <b-card>
            <b-card-title>Wallet Connection</b-card-title>
            <b-card-text>
                <status-label
                    heading="Connection Status"
                    :variant="connectionStatusVariant"
                    :status="connectionStatusLabel"
                    data-testid="connectionStatusLabel"
                />
                <div v-if="isConnectionConnected" class="mt-2 text-muted">
                    Credentials in Wallet: {{ addedCredentials.length }}
                </div>
                <div
                    v-else-if="isConnectionPending"
                    class="mx-auto mt-3 text-center"
                >
                    <div class="mb-3">
                        <div class="mb-3">
                            If you're on a mobile device, click this link to
                            initiate the connection in your wallet app:
                        </div>
                        <hg-button
                            variant="link"
                            data-testid="mobileConnectCredentialsButton"
                            :href="connection.invitationEndpoint"
                            :disabled="connection.invitationEndpoint === null"
                            class="mb-3"
                        >
                            Connect
                        </hg-button>
                    </div>
                    <div class="d-none d-md-block mb-3">
                        <div class="mb-3">
                            Otherwise, open the wallet app on your mobile device
                            and scan this QR code:
                        </div>
                        <img
                            v-if="qrCodeDataUrl !== null"
                            :src="qrCodeDataUrl"
                            data-testid="qrCodeImage"
                            class="mb-3"
                        />
                    </div>
                    <div class="mb-3">
                        In the wallet app, you should see a connection request
                        from Health Gateway. Accept the request to establish the
                        connection, then click the button below.
                    </div>
                </div>
            </b-card-text>
            <template v-if="!isConnectionConnected" #footer>
                <div class="text-center">
                    <hg-button
                        v-if="isConnectionUndefined || isConnectionDisconnected"
                        variant="primary"
                        data-testid="createConnectionButton"
                        @click="handleCreateConnection"
                    >
                        Create Connection
                    </hg-button>
                    <hg-button
                        v-if="isConnectionPending"
                        variant="primary"
                        data-testid="confirmConnectionEstablishedButton"
                        @click="refreshConnection"
                    >
                        Continue
                    </hg-button>
                </div>
            </template>
        </b-card>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
