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
import Vuelidate from "vuelidate";
import "@/registerComponentHooks";

import App from "@/app.vue";
import router from "@/router";
import store from "@/store/store";
import {
  IAuthenticationService,
  IImmsService,
  IPatientService,
  IMedicationService,
  IHttpDelegate,
  IConfigService,
  IUserProfileService,
  IUserFeedbackService,
  IEmailValidationService
} from "@/services/interfaces";
import SERVICE_IDENTIFIER, {
  DELEGATE_IDENTIFIER
} from "@/constants/serviceIdentifiers";
import container from "@/inversify.config";
import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";
import { RegistrationStatus } from "./constants/registrationStatus";

Vue.use(BootstrapVue);
Vue.use(VueRouter);
Vue.use(Vuelidate);

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
  const userProfileService: IUserProfileService = container.get(
    SERVICE_IDENTIFIER.UserProfileService
  );
  const userFeedbackService: IUserFeedbackService = container.get(
    SERVICE_IDENTIFIER.UserFeedbackService
  );
  const emailValidationService: IEmailValidationService = container.get(
    SERVICE_IDENTIFIER.EmailValidationService
  );

  // Initialize services
  authService.initialize(config.openIdConnect, httpDelegate);
  immsService.initialize(config, httpDelegate);
  patientService.initialize(config, httpDelegate);
  medicationService.initialize(config, httpDelegate);
  userProfileService.initialize(httpDelegate);
  userFeedbackService.initialize(httpDelegate);
  emailValidationService.initialize(httpDelegate);
  Vue.use(IdleVue, {
    eventEmitter: new Vue(),
    idleTime: config.webClient.timeouts!.idle || 300000,
    registrationStatus:
      config.webClient.registrationStatus || RegistrationStatus.Closed,
    store,
    startAtIdle: false
  });

  store.dispatch("auth/getOidcUser").then(() => {
    let user: User = store.getters["user/user"];
    if (user.hdid) {
      store.dispatch("user/checkRegistration", { hdid: user.hdid }).then(() => {
        initializeVue();
      });
    } else {
      initializeVue();
    }
  });
});

function initializeVue() {
  new Vue({
    el: "#app-root",
    i18n: i18n,
    store,
    router,
    render: h => h(App)
  });
}
