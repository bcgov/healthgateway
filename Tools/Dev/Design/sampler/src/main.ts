import "@/assets/scss/bcgov/bootstrap-theme.scss";

import {
    FontAwesomeIcon,
    FontAwesomeLayers,
    FontAwesomeLayersText,
} from "@fortawesome/vue-fontawesome";
import Vue from "vue";

import App from "./App.vue";

Vue.config.productionTip = false;

Vue.component("FontAwesomeIcon", FontAwesomeIcon);
Vue.component("FontAwesomeLayers", FontAwesomeLayers);
Vue.component("FontAwesomeLayersText", FontAwesomeLayersText);

new Vue({
    render: (h) => h(App),
}).$mount("#app-root");
