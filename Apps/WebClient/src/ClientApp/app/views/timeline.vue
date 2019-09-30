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
import LoadingComponent from "@/components/loading.vue";
import container from "@/inversify.config";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";

@Component({
  components: {
    LoadingComponent
  }
})
export default class TimelineComponent extends Vue {
  //private items: ImmsData[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = false;

  mounted() {
    this.isLoading = true;
    // TODO: add the medication service
    /*const immsService: IImmsService = container.get(
      SERVICE_IDENTIFIER.ImmsService
    );*/

    /*immsService
      .getItems()
      .then(results => {
        this.items = results;
        this.isLoading = false;
      })
      .catch(err => {
        this.hasErrors = true;
        this.isLoading = false;
      });*/
      this.isLoading = false;
  }
}
</script>
