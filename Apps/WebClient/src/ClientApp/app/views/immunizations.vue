<template>
  <div>
    <h1 id="subject">
      <span class="fa fa-1x fa-syringe"></span>
      &nbsp;{{ $t('immz-component.immunizations')}}
    </h1>
    <p id="subtext" align="right">
      <b>Reference:</b>&nbsp;
      <a
        href="https://www.healthlinkbc.ca/tools-videos/bc-immunization-schedules"
        target="_blank"
      >BC Immunization Schedules</a>
    </p>

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
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.route') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.route }}</b-col>
          </b-row>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.manufacturer') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.manufacturer }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.tradeName') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.tradeName }}</b-col>
          </b-row>
          <b-row class="mb-2">
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.administeredBy') }}:</b>
            </b-col>
            <b-col sm="3">{{ row.item.administeredBy }}</b-col>
            <b-col sm="3" class="text-sm-right">
              <b>{{ $t('immz-component.fields.administeredAt') }}:</b>
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
              >{{ row.item.vaccine}}</b-link>
            </b-col>
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
  dose: string;
  site: string;
  lot: string;
  boost: string;
  tradeName: string;
  manufacturer: string;
  route: string;
  administeredBy: string;
  administeredAt: string;
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
      boost: "1999 Aug 10",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""

    },
    {
      date: "1999 Aug 14",
      vaccine: "DTaP-HB-IPV-Hib",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "8793BC",
      boost: "1999 Oct 15",
      tradeName: "INFANRIX hexa",
      manufacturer: "GlaxoSmithKline Inc.",
      route: "Intramuscular injection",
      administeredAt: "Vancouver Coastal Health Authority",
      administeredBy: "Paediatric Nurse"
    },
    {
      date: "1999 Oct 28",
      vaccine: "DTaP-HB-IPV-Hib",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "93435DD",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2000 Apr 14",
      vaccine: "Chickenpox (Varicella)",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "99693AA",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2000 Apr 23",
      vaccine: "Measles, Mumps, Rubella (MMR)",
      dose: "0.5 mL",
      site: "left vastus lateralis",
      lot: "100330AA",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2000 Oct 30",
      vaccine: "DTaP-IPV-Hib",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "103234AB",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2000 Jul 11",
      vaccine: "Influenza, inactivated (Flu)",
      dose: "0.25 mL",
      site: "left deltoid",
      lot: "990093FA",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2003 Sep 11",
      vaccine: "Measles, Mumps, Rubella, Varicella  (MMRV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "880899AA",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2003 Sep 11",
      vaccine: "Tetanus, Diphtheria, Pertussis, Polio vaccine (Tdap-IPV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "778099DT",
      boost: "2013 Sep 11 (Td)",
      tradeName: "ADACEL®-POLIO",
      manufacturer: "Sanofi Pasteur Limited",
      route: "Intramuscular injection",
      administeredAt: "Vancouver Island Health Authority",
      administeredBy: "Public Health Nurse"
    },
    {
      date: "2011 Sep 22",
      vaccine: "Human Papillomavirus (HPV)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "123450AA",
      boost: "",
      tradeName: "GARDASIL®9",
      manufacturer: "Merck & Co., Inc.",
      route: "Intramuscular injection",
      administeredAt: "Vancouver Island Health Authority",
      administeredBy: "Public Health Nurse"
    },
    {
      date: "2013 Nov 2",
      vaccine: "Tetanus, Diphtheria (Td)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "440319DC",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2014 Sep 9",
      vaccine: "Meningococcal Quadrivalent",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "909102CZ",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2014 Oct 2",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "239941RA",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2015 Oct 24",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "503459AB",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2016 Jul 1",
      vaccine: "Tetanus, Diphtheria (Td)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "440319DC",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2017 Nov 2",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "right deltoid",
      lot: "100399AC",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    },
    {
      date: "2018 Oct 30",
      vaccine: "Influenza (Flu)",
      dose: "0.5 mL",
      site: "left deltoid",
      lot: "845003BB",
      boost: "",
      tradeName: "",
      manufacturer: "",
      route: "",
      administeredAt: "",
      administeredBy: ""
    }
  ];
}
</script>