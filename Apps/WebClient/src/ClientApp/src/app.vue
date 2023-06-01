<script setup lang="ts">
// Add font awesome styles manually
import "@fortawesome/fontawesome-svg-core/styles.css";

// Load general icons
import { config } from "@fortawesome/fontawesome-svg-core";
// Load Bootstrap general plugins
import {
    AlertPlugin,
    AvatarPlugin,
    ButtonPlugin,
    CardPlugin,
    CarouselPlugin,
    FormCheckboxPlugin,
    FormDatepickerPlugin,
    FormGroupPlugin,
    FormInputPlugin,
    FormPlugin,
    FormRadioPlugin,
    FormRatingPlugin,
    FormSelectPlugin,
    FormTextareaPlugin,
    IconsPlugin,
    InputGroupPlugin,
    LayoutPlugin,
    LinkPlugin,
    ModalPlugin,
    NavbarPlugin,
    NavPlugin,
    PaginationNavPlugin,
    SpinnerPlugin,
    TablePlugin,
    TooltipPlugin,
} from "bootstrap-vue";
import Vue, {
    computed,
    nextTick,
    onBeforeUnmount,
    onMounted,
    ref,
    watch,
} from "vue";
import { useRoute, useRouter, useStore } from "vue-composition-wrapper";
import VueTheMask from "vue-the-mask";

import CommunicationComponent from "@/components/CommunicationComponent.vue";
import ErrorCardComponent from "@/components/ErrorCardComponent.vue";
import IdleComponent from "@/components/modal/IdleComponent.vue";
import FooterComponent from "@/components/navmenu/FooterComponent.vue";
import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
import SidebarComponent from "@/components/navmenu/SidebarComponent.vue";
import NotificationCentreComponent from "@/components/NotificationCentreComponent.vue";
import ResourceCentreComponent from "@/components/ResourceCentreComponent.vue";
import { AppErrorType } from "@/constants/errorType";
import Process, { EnvironmentType } from "@/constants/process";
import ScreenWidth from "@/constants/screenWidth";
import Eventbus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";
import { IdleDetector } from "@/utility/idleDetector";
import AppErrorView from "@/views/errors/AppErrorView.vue";

Vue.use(AlertPlugin);
Vue.use(AvatarPlugin);
Vue.use(ButtonPlugin);
Vue.use(CardPlugin);
Vue.use(CarouselPlugin);
Vue.use(FormCheckboxPlugin);
Vue.use(FormDatepickerPlugin);
Vue.use(FormGroupPlugin);
Vue.use(FormInputPlugin);
Vue.use(FormPlugin);
Vue.use(FormRadioPlugin);
Vue.use(FormRatingPlugin);
Vue.use(FormSelectPlugin);
Vue.use(FormTextareaPlugin);
Vue.use(IconsPlugin);
Vue.use(InputGroupPlugin);
Vue.use(LayoutPlugin);
Vue.use(LinkPlugin);
Vue.use(ModalPlugin);
Vue.use(NavbarPlugin);
Vue.use(NavPlugin);
Vue.use(PaginationNavPlugin);
Vue.use(SpinnerPlugin);
Vue.use(TablePlugin);
Vue.use(TooltipPlugin);
Vue.use(VueTheMask);

// Prevent auto adding CSS to the header since that breaks Content security policies.
config.autoAddCss = false;

const landingPath = "/";
const acceptTermsOfServicePath = "/accepttermsofservice";
const covidTestPath = "/covidtest";
const dependentsPath = "/dependents";
const loginCallbackPath = "/logincallback";
const pcrTestPath = "/pcrtest";
const queueFullPath = "/busy";
const queuePath = "/queue";
const registrationPath = "/registration";
const reportsPath = "/reports";
const timelinePath = "/timeline";
const vaccineCardPath = "/vaccinecard";

const logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
const route = useRoute();
const router = useRouter();
const store = useStore();

let idleDetector: IdleDetector | undefined;

const host = ref(window.location.hostname.toLocaleUpperCase());
const initialized = ref(false);
const windowWidth = ref(0);

