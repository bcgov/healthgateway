// eslint-disable-next-line
import container from "@/plugins/container";

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

import HgButtonComponent from "@/components/shared/HgButtonComponent.vue";
import HgCardButtonComponent from "@/components/shared/HgCardButtonComponent.vue";
import HgDropdownComponent from "@/components/shared/HgDropdownComponent.vue";
import HgIconComponent from "@/components/shared/HgIconComponent.vue";
import PageTitleComponent from "@/components/shared/PageTitleComponent.vue";
import StatusLabelComponent from "@/components/shared/StatusLabelComponent.vue";

import { instanceOfResultError } from "@/models/errors";
import User from "@/models/user";
import {
    DELEGATE_IDENTIFIER,
    SERVICE_IDENTIFIER,
    STORE_IDENTIFIER,
} from "@/plugins/inversify";

import router from "@/router";

import { RootState } from "@/store/types";
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
    IPcrTestService,
    IReportService,
    IStoreProvider,
    IUserCommentService,
    IUserFeedbackService,
    IUserNoteService,
    IUserProfileService,
    IUserRatingService,
    IVaccinationStatusService,
} from "@/services/interfaces";

Vue.component("BBadge", BBadge);
Vue.component("BBreadcrumb", BBreadcrumb);
Vue.component("BBreadcrumbItem", BBreadcrumbItem);
Vue.component("BCard", BCard);
Vue.component("BPopover", BPopover);
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

const httpDelegate = container.get<IHttpDelegate>(
    DELEGATE_IDENTIFIER.HttpDelegate
);
const configService = container.get<IConfigService>(
    SERVICE_IDENTIFIER.ConfigService
);

configService.initialize(httpDelegate);

// Retrieve configuration and initialize services
configService
    .getConfiguration()
    .then((config) => {
        // Retrieve service interfaces
        const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        const authService = container.get<IAuthenticationService>(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        const immunizationService = container.get<IImmunizationService>(
            SERVICE_IDENTIFIER.ImmunizationService
        );
        const patientService = container.get<IPatientService>(
            SERVICE_IDENTIFIER.PatientService
        );
        const medicationService = container.get<IMedicationService>(
            SERVICE_IDENTIFIER.MedicationService
        );
        const laboratoryService = container.get<ILaboratoryService>(
            SERVICE_IDENTIFIER.LaboratoryService
        );
        const encounterService = container.get<IEncounterService>(
            SERVICE_IDENTIFIER.EncounterService
        );
        const userProfileService = container.get<IUserProfileService>(
            SERVICE_IDENTIFIER.UserProfileService
        );
        const userFeedbackService = container.get<IUserFeedbackService>(
            SERVICE_IDENTIFIER.UserFeedbackService
        );
        const userNoteService = container.get<IUserNoteService>(
            SERVICE_IDENTIFIER.UserNoteService
        );
        const communicationService = container.get<ICommunicationService>(
            SERVICE_IDENTIFIER.CommunicationService
        );
        const userCommentService = container.get<IUserCommentService>(
            SERVICE_IDENTIFIER.UserCommentService
        );
        const userRatingService = container.get<IUserRatingService>(
            SERVICE_IDENTIFIER.UserRatingService
        );
        const dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
        const reportService = container.get<IReportService>(
            SERVICE_IDENTIFIER.ReportService
        );
        const vaccinationStatusService =
            container.get<IVaccinationStatusService>(
                SERVICE_IDENTIFIER.VaccinationStatusService
            );
        const storeProvider = container.get<IStoreProvider>(
            STORE_IDENTIFIER.StoreProvider
        );
        const pcrTestKitService = container.get<IPcrTestService>(
            SERVICE_IDENTIFIER.PcrTestService
        );

        const store = storeProvider.getStore();
        store.dispatch("config/initialize", config);

        logger.initialize(config.webClient.logLevel);

        // Initialize services
        const authInitializePromise = authService.initialize(
            config.openIdConnect
        );
        immunizationService.initialize(config, httpDelegate);
        patientService.initialize(config, httpDelegate);
        medicationService.initialize(config, httpDelegate);
        laboratoryService.initialize(config, httpDelegate);
        encounterService.initialize(config, httpDelegate);
        userProfileService.initialize(config, httpDelegate);
        userFeedbackService.initialize(config, httpDelegate);
        userNoteService.initialize(config, httpDelegate);
        communicationService.initialize(config, httpDelegate);
        userCommentService.initialize(config, httpDelegate);
        userRatingService.initialize(config, httpDelegate);
        dependentService.initialize(config, httpDelegate);
        pcrTestKitService.initialize(config, httpDelegate);
        reportService.initialize(config, httpDelegate);
        vaccinationStatusService.initialize(config, httpDelegate);

        authInitializePromise.then(async () => {
            Vue.use(IdleVue, {
                eventEmitter: new Vue(),
                idleTime: config.webClient.timeouts.idle,
                store,
                startAtIdle: false,
            });

            if (window.location.pathname !== "/loginCallback") {
                const signedIn = await store.dispatch("auth/checkStatus");
                if (signedIn) {
                    logger.verbose("User is signed in");
                } else {
                    logger.verbose("User is not signed in");
                }

                const isValidIdentityProvider: boolean =
                    store.getters["auth/isValidIdentityProvider"];
                const user: User = store.getters["user/user"];

                if (user.hdid && isValidIdentityProvider) {
                    try {
                        await store.dispatch("user/checkRegistration");
                    } catch (error) {
                        initializeVueError(false);
                    }
                }
            }

            initializeVue(store);
        });
    })
    .catch((error) => {
        let busy = false;
        if (instanceOfResultError(error) && error.statusCode === 429) {
            busy = true;
        }

        initializeVueError(busy);
    });

function initializeVue(store: Store<RootState>): Vue {
    const App = () => import(/* webpackChunkName: "entry" */ "./app.vue");
    return new Vue({
        el: "#app-root",
        store,
        router,
        render: (h) => h(App),
    });
}

function initializeVueError(busy: boolean): Vue {
    const AppErrorView = () =>
        import(
            /* webpackChunkName: "error" */ "./views/errors/AppErrorView.vue"
        );
    return new Vue({
        el: "#app-root",
        render: (h) => h(AppErrorView, { props: { busy } }),
    });
}
