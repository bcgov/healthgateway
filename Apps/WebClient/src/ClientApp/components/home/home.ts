import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component({
    components: {
        LocaleComponent: require('../locale/locale.vue.html')
    }
})
export default class HomeComponent extends Vue {
}
