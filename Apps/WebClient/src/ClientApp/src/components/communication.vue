<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import Communication from "@/models/communication";
import { ILogger, ICommunicationService } from "@/services/interfaces";

@Component
export default class CommunicationComponent extends Vue {
    private logger!: ILogger;
    private isLoaded = false;
    private communication!: Communication;

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.fetchCommunication();
    }

    private get hasCommunication(): boolean {
        return this.isLoaded && this.communication != null;
    }

    private get text(): string {
        return this.communication ? this.communication.text : "";
    }

    private fetchCommunication() {
        const communicationService: ICommunicationService = container.get<
            ICommunicationService
        >(SERVICE_IDENTIFIER.CommunicationService);

        communicationService
            .getActive()
            .then((requestResult) => {
                this.isLoaded = true;
                this.communication = requestResult.resourcePayload;
            })
            .catch((err) => {
                this.logger.error(JSON.stringify(err));
            });
    }
}
</script>

<template>
    <b-row v-if="hasCommunication">
        <b-col class="p-0">
            <div class="m-0 py-0 text-center communication">
                <span v-html="text"></span>
            </div>
        </b-col>
    </b-row>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.communication {
    background-color: $bcgold;
    color: black;
}
</style>
