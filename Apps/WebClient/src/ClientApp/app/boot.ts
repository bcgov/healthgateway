import Vue from "vue";
import VueRouter from "vue-router";
import "@fortawesome/fontawesome-free/js/fontawesome";
import "@fortawesome/fontawesome-free/js/solid";
import "@fortawesome/fontawesome-free/js/regular";
import "@fortawesome/fontawesome-free/js/brands";
import "bootstrap-vue/dist/bootstrap-vue.css";
import "@/assets/scss/bcgov/bootstrap-theme.scss";

import BootstrapVue from "bootstrap-vue";
import i18n from "./i18n";

import App from "./app.vue";
import router from "./router";
import store from "@/store/store";
import { IAuthenticationService } from './services/interfaces';
import SERVICE_IDENTIFIER from './constants/serviceIdentifiers';
import container from './inversify.config';
import { ExternalConfiguration } from './models/ConfigData';

Vue.use(BootstrapVue);
Vue.use(VueRouter);

// Initialize the store only then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
  const authService: IAuthenticationService = container.get<
    IAuthenticationService
  >(SERVICE_IDENTIFIER.AuthenticationService);
  authService.initialize(config.openIdConnect);

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
