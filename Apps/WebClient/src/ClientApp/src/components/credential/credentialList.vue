<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEllipsisV, faSyringe } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import User from "@/models/user";
import { CredentialStatus, WalletCredential } from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

library.add(faSyringe, faEllipsisV);

interface CredentialEntry {
    id: string;
    event: ImmunizationEvent;
    credential: WalletCredential | undefined;
}

@Component({
    components: {
        "message-modal": MessageModalComponent,
    },
})
export default class CredentialList extends Vue {
    @Getter("user", { namespace: "user" })
    private user!: User;

    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("credentials", { namespace: "credential" })
    credentials!: WalletCredential[];

    @Getter("isLoading", { namespace: "credential" })
    isLoading!: boolean;

    @Action("createCredential", { namespace: "credential" })
    createCredential!: (params: {
        hdid: string;
        targetId: string;
    }) => Promise<boolean>;

    @Action("revokeCredential", { namespace: "credential" })
    revokeCredential!: (params: {
        hdid: string;
        credentialId: string;
    }) => Promise<boolean>;

    private logger!: ILogger;

    private get entries(): CredentialEntry[] {
        return this.covidImmunizations.map((i) => ({
            id: i.id,
            event: i,
            credential: this.credentials.find(
                (c) =>
                    c.sourceId === i.id && c.status !== CredentialStatus.Revoked
            ),
        }));
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private formatDate(date: string): string {
        return DateWrapper.format(date);
    }

    private productString(event: ImmunizationEvent): string {
        const products = event.immunization?.immunizationAgents.map(
            (a) => a.productName
        );
        if (products) {
            return "Product: " + products.join(", ");
        }
        return "";
    }

    private statusLabel(status: CredentialStatus): string | undefined {
        switch (status) {
            case CredentialStatus.Created:
                return "Created";
            case CredentialStatus.Added:
                return "In Wallet";
            default:
                return undefined;
        }
    }

    private statusClasses(status: CredentialStatus): unknown {
        if (status === CredentialStatus.Added) {
            return { "text-success": true };
        }
        return { "text-muted": true };
    }

    private isCredentialInWallet(credential: WalletCredential): boolean {
        return credential?.status === CredentialStatus.Added;
    }

    private isMenuDisabled(credential: WalletCredential): boolean {
        return (
            (credential.status !== CredentialStatus.Created &&
                credential.status !== CredentialStatus.Added) ||
            this.isLoading
        );
    }

    private handleCreateCredential(targetId: string): void {
        this.createCredential({
            hdid: this.user.hdid,
            targetId: targetId,
        }).catch((err) => {
            this.logger.error(`Error creating credential: ${err}`);
        });
    }

    private handleRevokeCredential(credentialId: string): void {
        this.revokeCredential({
            hdid: this.user.hdid,
            credentialId: credentialId,
        }).catch((err) => {
            this.logger.error(`Error revoking credential: ${err}`);
        });
    }

    private handleReissueCredential(credential: WalletCredential): void {
        this.revokeCredential({
            hdid: this.user.hdid,
            credentialId: credential.credentialId,
        })
            .then((result) => {
                if (result === true) {
                    return this.createCredential({
                        hdid: this.user.hdid,
                        targetId: credential.sourceId,
                    }).catch((err) => {
                        this.logger.error(`Error creating credential: ${err}`);
                    });
                }
            })
            .catch((err) => {
                this.logger.error(`Error revoking credential: ${err}`);
            });
    }
}
</script>

<template>
    <div>
        <b-row
            v-for="entry in entries"
            :key="entry.id"
            class="cardWrapper mb-1"
        >
            <b-col class="ml-0 ml-md-2">
                <b-row class="entryHeading p-3 align-items-center">
                    <b-col class="col-auto">
                        <div
                            class="icon"
                            :class="{
                                'bg-success': isCredentialInWallet(
                                    entry.credential
                                ),
                                'bg-secondary': !isCredentialInWallet(
                                    entry.credential
                                ),
                            }"
                        >
                            <hg-icon icon="syringe" size="large" fixed-width />
                        </div>
                    </b-col>
                    <b-col class="mx-3">
                        <b-row>
                            <b-col>
                                <strong
                                    data-testid="credentialTitle"
                                    :class="{
                                        'text-muted': !isCredentialInWallet(
                                            entry.credential
                                        ),
                                    }"
                                >
                                    {{ entry.event.targetedDisease }}
                                </strong>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="py-1 text-muted align-self-center">
                                <slot
                                    name="header-description"
                                    data-testid="credentialProduct"
                                >
                                    {{ productString(entry.event) }}
                                </slot>
                            </b-col>
                        </b-row>
                        <b-row>
                            <b-col class="text-muted align-self-center">
                                <slot name="header-description">
                                    <small
                                        data-testid="credentialImmunizationDate"
                                    >
                                        {{
                                            formatDate(
                                                entry.event.dateOfImmunization
                                            )
                                        }}
                                    </small>
                                </slot>
                            </b-col>
                        </b-row>
                    </b-col>
                    <b-col class="col-auto">
                        <hg-button
                            v-if="entry.credential === undefined"
                            data-testid="addCredentialButton"
                            variant="primary"
                            @click="handleCreateCredential(entry.id)"
                        >
                            Add
                        </hg-button>
                        <strong
                            v-else
                            data-testid="credentialStatus"
                            :class="statusClasses(entry.credential.status)"
                        >
                            {{ statusLabel(entry.credential.status) }}
                        </strong>
                    </b-col>
                    <b-col cols="auto" class="pl-3">
                        <b-navbar-nav v-if="entry.credential !== undefined">
                            <b-nav-item-dropdown
                                right
                                text=""
                                :no-caret="true"
                                :disabled="isMenuDisabled(entry.credential)"
                            >
                                <!-- Using 'button-content' slot -->
                                <template slot="button-content">
                                    <hg-icon
                                        icon="ellipsis-v"
                                        size="small"
                                        data-testid="credentialMenuBtn"
                                        class="credentialMenu"
                                    />
                                </template>
                                <b-dropdown-item
                                    data-testid="revokeCredentialMenuBtn"
                                    class="menuItem"
                                    @click.stop="
                                        handleRevokeCredential(
                                            entry.credential.credentialId
                                        )
                                    "
                                >
                                    Revoke
                                </b-dropdown-item>
                                <b-dropdown-item
                                    data-testid="reissueCredentialMenuBtn"
                                    class="menuItem"
                                    @click.stop="
                                        handleReissueCredential(
                                            entry.credential
                                        )
                                    "
                                >
                                    Reissue
                                </b-dropdown-item>
                            </b-nav-item-dropdown>
                        </b-navbar-nav>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

div[class^="col"],
div[class*=" col"] {
    padding: 0px;
    margin: 0px;
}

div[class^="row"],
div[class*=" row"] {
    padding: 0px;
    margin: 0px;
}

.cardWrapper {
    width: 100%;
    min-width: 100%;
}

.entryHeading {
    background-color: $soft_background;
}

.icon {
    color: white;
    text-align: center;
    border-radius: 50%;
    height: 60px;
    width: 60px;
    padding-top: 17px;
}

.credentialMenu {
    color: $soft_text;
}
</style>
