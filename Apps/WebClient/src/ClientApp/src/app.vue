<script lang="ts">
// Load Bootstrap general plugins
import {
    AlertPlugin,
    ButtonPlugin,
    CardPlugin,
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

Vue.use(LayoutPlugin);
Vue.use(NavPlugin);
Vue.use(NavbarPlugin);
Vue.use(ModalPlugin);
Vue.use(ButtonPlugin);
Vue.use(CardPlugin);
Vue.use(LinkPlugin);
Vue.use(FormPlugin);
Vue.use(FormInputPlugin);
Vue.use(FormCheckboxPlugin);
Vue.use(FormTextareaPlugin);
Vue.use(AlertPlugin);
Vue.use(SpinnerPlugin);
Vue.use(InputGroupPlugin);
Vue.use(PaginationNavPlugin);
Vue.use(TablePlugin);
Vue.use(TooltipPlugin);
Vue.use(FormGroupPlugin);
Vue.use(FormRadioPlugin);
Vue.use(FormRatingPlugin);
Vue.use(FormSelectPlugin);
Vue.use(FormDatepickerPlugin);
Vue.use(IconsPlugin);
Vue.use(VueTheMask);

// Load general icons
import { config } from "@fortawesome/fontawesome-svg-core";
// Prevent auto adding CSS to the header since that breaks Content security policies.
config.autoAddCss = false;
// Add font awesome styles manually
import "@fortawesome/fontawesome-svg-core/styles.css";

import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import VueTheMask from "vue-the-mask";
import { Action, Getter } from "vuex-class";

import CommunicationComponent from "@/components/CommunicationComponent.vue";
import ErrorCardComponent from "@/components/ErrorCardComponent.vue";
import IdleComponent from "@/components/modal/IdleComponent.vue";
import FooterComponent from "@/components/navmenu/FooterComponent.vue";
import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
import SidebarComponent from "@/components/navmenu/SidebarComponent.vue";
import ResourceCentreComponent from "@/components/ResourceCentreComponent.vue";
import Process, { EnvironmentType } from "@/constants/process";
import ScreenWidth from "@/constants/screenWidth";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

const logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

@Component({
    components: {
        NavHeader: HeaderComponent,
        NavFooter: FooterComponent,
        NavSidebar: SidebarComponent,
        ErrorCardComponent,
        IdleComponent,
        CommunicationComponent,
        ResourceCentreComponent,
    },
})
export default class App extends Vue {
    @Ref("idleModal")
    readonly idleModal!: IdleComponent;

    @Action("setIsMobile")
    setIsMobile!: (isMobile: boolean) => void;

    @Getter("isMobile")
    isMobile!: boolean;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated?: boolean;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider?: boolean;

    @Getter("hasTermsOfServiceUpdated", { namespace: "user" })
    hasTermsOfServiceUpdated!: boolean;

    private readonly host: string =
        window.location.hostname.toLocaleUpperCase();
    private readonly isProduction: boolean =
        Process.NODE_ENV == EnvironmentType.production &&
        (this.host.startsWith("HEALTHGATEWAY") ||
            this.host.startsWith("WWW.HEALTHGATEWAY"));

    private initialized = false;
    private windowWidth = 0;
    private vaccineCardPath = "/vaccinecard";
    private covidTestPath = "/covidtest";
    private loginCallbackPath = "/logincallback";
    private registrationPath = "/registration";
    private pcrTestPath = "/pcrtest";
    private acceptTermsOfServicePath = "/accepttermsofservice";
    private dependentsPath = "/dependents";
    private reportsPath = "/reports";
    private timelinePath = "/timeline";

    constructor() {
        super();
        logger.debug(
            `Node ENV: ${JSON.stringify(
                Process.NODE_ENV
            )}; host: ${JSON.stringify(this.host)}`
        );
        logger.debug(
            `VUE Config Integrity Environment Variable: ${JSON.stringify(
                process.env.VUE_APP_CONFIG_INTEGRITY
            )}`
        );
    }

    @Watch("isAppIdle")
    private onIsAppIdleChanged(idle: boolean) {
        if (idle && this.oidcIsAuthenticated && this.isValidIdentityProvider) {
            this.idleModal.show();
        }
    }

    private created() {
        this.windowWidth = window.innerWidth;
        this.$nextTick(() => {
            window.addEventListener("resize", this.onResize);
            this.onResize();
            this.initialized = true;
        });
    }

    private beforeDestroy() {
        window.removeEventListener("resize", this.onResize);
    }

    private onResize() {
        this.windowWidth = window.innerWidth;

        if (this.windowWidth < ScreenWidth.Mobile) {
            if (!this.isMobile) {
                this.setIsMobile(true);
            }
        } else {
            if (this.isMobile) {
                this.setIsMobile(false);
            }
        }
    }

    private currentPathMatches(...paths: string[]): boolean {
        const currentPath = this.$route.path.toLowerCase();
        return paths.some((path) => path === currentPath);
    }

    private get pageHasCustomLayout(): boolean {
        return this.currentPathMatches(
            this.vaccineCardPath,
            this.covidTestPath
        );
    }

    private get isHeaderVisible(): boolean {
        return !this.currentPathMatches(
            this.loginCallbackPath,
            this.vaccineCardPath,
            this.covidTestPath
        );
    }

    private get isFooterVisible(): boolean {
        return !this.currentPathMatches(
            this.loginCallbackPath,
            this.registrationPath,
            this.vaccineCardPath,
            this.covidTestPath
        );
    }

    private get isSidebarVisible(): boolean {
        return (
            !this.currentPathMatches(
                this.loginCallbackPath,
                this.registrationPath,
                this.acceptTermsOfServicePath
            ) &&
            !this.$route.path.toLowerCase().startsWith(this.pcrTestPath) &&
            !this.hasTermsOfServiceUpdated
        );
    }

    private get isCommunicationVisible(): boolean {
        return (
            !this.currentPathMatches(
                this.loginCallbackPath,
                this.vaccineCardPath,
                this.covidTestPath
            ) && !this.$route.path.toLowerCase().startsWith(this.pcrTestPath)
        );
    }

    private get isResourceCentreVisible(): boolean {
        return this.currentPathMatches(
            this.dependentsPath,
            this.reportsPath,
            this.timelinePath
        );
    }
}
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

        <NavHeader v-if="isHeaderVisible" class="d-print-none" />
        <b-row>
            <NavSidebar
                v-if="isSidebarVisible"
                class="d-print-none sticky-top vh-100"
            />
            <main class="col fill-height d-flex flex-column">
                <CommunicationComponent v-if="isCommunicationVisible" />
                <ErrorCardComponent v-if="!pageHasCustomLayout" />
                <router-view />
                <ResourceCentreComponent v-if="isResourceCentreVisible" />
                <IdleComponent ref="idleModal" />
            </main>
        </b-row>

        <footer v-if="isFooterVisible" class="footer d-print-none">
            <NavFooter />
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
</style>
