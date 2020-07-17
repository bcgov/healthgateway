<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.communication {
    background-color: $bcgold;
    color: black;
}
</style>
<template>
    <b-row v-if="hasCommunication">
        <b-col class="p-0">
            <div class="m-0 py-3 text-center communication">
                <span>{{ text }}</span>
            </div>
        </b-col>
    </b-row>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import Communication from "@/models/communication";
import { ICommunicationService } from "@/services/interfaces";

@Component
export default class CommunicationComponent extends Vue {
    private isLoaded: boolean = false;
    private communication!: Communication;

    private mounted() {
        this.fetchCommunication();
    }

    private get hasCommunication(): boolean {
        return this.isLoaded && this.communication != null;
    }

    private get text(): string {
        return this.communication ? this.communication.text : "";
    }

    private fetchCommunication() {
        let self = this;
        const communicationService: ICommunicationService = container.get(
            SERVICE_IDENTIFIER.CommunicationService
        );
        communicationService
            .getActive()
            .then((requestResult) => {
                self.isLoaded = true;
                self.communication = requestResult.resourcePayload;
            })
            .catch((err) => {
                console.log(err);
            });
    }
}
</script>