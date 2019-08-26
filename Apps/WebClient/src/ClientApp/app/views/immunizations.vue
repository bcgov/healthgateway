<template>
  <div>
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <b-alert :show="hasErrors" dismissible variant="danger">
      <h4>Error</h4>
      <span>An unexpected error occured while processing the request.</span>
    </b-alert>
    <h1 id="subject">
      <span class="fa fa-1x fa-syringe"></span>
      &nbsp;{{ $t("immz-component.immunizations") }}
    </h1>
    <p id="subtext" align="right">
      <b>Reference:</b>&nbsp;
      <b-link
        v-bind:href="
          'https://www.healthlinkbc.ca/tools-videos/bc-immunization-schedules'
        "
        target="_blank"
        >BC Immunization Schedules</b-link
      >
    </p>

    <b-table striped responsive small :items="items" :fields="fields">
      <template id="f1" slot="HEAD_date">
        {{ $t("immz-component.fields.date") }}
      </template>
      <template id="f2" slot="HEAD_vaccine">
        {{ $t("immz-component.fields.vaccine") }}
      </template>
      <template id="f3" slot="HEAD_dose">
        {{ $t("immz-component.fields.dose") }}
      </template>
      <template id="f4" slot="HEAD_site">
        {{ $t("immz-component.fields.site") }}
      </template>
      <template id="f5" slot="HEAD_lot">
        {{ $t("immz-component.fields.lot") }}
      </template>
      <template id="f6" slot="HEAD_boost">
        {{ $t("immz-component.fields.boost") }}
      </template>
      <template slot="show_details" slot-scope="row">
        <b-button
          id="btn1"
          size="sm"
          variant="outline-info"
          @click="row.toggleDetails"
          class="pb-2"
          >{{ row.detailsShowing ? "Hide" : "Show" }} Details</b-button
        >
      </template>
      <template slot="row-details" id="rd" slot-scope="row">
        <b-card>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.lot") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.lot }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.site") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.site }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.dose") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.dose }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.route") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.route }}</b-col>
          </b-row>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.manufacturer") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.manufacturer }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.tradeName") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.tradeName }}</b-col>
          </b-row>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.administeredBy") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.administeredBy }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t("immz-component.fields.administeredAt") }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.administeredAt }}</b-col>
          </b-row>
          <b-row>
            <b-col sm="3" class="text-sm-right">
              <b>More Infomation on HealthLinkBC:</b>
            </b-col>
            <b-col sm="6">
              <b-link
                :href="'https://www.healthlinkbc.ca/search/' + row.item.vaccine"
                target="_blank"
                >{{ row.item.vaccine }}</b-link
              >
            </b-col>
          </b-row>
          <b-button
            size="sm"
            variant="outline-secondary"
            @click="row.toggleDetails"
            >Hide Details</b-button
          >
        </b-card>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import ImmsData from "@/models/immsData";
import LoadingComponent from "@/components/loading.vue";
import { IImmsService } from "@/services/interfaces";
import container from "@/inversify.config";
import SERVICE_IDENTIFIER from "@/constants/serviceIdentifiers";
import { ExternalConfiguration } from "@/models/configData";

@Component({
  components: {
    LoadingComponent
  }
})
export default class ImmunizationsComponent extends Vue {
  private items: ImmsData[] = [];
  private isLoading: boolean = false;
  private hasErrors: boolean = false;
  private sortyBy: string = "date";
  private sortDesc: boolean = false;
  private fields = {
    date: { sortable: true },
    vaccine: { sortable: true },
    boost: { sortable: true },
    show_details: { sortable: false }
  };

  mounted() {
    this.isLoading = true;
    const immsService: IImmsService = container.get(
      SERVICE_IDENTIFIER.ImmsService
    );
    immsService
      .getItems()
      .then(results => {
        this.items = results;
        this.isLoading = false;
      })
      .catch(err => {
        this.hasErrors = true;
        this.isLoading = false;
      });
  }
}
</script>
