import "core-js/stable";
import "regenerator-runtime/runtime";
import Vue from "vue";
import vuetify from "./plugins/vuetify";
import App from "./App.vue";
import router from "./router";
import store from "@/store/store";
import "./registerServiceWorker";
import dateFilter from "@/filters/date.filter";
import DatetimePicker from "vuetify-datetime-picker";

import {
    IHttpDelegate,
    IBetaRequestService,
    IConfigService,
    IAuthenticationService,
    IUserFeedbackService,
    IDashboardService,
    IEmailAdminService,
    ICommunicationService
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER, DELEGATE_IDENTIFIER } from "@/plugins/inversify";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import container from "@/plugins/inversify.config";
import ExternalConfiguration from "@/models/externalConfiguration";

Vue.config.productionTip = false;

Vue.use(DatetimePicker);
Vue.filter("date", dateFilter);
Vue.component("font-awesome-icon", FontAwesomeIcon);

const httpDelegate: IHttpDelegate = container.get(
    DELEGATE_IDENTIFIER.HttpDelegate
);
const configService: IConfigService = container.get(
    SERVICE_IDENTIFIER.ConfigService
);
const authenticationService: IAuthenticationService = container.get(
    SERVICE_IDENTIFIER.AuthenticationService
);
const emailAdminService: IEmailAdminService = container.get(
    SERVICE_IDENTIFIER.EmailAdminService
);

configService.initialize(httpDelegate);
// Initialize the store only then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
    // Retrieve service interfaces
    const betaRequestService: IBetaRequestService = container.get(
        SERVICE_IDENTIFIER.BetaRequestService
    );
    const userFeedbackService: IUserFeedbackService = container.get(
        SERVICE_IDENTIFIER.UserFeedbackService
    );
    const dashboardService: IDashboardService = container.get(
        SERVICE_IDENTIFIER.DashboardService
    );
    const communicationService: ICommunicationService = container.get(
        SERVICE_IDENTIFIER.CommunicationService
    );

    // Initialize services
    authenticationService.initialize(httpDelegate, config);
    betaRequestService.initialize(httpDelegate);
    userFeedbackService.initialize(httpDelegate);
    dashboardService.initialize(httpDelegate);
    emailAdminService.initialize(httpDelegate);
    communicationService.initialize(httpDelegate);
    initializeVue();
});

function initializeVue() {
    new Vue({
        vuetify,
        router,
        store,
        render: h => h(App)
    }).$mount("#app");
}
