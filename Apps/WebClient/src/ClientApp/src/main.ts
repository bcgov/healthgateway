// eslint-disable-next-line
import container from "@/plugins/inversify.container";

import "@/plugins/inversify.config";

import "core-js/stable";
import "bootstrap-vue/dist/bootstrap-vue.css";
import "@/assets/scss/hg-styles.scss";
import "@/plugins/registerComponentHooks";

import {
    BBadge,
    BBreadcrumb,
    BBreadcrumbItem,
    BCard,
    BDropdown,
    BDropdownDivider,
    BDropdownItem,
    BDropdownItemButton,
    BDropdownText,
    BFormTag,
    BFormTags,
    BPopover,
} from "bootstrap-vue";
import IdleVue from "idle-vue";
import Vue from "vue";
import VueContentPlaceholders from "vue-content-placeholders";
import VueRouter from "vue-router";
import Vuelidate from "vuelidate";
import { Store } from "vuex";

import HgButtonComponent from "@/components/shared/hgButton.vue";
import HgCardButtonComponent from "@/components/shared/hgCardButton.vue";
import HgDropdownComponent from "@/components/shared/hgDropdown.vue";
import HgIconComponent from "@/components/shared/hgIcon.vue";
import PageTitleComponent from "@/components/shared/pageTitle.vue";
import StatusLabelComponent from "@/components/shared/statusLabel.vue";

import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";
import {
    DELEGATE_IDENTIFIER,
    SERVICE_IDENTIFIER,
    STORE_IDENTIFIER,
} from "@/plugins/inversify";

import router from "@/router";
import {
    IAuthenticationService,
    ICommunicationService,
    IConfigService,
    IDependentService,
    IEncounterService,
    IHttpDelegate,
    IImmunizationService,
    ILaboratoryService,
    ILogger,
    IMedicationService,
    IPatientService,
    IPCRTestService,
    IReportService,
    IStoreProvider,
    IUserCommentService,
    IUserFeedbackService,
    IUserNoteService,
    IUserProfileService,
    IUserRatingService,
    IVaccinationStatusService,
} from "@/services/interfaces";

import { RootState } from "./store/types";

Vue.component("BBreadcrumb", BBreadcrumb);
Vue.component("BBreadcrumbItem", BBreadcrumbItem);
Vue.component("BPopover", BPopover);
Vue.component("BBadge", BBadge);
Vue.component("BCard", BCard);
Vue.component("BDropdown", BDropdown);
Vue.component("BDropdownDivider", BDropdownDivider);
Vue.component("BDropdownItem", BDropdownItem);
Vue.component("BDropdownItemButton", BDropdownItemButton);
Vue.component("BDropdownText", BDropdownText);
Vue.component("BFormTags", BFormTags);
Vue.component("BFormTag", BFormTag);
Vue.component("HgButton", HgButtonComponent);
Vue.component("HgCardButton", HgCardButtonComponent);
Vue.component("HgDropdown", HgDropdownComponent);
Vue.component("HgIcon", HgIconComponent);
Vue.component("PageTitle", PageTitleComponent);
Vue.component("StatusLabel", StatusLabelComponent);

Vue.use(VueRouter);
Vue.use(Vuelidate);
Vue.use(VueContentPlaceholders);

const httpDelegate: IHttpDelegate = container.get(
    DELEGATE_IDENTIFIER.HttpDelegate
);
const configService: IConfigService = container.get(
    SERVICE_IDENTIFIER.ConfigService
);

configService.initialize(httpDelegate);

// Retrieve configuration and initialize services
configService.getConfiguration().then((config: ExternalConfiguration) => {
    // Retrieve service interfaces
    const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    const authService: IAuthenticationService = container.get(
        SERVICE_IDENTIFIER.AuthenticationService
    );
    const immunizationService: IImmunizationService = container.get(
        SERVICE_IDENTIFIER.ImmunizationService
    );
    const patientService: IPatientService = container.get(
        SERVICE_IDENTIFIER.PatientService
    );
    const medicationService: IMedicationService = container.get(
        SERVICE_IDENTIFIER.MedicationService
    );
    const laboratoryService: ILaboratoryService = container.get(
        SERVICE_IDENTIFIER.LaboratoryService
    );
    const encounterService: IEncounterService = container.get(
        SERVICE_IDENTIFIER.EncounterService
    );
    const userProfileService: IUserProfileService = container.get(
        SERVICE_IDENTIFIER.UserProfileService
    );
    const userFeedbackService: IUserFeedbackService = container.get(
        SERVICE_IDENTIFIER.UserFeedbackService
    );
    const userNoteService: IUserNoteService = container.get(
        SERVICE_IDENTIFIER.UserNoteService
    );
    const communicationService: ICommunicationService = container.get(
        SERVICE_IDENTIFIER.CommunicationService
    );
    const userCommentService: IUserCommentService = container.get(
        SERVICE_IDENTIFIER.UserCommentService
    );
    const userRatingService: IUserRatingService = container.get(
        SERVICE_IDENTIFIER.UserRatingService
    );
    const dependentService: IDependentService = container.get(
        SERVICE_IDENTIFIER.DependentService
    );
    const reportService: IReportService = container.get(
        SERVICE_IDENTIFIER.ReportService
    );
    const vaccinationStatusService: IVaccinationStatusService = container.get(
        SERVICE_IDENTIFIER.VaccinationStatusService
    );
    const storeProvider: IStoreProvider = container.get(
        STORE_IDENTIFIER.StoreProvider
    );
    const pcrTestKitService: IPCRTestService = container.get(
        SERVICE_IDENTIFIER.PCRTestService
    );

    const store = storeProvider.getStore();
    store.dispatch("config/initialize", config);

    logger.initialize(config.webClient.logLevel);

    // Initialize services
    authService.initialize(config.openIdConnect, httpDelegate);
    immunizationService.initialize(config, httpDelegate);
    patientService.initialize(config, httpDelegate);
    medicationService.initialize(config, httpDelegate);
    laboratoryService.initialize(config, httpDelegate);
    encounterService.initialize(config, httpDelegate);
    userProfileService.initialize(httpDelegate);
    userFeedbackService.initialize(httpDelegate);
    userNoteService.initialize(config, httpDelegate);
    communicationService.initialize(httpDelegate);
    userCommentService.initialize(config, httpDelegate);
    userRatingService.initialize(httpDelegate);
    dependentService.initialize(config, httpDelegate);
    pcrTestKitService.initialize(config, httpDelegate);
    reportService.initialize(httpDelegate);
    vaccinationStatusService.initialize(config, httpDelegate);

    Vue.use(IdleVue, {
        eventEmitter: new Vue(),
        idleTime: config.webClient.timeouts.idle,
        store,
        startAtIdle: false,
    });
    if (window.location.pathname === "/loginCallback") {
        initializeVue(store);
    } else {
        store.dispatch("auth/getOidcUser").then(() => {
            const isValid: boolean =
                store.getters["auth/isValidIdentityProvider"];
            const user: User = store.getters["user/user"];
            if (user.hdid && isValid) {
                store.dispatch("user/checkRegistration").then(() => {
                    initializeVue(store);
                });
            } else {
                initializeVue(store);
            }
        });
    }
});

const App = () => import(/* webpackChunkName: "entry" */ "./app.vue");

function initializeVue(store: Store<RootState>) {
    return new Vue({
        el: "#app-root",
        store,
        router,
        render: (h) => h(App),
    });
}
