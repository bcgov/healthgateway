import "core-js/stable";
import "tiptap-vuetify/dist/main.css";

import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { TiptapVuetifyPlugin } from "tiptap-vuetify";
import Vue from "vue";
import VueTheMask from "vue-the-mask";
import DatetimePicker from "vuetify-datetime-picker";

import App from "@/App.vue";
import dateFilter from "@/filters/date.filter";
import ExternalConfiguration from "@/models/externalConfiguration";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import vuetify from "@/plugins/vuetify";
import router from "@/router";
import {
    IAuthenticationService,
    IConfigService,
    ICovidSupportService,
    IHttpDelegate,
    ISupportService,
} from "@/services/interfaces";
import store from "@/store/store";

Vue.config.productionTip = false;

Vue.use(DatetimePicker);
Vue.use(TiptapVuetifyPlugin, {
    vuetify,
    iconsGroup: "md",
});
Vue.use(VueTheMask);
Vue.filter("date", dateFilter);
Vue.component("FontAwesomeIcon", FontAwesomeIcon);

const httpDelegate: IHttpDelegate = container.get(
    DELEGATE_IDENTIFIER.HttpDelegate
);

const configService: IConfigService = container.get(
    SERVICE_IDENTIFIER.ConfigService
);

const authenticationService: IAuthenticationService = container.get(
    SERVICE_IDENTIFIER.AuthenticationService
);

const supportService: ISupportService = container.get(
    SERVICE_IDENTIFIER.SupportService
);

const covidSupportService: ICovidSupportService = container.get(
    SERVICE_IDENTIFIER.CovidSupportService
);

configService.initialize(httpDelegate);

// Initialize the store only, then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
    // Initialize services
    authenticationService.initialize(httpDelegate, config);
    supportService.initialize(httpDelegate);
    covidSupportService.initialize(httpDelegate);
    initializeVue();
});

function initializeVue() {
    new Vue({
        vuetify,
        router,
        store,
        render: (h) => h(App),
    }).$mount("#app");
}