const idleModal = ref<InstanceType<typeof IdleComponent>>();

const appError = computed<AppErrorType>(() => store.getters["appError"]);
const isMobile = computed<boolean>(() => store.getters["isMobile"]);
const isOffline = computed<boolean>(() => store.getters["config/isOffline"]);
const webClientConfig = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const oidcIsAuthenticated = computed<boolean>(
    () => store.getters["auth/oidcIsAuthenticated"]
);
const isValidIdentityProvider = computed<boolean>(
    () => store.getters["user/isValidIdentityProvider"]
);
const hasTermsOfServiceUpdated = computed<boolean>(
    () => store.getters["user/hasTermsOfServiceUpdated"]
);
const userIsRegistered = computed<boolean>(
    () => store.getters["user/userIsRegistered"]
);
const userIsActive = computed<boolean>(
    () => store.getters["user/userIsActive"]
);
const patientRetrievalFailed = computed<boolean>(
    () => store.getters["user/patientRetrievalFailed"]
);
const isProduction = computed(
    () =>
        Process.NODE_ENV == EnvironmentType.production &&
        (host.value.startsWith("HEALTHGATEWAY") ||
            host.value.startsWith("WWW.HEALTHGATEWAY"))
);
const timeBeforeIdle = computed(
    () => webClientConfig.value?.timeouts?.idle ?? 0
);
const maxIdleDialogCountdown = computed(() => 60000);
const pageHasCustomLayout = computed(() =>
    currentPathMatches(
        vaccineCardPath,
        covidTestPath,
        landingPath,
        queuePath,
        queueFullPath
    )
);
const isHeaderVisible = computed(
    () =>
        appError.value === undefined &&
        !currentPathMatches(loginCallbackPath, vaccineCardPath, covidTestPath)
);
const isFooterVisible = computed(
    () =>
        appError.value === undefined &&
        !currentPathMatches(
            loginCallbackPath,
            registrationPath,
            vaccineCardPath,
            covidTestPath
        )
);
const isSidebarVisible = computed(
    () =>
        !currentPathMatches(
            loginCallbackPath,
            registrationPath,
            acceptTermsOfServicePath
        ) &&
        !route.value.path.toLowerCase().startsWith(pcrTestPath) &&
        !hasTermsOfServiceUpdated.value
);
const isNotificationCentreEnabled = computed(
    () =>
        webClientConfig.value.featureToggleConfiguration.notificationCentre
            .enabled &&
        !isOffline.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value
);
const isCommunicationVisible = computed(
    () =>
        !currentPathMatches(
            loginCallbackPath,
            vaccineCardPath,
            covidTestPath
        ) && !route.value.path.toLowerCase().startsWith(pcrTestPath)
);
const isResourceCentreVisible = computed(() =>
    currentPathMatches(dependentsPath, reportsPath, timelinePath)
);

function setIsMobile(isMobile: boolean): void {
    store.dispatch("setIsMobile", isMobile);
}

function initializeResizeListener(): void {
    window.addEventListener("resize", onResize);
    onResize();
}

function onResize(): void {
    windowWidth.value = window.innerWidth;

    if (windowWidth.value < ScreenWidth.Mobile) {
        if (!isMobile.value) {
            setIsMobile(true);
        }
    } else {
        if (isMobile.value) {
            setIsMobile(false);
        }
    }
}

function initializeIdleDetector(): void {
    if (timeBeforeIdle.value > 0) {
        idleDetector = new IdleDetector(
            (timeIdle) => handleIsIdle(timeIdle),
            timeBeforeIdle.value
        );
        if (oidcIsAuthenticated.value) {
            idleDetector.enable();
        }
    }
}

function handleIsIdle(timeIdle: number): void {
    if (!oidcIsAuthenticated.value) {
        return;
    }

    const countdownTime = maxIdleDialogCountdown.value - timeIdle;
    if (countdownTime <= 0) {
        router.push("/logout");
    } else {
        idleModal.value?.show(countdownTime, () => idleDetector?.enable());
    }
}

