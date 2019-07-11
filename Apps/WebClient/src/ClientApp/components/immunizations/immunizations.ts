import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { IVueI18n } from 'vue-i18n';



interface Immunization {
    date: string;
    vaccine: string;
    lot: string;
    dose: string;
    booster: string;
    site: string;
}

@Component
export default class ImmunizationsComponent extends Vue {
    data() {
        return {
            /*
            fields: [
              { key: 'date', sortable: true},
              { key: 'vaccine', label: 'Vaxene', sortable: true },
              { key: 'dose', label: 'DOSE', sortable: false },
              { key: 'site', label: 'Site', sortable: true },
              { key: 'lot', label: 'LOOT', sortable: true },
              { key: 'boost', label: 'BOOSTER', sortable: true }
            ], */
            items: [
                {
                    date: '1999 Jun 10',
                    vaccine: 'DTaP-HB-IPV-Hib',
                    dose: '1',
                    site: 'left vastus lateralis',
                    lot: '4792AB',
                    boost: '1999 Aug 10'
                },
                {
                    date: '1999 Aug 14',
                    vaccine: 'DTaP-HB-IPV-Hib',
                    dose: '2',
                    site: 'left vastus lateralis',
                    lot: '8793BC',
                    boost: '1999 Oct 15'
                },
                {
                    date: '1999 Aug 14',
                    vaccine: 'DTaP-HB-IPV-Hib',
                    dose: '',
                    site: 'left vastus lateralis',
                    lot: '8793BC',
                    boost: '1999 Oct 15'
                }
            ]
        }
    }
}
