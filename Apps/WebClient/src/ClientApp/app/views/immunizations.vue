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
    <p id="subtext">{{ $t("immz-component.prototype") }}</p>

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
    </b-table>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import ImmsData from "@/models/immsData";
import LoadingComponent from "@/components/loading.vue";

const namespace: string = "imms";

@Component({
  components: {
    LoadingComponent
  }
})
export default class ImmunizationsComponent extends Vue {
  @Action("getitems", { namespace })
  private getitems!: any;
  @Getter("items", { namespace })
  private items!: ImmsData[];
  @Getter("isLoading", { namespace })
  private isLoading!: boolean;
  @Getter("hasErrors", { namespace })
  private hasErrors!: boolean;

  private sortyBy: string = "date";
  private sortDesc: boolean = false;

  private fields = {
    date: { sortable: true },
    vaccine: { sortable: true },
    dose: { sortable: false },
    site: { sortable: true },
    lot: { sortable: true },
    boost: { sortable: true }
  };

  mounted() {
    this.getitems();
  }
}
</script>
