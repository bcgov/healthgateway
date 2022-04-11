<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";

import Communication, { CommunicationType } from "@/models/communication";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ICommunicationService, ILogger } from "@/services/interfaces";

@Component
export default class CommunicationComponent extends Vue {
    private logger!: ILogger;
    private bannerCommunication: Communication | null = null;
    private inAppCommunication: Communication | null = null;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("userIsRegistered", { namespace: "user" })
    userIsRegistered!: boolean;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;

    @Getter("isOffline", { namespace: "config" })
    isOffline!: boolean;

    private get isInApp(): boolean {
        return (
            this.oidcIsAuthenticated &&
            this.userIsRegistered &&
            this.isValidIdentityProvider &&
            !this.isOffline
        );
    }

    private get hasCommunication(): boolean {
        if (this.isInApp) {
            return this.inAppCommunication != null;
        } else {
            return this.bannerCommunication != null;
        }
    }

    private get text(): string {
        if (this.isInApp) {
            return this.inAppCommunication ? this.inAppCommunication.text : "";
        } else {
            return this.bannerCommunication
                ? this.bannerCommunication.text
                : "";
        }
    }

    @Watch("$route.path")
    private onRouteChange() {
        this.fetchCommunication(CommunicationType.InApp);
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private mounted() {
        this.fetchCommunication(CommunicationType.Banner);
        this.fetchCommunication(CommunicationType.InApp);
    }

    private fetchCommunication(type: CommunicationType) {
        const communicationService: ICommunicationService =
            container.get<ICommunicationService>(
                SERVICE_IDENTIFIER.CommunicationService
            );

        communicationService
            .getActive(type)
            .then((requestResult) => {
                if (type === CommunicationType.Banner) {
                    this.bannerCommunication = requestResult.resourcePayload;
                } else {
                    this.inAppCommunication = requestResult.resourcePayload;
                }
            })
            .catch((err) => {
                this.logger.error(JSON.stringify(err));
            });
    }
}
</script>

<template>
    <b-row v-if="hasCommunication">
        <b-col class="p-0 m-0">
            <div
                data-testid="communicationBanner"
                class="text-center communication p-2"
                v-html="text"
            />
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.communication {
    background-color: $bcgold;
    color: black;

    :last-child {
        margin-bottom: 0;
    }
}
</style>
