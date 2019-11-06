import Vue from "vue";
import VueRouter from "vue-router";
import "@fortawesome/fontawesome-free/js/fontawesome";
import "@fortawesome/fontawesome-free/js/solid";
import "@fortawesome/fontawesome-free/js/regular";
import "@fortawesome/fontawesome-free/js/brands";
import "bootstrap-vue/dist/bootstrap-vue.css";
import "@/assets/scss/bcgov/bootstrap-theme.scss";

import BootstrapVue from "bootstrap-vue";
import i18n from "@/i18n";
import IdleVue from "idle-vue";

import App from "@/app.vue";
import router from "@/router";
import store from "@/store/store";
import {
  IAuthenticationService,
  IImmsService,
  IPatientService,
  IMedicationService,
  IHttpDelegate,
  IConfigService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER, {
  DELEGATE_IDENTIFIER
} from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { ExternalConfiguration } from "@/models/configData";

Vue.use(BootstrapVue);
Vue.use(VueRouter);

const httpDelegate: IHttpDelegate = container.get(
  DELEGATE_IDENTIFIER.HttpDelegate
);
const configService: IConfigService = container.get(
  SERVICE_IDENTIFIER.ConfigService
);

configService.initialize(httpDelegate);
// Initialize the store only then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
  // Retrieve service interfaces
  const authService: IAuthenticationService = container.get(
    SERVICE_IDENTIFIER.AuthenticationService
  );
  const immsService: IImmsService = container.get(
    SERVICE_IDENTIFIER.ImmsService
  );
  const patientService: IPatientService = container.get(
    SERVICE_IDENTIFIER.PatientService
  );
  const medicationService: IMedicationService = container.get(
    SERVICE_IDENTIFIER.MedicationService
  );

  // Initialize services
  authService.initialize(config.openIdConnect);
  immsService.initialize(config, httpDelegate);
  patientService.initialize(config, httpDelegate);
  medicationService.initialize(config, httpDelegate);

  Vue.use(IdleVue, {
    eventEmitter: new Vue(),
    idleTime: config.webClient.timeouts!.idle || 300000,
    store,
    startAtIdle: false
  });

  store.dispatch("auth/getOidcUser").then(() => {
    new Vue({
      el: "#app-root",
      i18n: i18n,
      store,
      router,
      render: h => h(App)
    });
  });
});
