import Vue from 'vue';
import { Component } from 'vue-property-decorator';


interface Immunization {
    date: string;
    vaccine: string;
    lot: string;
    dose: string;
    booster: string;
    site: string;
}

@Component({
    components: {
        LocaleComponent: require('../locale/locale.vue.html')
    }
})

@Component
export default class ImmunizationsComponent extends Vue {
    data() {
        return {
            // Immunization
            fields: {
                immz_date: {
                    key: 'date',
                    label: this.$i18n.t('immz-component.fields.date'),
                    sortable: true
                },
                immz_vaccine: {
                    key: 'vaccine',
                    label: this.$i18n.t('immz-component.fields.vaccine'),
                    sortable: true
                },
                immz_dose: {
                    key: 'dose',
                    label: this.$i18n.t('immz-component.fields.dose'),
                    sortable: true
                },
                immz_site: {
                    key: 'site',
                    label: this.$i18n.t('immz-component.fields.site'),
                    sortable: true
                },
                immz_lot: {
                    key: 'lot',
                    label: this.$i18n.t('immz-component.fields.lot'),
                    sortable: true
                },
                immz_boost: {
                    key: 'boost',
                    label: this.$i18n.t('immz-component.fields.boost'),
                    sortable: true
                }
            },
            items: [
                {
                    date: '1999 Jun 10',
                    vaccine: 'DTaP-HB-IPV-Hib',
                    dose: '',
                    site: 'left vastus lateralis',
                    lot: '4792AB',
                    boost: '1999 Aug 10'
                },
                {
                    date: '1999 Aug 14',
                    vaccine: 'DTaP-HB-IPV-Hib',
                    dose: '',
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
