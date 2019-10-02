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
    <ul id="example-1">
      <!--TODO: The DIN might not be unique. Instead we need to pass the ID of this statement-->
      <li v-for="statement in medicationStatements" :key="statement.DIN">
        Generic Name: {{ statement.genericName }}
      </li>
    </ul>
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
import MedicationStatement from "@/models/medicationStatement";

const namespace: string = "user";

@Component({
  components: {
    LoadingComponent
  }
})
export default class TimelineComponent extends Vue {
  @Getter("user", { namespace }) user: User;
  private medicationStatements: MedicationStatement[] = [];
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
      .getPatientMedicationStatemens(this.user.hdid)
      .then(results => {
        this.medicationStatements = results;
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
