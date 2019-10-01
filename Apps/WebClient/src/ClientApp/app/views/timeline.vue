<template>
  <div>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-alert :show="hasErrors" dismissible variant="danger">
      <h4>Error</h4>
      <span>An unexpected error occured while processing the request.</span>
    </b-alert>
    <h1 id="subject">
      Health Care Timeline: TODO
    </h1>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import { IMedicationService } from "@/services/interfaces";
import LoadingComponent from "@/components/loading.vue";
import container from "@/inversify.config";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import User from "@/models/user";
import Prescription from "@/models/prescription";

const namespace: string = "user";

@Component({
  components: {
    LoadingComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;
  private prescriptions: Prescription[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = false;

  mounted() {
    this.isLoading = true;
    const medicationService: IMedicationService = container.get(
      SERVICE_IDENTIFIER.MedicationService
    );

    medicationService
      .getPatientPrescriptions(this.user.hdid)
      .then(results => {
        this.prescriptions = results;
      })
      .catch(err => {
        this.hasErrors = true;
      })
      .finally(() => {
        this.isLoading = false;
      });
  }
}
</script>
