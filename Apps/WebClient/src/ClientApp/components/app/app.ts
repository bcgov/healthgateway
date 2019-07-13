import Vue from 'vue';
import { Component } from 'vue-property-decorator';

@Component({
    components: {
        MenuComponent: require('../navmenu/navmenu.vue.html')
    }
})
export default class AppComponent extends Vue {
    host: string = window.location.hostname.toLocaleUpperCase();

    constructor() {
        super();
        console.log(this.host);
    }
}