function currentPathMatches(...paths: string[]): boolean {
    const currentPath = route.value.path.toLowerCase();
    return paths.some((path) => path === currentPath);
}

function releaseTicketBeforeUnloadListener(): void {
    store.dispatch("waitlist/releaseTicket");
}

watch(oidcIsAuthenticated, (value: boolean) => {
    // enable idle detector when authenticated and disable when not
    if (value) {
        idleDetector?.enable();
    } else {
        idleDetector?.disable();
    }
});

onMounted(async () => {
    windowWidth.value = window.innerWidth;

    await nextTick();
    initializeResizeListener();
    initializeIdleDetector();
    initialized.value = true;
});

onBeforeUnmount(() => {
    window.removeEventListener("resize", onResize);
});

// Created hook
logger.debug(`Node ENV: ${Process.NODE_ENV}; host: ${host.value}`);
logger.debug(
    `VUE Config Integrity Environment Variable: ${process.env.VUE_APP_CONFIG_INTEGRITY}`
);
Eventbus.$on(EventMessageName.RegisterBeforeUnloadWaitlistListener, () => {
    logger.debug("Waitlist - ticket unload listener registered.");
    window.addEventListener("beforeunload", releaseTicketBeforeUnloadListener);
});

Eventbus.$on(EventMessageName.UnregisterBeforeUnloadWaitlistListener, () => {
    logger.debug("Waitlist - ticket unload listener unregistered.");
    window.removeEventListener(
        "beforeunload",
        releaseTicketBeforeUnloadListener
    );
});
// Initial binding of the ticket release listener
Eventbus.$emit(EventMessageName.RegisterBeforeUnloadWaitlistListener);
</script>

<template>
    <div
        v-if="initialized"
        id="app-root"
        class="container-fluid-fill d-flex h-100 flex-column"
    >
        <div v-if="!isProduction" class="devBanner d-print-none">
            <div class="text-center bg-warning small">
                Non-production environment:
                <strong>{{ host }}</strong>
            </div>
        </div>

        <HeaderComponent v-if="isHeaderVisible" class="d-print-none" />

        <AppErrorView v-if="appError !== undefined" />
        <b-row v-else>
            <SidebarComponent
                v-if="isSidebarVisible"
                class="d-print-none sticky-top vh-100"
            />
            <main class="col fill-height d-flex flex-column">
                <CommunicationComponent v-if="isCommunicationVisible" />
                <b-sidebar
                    v-if="isNotificationCentreEnabled"
                    id="notification-centre-sidebar"
                    body-class="d-flex"
                    no-header
                    right
                    shadow
                    backdrop
                >
                    <NotificationCentreComponent />
                </b-sidebar>

                <router-view v-if="pageHasCustomLayout" />
                <div v-else class="m-3 m-md-4">
                    <ErrorCardComponent />
                    <router-view />
                </div>

                <ResourceCentreComponent v-if="isResourceCentreVisible" />
                <IdleComponent ref="idleModal" />
            </main>
        </b-row>

        <footer v-if="isFooterVisible" class="footer d-print-none">
            <FooterComponent />
        </footer>
    </div>
</template>

<style lang="scss" scoped>
.row {
    margin: 0px;
    padding: 0px;
}

.col {
    margin: 0px;
    padding: 0px;
}
</style>
<style lang="scss">
@import "@/assets/scss/_variables.scss";

@media print {
    .navbar {
        display: flex !important;
    }
}

html {
    height: 100vh;
}

body {
    min-height: 100vh;
    display: flex;
    flex-direction: column;
}

main {
    padding-bottom: 0px;
    padding-top: 0px;
}

#app-root {
    min-height: 100vh;
}

.fill-height {
    margin: 0px;
    min-height: 100vh;
}

.devBanner {
    z-index: $z_header;
}

label.hg-label {
    font-weight: bold;
}

.is-invalid .hg-label {
    color: $danger;
}

.popover {
    z-index: $z_popover;
}
</style>
