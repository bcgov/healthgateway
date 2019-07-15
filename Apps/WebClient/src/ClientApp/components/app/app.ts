import Vue from 'vue';
import { Component } from 'vue-property-decorator';

import menu from '../navmenu/navmenu.vue';

@Component({
    components: {
        MenuComponent: menu
    }
})
export default class AppComponent extends Vue {
    host: string = window.location.hostname.toLocaleUpperCase();

    constructor() {
        super();
        console.log(this.host);
    }
}
