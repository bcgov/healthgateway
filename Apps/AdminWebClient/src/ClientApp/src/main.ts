import "core-js/stable";
import "regenerator-runtime/runtime";
import Vue from "vue";
import vuetify from "./plugins/vuetify";
import App from "./App.vue";
import router from "./router";
import store from "@/store/store";
import "./registerServiceWorker";
import dateFilter from "@/filters/date.filter";

import {
  IHttpDelegate,
  IBetaRequestService,
  IConfigService,
  IAuthenticationService
} from "@/services/interfaces";
import { SERVICE_IDENTIFIER, DELEGATE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import ExternalConfiguration from "@/models/externalConfiguration";

Vue.config.productionTip = false;

Vue.filter("date", dateFilter);

const httpDelegate: IHttpDelegate = container.get(
  DELEGATE_IDENTIFIER.HttpDelegate
);
const configService: IConfigService = container.get(
  SERVICE_IDENTIFIER.ConfigService
);
const authenticationService: IAuthenticationService = container.get(
  SERVICE_IDENTIFIER.AuthenticationService
);

configService.initialize(httpDelegate);
authenticationService.initialize(httpDelegate);
// Initialize the store only then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
  // Retrieve service interfaces
  const betaRequestService: IBetaRequestService = container.get(
    SERVICE_IDENTIFIER.BetaRequestService
  );

  // Initialize services
  betaRequestService.initialize(httpDelegate);
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
