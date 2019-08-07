<template>
    <div>
        <h1 id="subject">
            <span class="fa fa-1x fa-syringe"></span>
            &nbsp;{{ $t('immz-component.immunizations')}}
        </h1>
        <p id="subtext">{{ $t('immz-component.prototype')}}</p>

        <b-table striped responsive small :items="items" :fields="fields">
            <template slot="HEAD_date" id="f1">
                {{ $t('immz-component.fields.date') }}
            </template>
            <template slot="HEAD_vaccine" id="f2">
                {{ $t('immz-component.fields.vaccine') }}
            </template>
            <template slot="HEAD_dose" id="f3">
                {{ $t('immz-component.fields.dose') }}
            </template>
            <template slot="HEAD_site" id="f4">
                {{ $t('immz-component.fields.site') }}
            </template>
            <template slot="HEAD_lot" id="f5">
                {{ $t('immz-component.fields.lot') }}
            </template>
            <template slot="HEAD_boost" id="f6">
                {{ $t('immz-component.fields.boost') }}
            </template>
        </b-table>
    </div>
</template>

<script lang="ts">
    import Vue from "vue";
    import { Component } from "vue-property-decorator";
    import { State, Action, Getter } from "vuex-class";
    const namespace: string = "imms";

    @Component
    export default class ImmunizationsComponent extends Vue {
        @Action("getitems", { namespace }) getItems: any;

        private sortyBy: string = "date";
        private sortDesc: boolean = false;
        private items: boolean = false;

        private fields = {
            date: { sortable: true },
            vaccine: { sortable: true },
            dose: { sortable: false },
            site: { sortable: true },
            lot: { sortable: true },
            boost: { sortable: true }
        };

        mounted() {
        }

    }
</script>