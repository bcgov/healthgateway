import Vue from 'vue';
import { Component } from 'vue-property-decorator';

import HeaderMenu from '../navmenu/header.vue';
import FooterMenu from '../navmenu/footer.vue';

//import "../node_modules/@bcgov/bootstrap-theme/dist/scss/bootstrap-theme";

@Component({
    components: {
        HeaderComponent: HeaderMenu,
        FooterComponent: FooterMenu,
    }
})
export default class AppComponent extends Vue {
    host: string = window.location.hostname.toLocaleUpperCase();

    constructor() {
        super();
        console.log(this.host);
    }
}
