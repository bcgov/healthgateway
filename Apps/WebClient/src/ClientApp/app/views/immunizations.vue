<template>
  <div>
    <h1 id="subject">
      <span class="fa fa-1x fa-syringe"></span>
      &nbsp;{{ $t('immz-component.immunizations')}}
    </h1>
    <p id="subtext">{{ $t('immz-component.prototype')}}</p>

    <b-table striped responsive small :items="items" :fields="fields">
      <template slot="show_details" slot-scope="row">
        <b-button
          size="sm"
          variant="outline-info"
          @click="row.toggleDetails"
          class="pb-2"
        >{{ row.detailsShowing ? 'Hide' : 'Show'}} Details</b-button>
      </template>
      <template slot="row-details" id="rd" slot-scope="row">
        <b-card>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.lot') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.lot }}</b-col>

            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.site') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.site }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.dose') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.dose }}</b-col>
          </b-row>
          <b-button size="sm" variant="outline-secondary" @click="row.toggleDetails">Hide Details</b-button>
        </b-card>
      </template>
    </b-table>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { IVueI18n } from "vue-i18n";

interface Immunization {
  date: Date;
  vaccine: string;
  lot: string;
  dose: string;
  booster: string;
  site: string;
}

@Component
export default class ImmunizationsComponent extends Vue {
  private sortyBy: string = "date";
  private sortDesc: boolean = false;

  private fields = {
    date: { sortable: true },
    vaccine: { sortable: true },
    boost: { sortable: true },
    show_details: { sortable: false }
  };

  private items = [
    {
      date: "1999 Jun 10",
      vaccine:
        "Diphtheria, Tetanus, Pertussis, Hepatitis B, Polio, Haemophilus Influenzae type b (DTaP-HB-IPV-Hib)",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "4792AB",
      boost: "1999 Aug 10"
    },
    {
      date: "1999 Aug 14",
      vaccine: "DTaP-HB-IPV-Hib",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "8793BC",
      boost: "1999 Oct 15"
    },
    {
      date: "1999 Oct 28",
      vaccine: "DTaP-HB-IPV-Hib",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "93435DD",
      boost: ""
    },
    {
      date: "2000 Apr 14",
      vaccine: "Chickenpox (Varicella)",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "99693AA",
      boost: ""
    },
    {
      date: "2000 Apr 23",
      vaccine: "Measles, Mumps, Rubella (MMR)",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "100330AA",
      boost: ""
    },
    {
      date: "2000 Oct 30",
      vaccine: "DTaP-IPV-Hib",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "103234AB",
      boost: ""
    },
    {
      date: "2000 Jul 11",
      vaccine: "Influenza, inactivated (Flu)",
      dose: "0.25 mL",
      site: "left deltoid",
      lot: "990093FA",
      boost: ""
    },
    {
      date: "2003 Sep 11",
      vaccine: "Measles, Mumps, Rubella, Varicella  (MMRV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "880899AA",
      boost: ""
    },
    {
      date: "2003 Sep 11",
      vaccine: "Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "778099DT",
      boost: "2013 Sep 11 (Td)"
    },
    {
      date: "2011 Sep 22",
      vaccine: "Human Papilomavirus (HPV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "123450AA",
      boost: ""
    },
    {
      date: "2013 Nov 2",
      vaccine: "Tetanus, Diphtheria (Td)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "440319DC",
      boost: ""
    },
    {
      date: "2014 Sep 9",
      vaccine: "Meningococcal Quadrivalent",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "909102CZ",
      boost: ""
    },
    {
      date: "2014 Oct 2",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "239941RA",
      boost: ""
    },
    {
      date: "2015 Oct 24",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "503459AB",
      boost: ""
    },
    {
      date: "2016 Jul 1",
      vaccine: "Tetanus, Diphtheria (Td)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "440319DC",
      boost: ""
    },
    {
      date: "2017 Nov 2",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "right deltoid",
      lot: "100399AC",
      boost: ""
    },
    {
      date: "2018 Oct 30",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "845003BB",
      boost: ""
    }
  ];
}
</script>