<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";

import Communication, { CommunicationType } from "@/models/communication";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ICommunicationService, ILogger } from "@/services/interfaces";

@Component
export default class CommunicationComponent extends Vue {
    private logger!: ILogger;
    private bannerCommunication: Communication | null = null;
    private inAppCommunication: Communication | null = null;

    private get hasCommunication(): boolean {
        if (this.$route.path === "/") {
            return this.bannerCommunication != null;
        } else {
            return this.inAppCommunication != null;
        }
    }

    private get text(): string {
        if (this.$route.path === "/") {
            return this.bannerCommunication
                ? this.bannerCommunication.text
                : "";
        } else {
            return this.inAppCommunication ? this.inAppCommunication.text : "";
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
            <div class="text-center communication p-2" v-html="text" />
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
