<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faSyringe } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

import MessageModalComponent from "@/components/modal/genericMessage.vue";
import { DateWrapper } from "@/models/dateWrapper";
import { ImmunizationEvent } from "@/models/immunizationModel";
import { CredentialState, WalletCredential } from "@/models/wallet";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

library.add(faSyringe);

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
    @Getter("covidImmunizations", { namespace: "immunization" })
    covidImmunizations!: ImmunizationEvent[];

    @Getter("credentials", { namespace: "credential" })
    credentials!: WalletCredential[];

    private get entries(): CredentialEntry[] {
        return this.covidImmunizations.map((i) => ({
            id: i.id,
            event: i,
            credential: this.credentials.find(
                (c) =>
                    c.sourceId === i.id && c.status !== CredentialState.Revoked
            ),
        }));
    }

    private logger!: ILogger;

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

    private statusLabel(state: CredentialState): string | undefined {
        switch (state) {
            case CredentialState.Created:
                return "Created";
            case CredentialState.Added:
                return "In Wallet";
            default:
                return undefined;
        }
    }

    private statusClasses(state: CredentialState): unknown {
        if (state === CredentialState.Added) {
            return { "text-success": true };
        }
        return { "text-muted": true };
    }
}
</script>

<template>
    <div class="mt-5">
        <b-row
            v-for="entry in entries"
            :key="entry.id"
            class="cardWrapper mb-1"
        >
            <b-col class="ml-0 ml-md-2">
                <b-row class="entryHeading p-3 align-items-center">
                    <b-col class="leftPane">
                        <div class="icon">
                            <hg-icon icon="syringe" size="large" fixed-width />
                        </div>
                    </b-col>
                    <b-col class="entryTitleWrapper">
                        <b-row>
                            <b-col data-testid="credentialTitle">
                                <strong>
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
                    <b-col class="rightPane">
                        <div
                            v-if="entry.credential === undefined"
                            class="text-muted"
                            data-testid="credentialStatus"
                        >
                            Queued
                        </div>
                        <div
                            v-else
                            data-testid="credentialStatus"
                            :class="statusClasses(entry.credential.status)"
                        >
                            {{ statusLabel(entry.credential.status) }}
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
$left-pane-width: 80px;
$right-pane-width: 90px;

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

.leftPane {
    width: $left-pane-width;
    max-width: $left-pane-width;
}

.rightPane {
    text-align: center;
    width: $right-pane-width;
    max-width: $right-pane-width;
}

.entryHeading {
    background-color: $soft_background;
}

.entryTitleWrapper {
    color: $primary;
    text-align: left;
}

.icon {
    background-color: $primary;
    color: white;
    text-align: center;
    border-radius: 50%;
    height: 60px;
    width: 60px;
    padding-top: 17px;
}
</style>
