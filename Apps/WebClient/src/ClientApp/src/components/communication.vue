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
        <span>{{ communicationText }}</span>
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
  private communicationText: string = "";

  private mounted() {
    this.fetchCommunication();
  }

  private get hasCommunication(): boolean {
    return !!this.communicationText;
  }

  private fetchCommunication() {
    const communicationService: ICommunicationService = container.get(
      SERVICE_IDENTIFIER.CommunicationService
    );
    communicationService
      .getActive()
      .then((requestResult) => {
        this.communicationText = requestResult.resourcePayload?.text;
      })
      .catch((err) => {
        console.log(err);
      });
  }
}
</script>
