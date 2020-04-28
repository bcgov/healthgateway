<template>
  <b-row v-if="communication">
    <b-col class="p-0">
      <b-alert :show="true" variant="warning" class="m-0 text-center">
        <h5>{{ communication.text }}</h5>
      </b-alert>
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
<<<<<<< HEAD
  private communication: Communication | null = null;
=======
  private communication: Communication = null;
>>>>>>> dev

  private mounted() {
    this.fetchCommunication();
  }

  private fetchCommunication() {
    const communicationService: ICommunicationService = container.get(
      SERVICE_IDENTIFIER.CommunicationService
    );
    communicationService
      .getActive()
      .then(requestResult => {
        this.communication = requestResult.resourcePayload;
      })
      .catch(err => {
        console.log(err);
      });
  }
}
</script>
