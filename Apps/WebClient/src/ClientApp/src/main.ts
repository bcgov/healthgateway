import "core-js/stable";

import Vue from "vue";
import VueRouter from "vue-router";

import "bootstrap-vue/dist/bootstrap-vue.css";
import "@/assets/scss/bcgov/bootstrap-theme.scss";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { BPopover } from "bootstrap-vue";
import IdleVue from "idle-vue";
import Vuelidate from "vuelidate";
import VueContentPlaceholders from "vue-content-placeholders";
import "@/plugins/registerComponentHooks";

const App = () => import(/* webpackChunkName: "entry" */ "./app.vue");
import router from "@/router";
import store from "@/store/store";
import {
    ILogger,
    IAuthenticationService,
    IBetaRequestService,
    ICommunicationService,
    IConfigService,
    IHttpDelegate,
    IImmunizationService,
    ILaboratoryService,
    IMedicationService,
    IPatientService,
    IUserCommentService,
    IUserFeedbackService,
    IUserNoteService,
    IUserProfileService,
    IUserRatingService,
    IEncounterService,
} from "@/services/interfaces";
import { DELEGATE_IDENTIFIER, SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ExternalConfiguration } from "@/models/configData";
import User from "@/models/user";

Vue.component("FontAwesomeIcon", FontAwesomeIcon);
Vue.component("BPopover", BPopover);

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
// Initialize the store only then start the app
store.dispatch("config/initialize").then((config: ExternalConfiguration) => {
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
    const betaRequestService: IBetaRequestService = container.get(
        SERVICE_IDENTIFIER.BetaRequestService
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
    betaRequestService.initialize(httpDelegate);
    userNoteService.initialize(config, httpDelegate);
    communicationService.initialize(httpDelegate);
    userCommentService.initialize(config, httpDelegate);
    userRatingService.initialize(httpDelegate);
    Vue.use(IdleVue, {
        eventEmitter: new Vue(),
        idleTime: config.webClient.timeouts.idle,
        store,
        startAtIdle: false,
    });
    if (window.location.pathname === "/loginCallback") {
        initializeVue();
    } else {
        store.dispatch("auth/getOidcUser").then(() => {
            const isValid: boolean =
                store.getters["auth/isValidIdentityProvider"];
            const user: User = store.getters["user/user"];
            if (user.hdid && isValid) {
                store
                    .dispatch("user/checkRegistration", { hdid: user.hdid })
                    .then(() => {
                        initializeVue();
                    });
            } else {
                initializeVue();
            }
        });
    }
});

function initializeVue() {
    new Vue({
        el: "#app-root",
        store,
        router,
        render: (h) => h(App),
    });
}
