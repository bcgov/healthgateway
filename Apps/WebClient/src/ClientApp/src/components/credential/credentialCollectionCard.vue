<script lang="ts">
import QRCode from "qrcode";
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import User from "@/models/user";
import {
    ConnectionState,
    CredentialState,
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
    @Prop({ required: true }) pendingCredentials!: string[];

    @Getter("user", { namespace: "user" })
    private user!: User;

    @Getter("connection", { namespace: "credential" })
    connection!: WalletConnection | undefined;

    @Getter("credentials", { namespace: "credential" })
    credentials!: WalletCredential[];

    @Action("createConnection", { namespace: "credential" })
    createConnection!: (params: {
        hdid: string;
        targetIds: string[];
    }) => Promise<boolean>;

    @Action("retrieveCredentials", { namespace: "credential" })
    retrieveCredentials!: (params: { hdid: string }) => Promise<boolean>;

    @Watch("connection")
    private updateQrCode(): void {
        const data = this.connection?.qrCode || "";
        if (data.length === 0) {
            this.qrCodeDataUrl = undefined;
        }
        QRCode.toDataURL(data, {}, (err: unknown, url: string) => {
            if (err) {
                this.logger.error(`Error generating QR Code: ${err}`);
                this.qrCodeDataUrl = undefined;
            } else {
                this.qrCodeDataUrl = url;
            }
        });
    }

    private get createdCredentials(): WalletCredential[] {
        return this.credentials.filter(
            (c) => c.status === CredentialState.Created
        );
    }

    private get addedCredentials(): WalletCredential[] {
        return this.credentials.filter(
            (c) => c.status === CredentialState.Added
        );
    }

    private get connectionStatusLabel(): string {
        if (this.connection === undefined) {
            return "Not Connected";
        }
        switch (this.connection.state) {
            case ConnectionState.Pending:
                return "Pending";
            case ConnectionState.Connected:
                return "Connected";
            case ConnectionState.Disconnected:
                return "Not Connected";
            default:
                return "Unknown";
        }
    }

    private get connectionStatusVariant(): string | undefined {
        if (this.connection?.state === ConnectionState.Connected) {
            return "success";
        }
        return undefined;
    }

    private get isConnectionUndefined(): boolean {
        return this.connection === undefined;
    }

    private get isConnectionPending(): boolean {
        return this.connection?.state === ConnectionState.Pending;
    }

    private get isConnectionConnected(): boolean {
        return this.connection?.state === ConnectionState.Connected;
    }

    private get isConnectionDisconnected(): boolean {
        return this.connection?.state === ConnectionState.Disconnected;
    }

    private logger!: ILogger;

    private qrCodeDataUrl: string | undefined = undefined;

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private createCredentials(): void {
        this.createConnection({
            hdid: this.user.hdid,
            targetIds: this.pendingCredentials,
        })
            .then(() => this.retrieveCredentials({ hdid: this.user.hdid }))
            .catch((err) => {
                this.logger.error(`Error loading patient data: ${err}`);
            });
    }
}
</script>

<template>
    <div>
        <b-card>
            <b-card-body>
                <b-card-title>Credential Collection</b-card-title>
                <b-card-sub-title class="mb-4">
                    <status-label
                        heading="Connection Status"
                        :variant="connectionStatusVariant"
                        :status="connectionStatusLabel"
                        data-testid="connectionStatusLabel"
                    />
                </b-card-sub-title>
                <b-card-text>
                    <b-row>
                        <b-col md="6">
                            <h5>How can I use this?</h5>
                            <p>
                                1-Porttitor facilisi quis in pulvinar.
                                Suspendisse donec lorem sed tortor gravida
                                imperdiet
                            </p>
                            <p>
                                2-auctor facilisis nunc. Vel lectus sit viverra
                                turpis tristique. Quis eu, a sit mattis ipsum,
                                lectus facilisis habitasse lorem.
                            </p>
                        </b-col>
                        <b-col md="6">
                            <b-card class="bg-light">
                                <b-card-text>
                                    <div
                                        v-if="
                                            isConnectionUndefined ||
                                            isConnectionDisconnected
                                        "
                                    >
                                        <b-row class="align-items-center">
                                            <b-col
                                                cols="6"
                                                md="12"
                                                class="text-center pb-md-3"
                                            >
                                                <h5
                                                    data-testid="pendingCredentialsCount"
                                                >
                                                    {{
                                                        pendingCredentials.length
                                                    }}
                                                </h5>
                                                <div>Records Queued</div>
                                            </b-col>
                                            <b-col cols="6" md="12">
                                                <hg-button
                                                    variant="primary"
                                                    data-testid="createCredentialsButton"
                                                    block
                                                    :disabled="
                                                        pendingCredentials.length ===
                                                        0
                                                    "
                                                    @click="createCredentials"
                                                >
                                                    Create Credentials
                                                </hg-button>
                                            </b-col>
                                        </b-row>
                                    </div>
                                    <div v-else-if="isConnectionPending">
                                        <hg-button
                                            variant="primary"
                                            data-testid="mobileConnectCredentialsButton"
                                            block
                                            disabled
                                        >
                                            Connect
                                        </hg-button>
                                        <img
                                            v-if="qrCodeDataUrl !== undefined"
                                            :src="qrCodeDataUrl"
                                            data-testid="qrCodeImage"
                                            class="
                                                d-none d-md-block
                                                mt-3
                                                mx-auto
                                                img-fluid
                                            "
                                        />
                                    </div>
                                    <div v-else-if="isConnectionConnected">
                                        <b-row class="align-items-center">
                                            <b-col
                                                cols="6"
                                                md="12"
                                                class="text-center pb-md-3"
                                            >
                                                <h5
                                                    data-testid="createdCredentialsCount"
                                                >
                                                    {{
                                                        createdCredentials.length
                                                    }}
                                                </h5>
                                                <div>Created</div>
                                            </b-col>
                                            <b-col
                                                cols="6"
                                                md="12"
                                                class="text-center text-success"
                                            >
                                                <h5
                                                    data-testid="addedCredentialsCount"
                                                >
                                                    {{
                                                        addedCredentials.length
                                                    }}
                                                </h5>
                                                <div>In Wallet</div>
                                            </b-col>
                                        </b-row>
                                    </div>
                                </b-card-text>
                            </b-card>
                        </b-col>
                    </b-row>
                </b-card-text>
            </b-card-body>
        </b-card>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
