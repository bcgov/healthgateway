<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faAngleDoubleLeft,
    faBoxArchive,
    faCheckCircle,
    faCloudArrowDown,
    faHandHoldingMedical,
    faHome,
    faUserFriends,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import FeedbackComponent from "@/components/FeedbackComponent.vue";
import TutorialComponent from "@/components/shared/TutorialComponent.vue";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

library.add(
    faAngleDoubleLeft,
    faBoxArchive,
    faCheckCircle,
    faCloudArrowDown,
    faHandHoldingMedical,
    faHome,
    faUserFriends
);

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        FeedbackComponent,
        TutorialComponent,
    },
};

@Component(options)
export default class SidebarComponent extends Vue {
    @Getter("isMobile")
    isMobileWidth!: boolean;

    @Getter("isOffline", { namespace: "config" })
    isOffline!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("toggleSidebar", { namespace: "navbar" })
    toggleSidebar!: () => void;

    @Action("setSidebarStoppedAnimating", { namespace: "navbar" })
    setSidebarStoppedAnimating!: () => void;

    @Getter("isSidebarOpen", { namespace: "navbar" })
    isOpen!: boolean;

    @Getter("isSidebarAnimating", { namespace: "navbar" })
    isAnimating!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("isValidIdentityProvider", { namespace: "user" })
    isValidIdentityProvider!: boolean;

    @Getter("userIsRegistered", { namespace: "user" })
    userIsRegistered!: boolean;

    @Getter("userIsActive", { namespace: "user" })
    userIsActive!: boolean;

    @Getter("patientRetrievalFailed", { namespace: "user" })
    patientRetrievalFailed!: boolean;

    private logger!: ILogger;

    @Watch("$route")
    private onRouteChanged(): void {
        this.clearOverlay();
    }

    @Watch("isOpen")
    @Watch("isMobileWidth")
    private onIsSidebarOpenChange(isOpen: boolean): void {
        if (this.isMobileWidth && isOpen) {
            document.body.classList.add("overflow-hidden");
        } else {
            document.body.classList.remove("overflow-hidden");
        }
    }

    private created(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }

    private async mounted(): Promise<void> {
        await this.$nextTick();

        // set up listener to monitor sidebar collapsing and expanding
        let sidebar = document.querySelector("#sidebar");
        sidebar?.addEventListener("transitionend", (event: Event) => {
            let transitionEvent = event as TransitionEvent;
            if (
                sidebar !== transitionEvent.target ||
                transitionEvent.propertyName !== "max-width"
            ) {
                return;
            }

            this.setSidebarStoppedAnimating();
        });
    }

    private toggleOpen(): void {
        this.toggleSidebar();
    }

    private clearOverlay(): void {
        if (this.isOverlayVisible) {
            this.toggleSidebar();
        }
    }

    private get exportTutorialPreference(): string {
        return UserPreferenceType.TutorialMenuExport;
    }

    private get isQueuePage(): boolean {
        return (
            this.$route.path.toLowerCase() === "/queue" ||
            this.$route.path.toLowerCase() === "/busy"
        );
    }

    private get isSidebarAvailable(): boolean {
        return (
            !this.isOffline &&
            this.oidcIsAuthenticated &&
            this.isValidIdentityProvider &&
            this.userIsRegistered &&
            this.userIsActive &&
            !this.patientRetrievalFailed &&
            !this.isQueuePage
        );
    }

    private get showExportTutorial(): boolean {
        return (this.isOpen || !this.isMobileWidth) && !this.isAnimating;
    }

    private get isFullyOpen(): boolean {
        return this.isOpen && !this.isAnimating;
    }

    private get isOverlayVisible(): boolean {
        return this.isOpen && this.isMobileWidth;
    }

    private get isHome(): boolean {
        return this.$route.path == "/home";
    }

    private get isTimeline(): boolean {
        return this.$route.path == "/timeline";
    }

    private get isCovid19(): boolean {
        return this.$route.path == "/covid19";
    }

    private get isServices(): boolean {
        return this.$route.path == "/services";
    }

    private get isTermsOfService(): boolean {
        return this.$route.path == "/profile/termsOfService";
    }

    private get isUnderProfile(): boolean {
        return this.$route.path.startsWith("/profile");
    }

    private get isReports(): boolean {
        return this.$route.path == "/reports";
    }

    private get isDependentEnabled(): boolean {
        return this.config.featureToggleConfiguration.dependents.enabled;
    }

    private get isServicesEnabled(): boolean {
        return this.config.featureToggleConfiguration.services.enabled;
    }

    private get isDependents(): boolean {
        return this.$route.path == "/dependents";
    }
}
</script>

<template>
    <div v-if="isSidebarAvailable" class="wrapper">
        <!-- Sidebar -->
        <nav
            id="sidebar"
            data-testid="sidebar"
            :class="{ collapsed: !isOpen }"
            aria-label="Side Nav"
        >
            <b-row class="row-container" no-gutters>
                <b-col>
                    <!-- Home button -->
                    <hg-button
                        v-show="userIsActive"
                        id="menuBtnHome"
                        data-testid="menu-btn-home-link"
                        to="/home"
                        variant="nav"
                        class="my-3 px-3 px-md-4"
                        :class="{ selected: isHome }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col title="Home" cols="auto" class="pr-md-4">
                                <hg-icon icon="home" size="large" square />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                data-testid="homeLabel"
                                class="button-text pl-3"
                            >
                                <span>Home</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Timeline button -->
                    <hg-button
                        v-show="userIsActive"
                        id="menuBtnTimeline"
                        data-testid="menu-btn-time-line-link"
                        to="/timeline"
                        variant="nav"
                        class="my-3 px-3 px-md-4"
                        :class="{ selected: isTimeline }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col title="Timeline" cols="auto" class="pr-md-4">
                                <hg-icon
                                    icon="box-archive"
                                    size="large"
                                    square
                                />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                data-testid="timelineLabel"
                                class="button-text pl-3"
                            >
                                <span>Timeline</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- COVID-19 button -->
                    <hg-button
                        v-show="userIsActive"
                        id="menuBtnCovid19"
                        data-testid="menu-btn-covid19-link"
                        to="/covid19"
                        variant="nav"
                        class="my-3 px-3 px-md-4"
                        :class="{ selected: isCovid19 }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col title="COVID‑19" cols="auto" class="pr-md-4">
                                <hg-icon
                                    icon="check-circle"
                                    size="large"
                                    square
                                />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                data-testid="covid19Label"
                                class="button-text pl-3"
                            >
                                <span>COVID‑19</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Dependents button -->
                    <hg-button
                        v-show="isDependentEnabled && userIsActive"
                        id="menuBtnDependents"
                        data-testid="menu-btn-dependents-link"
                        to="/dependents"
                        variant="nav"
                        class="my-3 px-3 px-md-4"
                        :class="{ selected: isDependents }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col
                                title="Dependents"
                                cols="auto"
                                class="pr-md-4"
                            >
                                <hg-icon
                                    icon="user-friends"
                                    size="large"
                                    square
                                />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                class="button-text pl-3"
                            >
                                <span>Dependents</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Services button -->
                    <hg-button
                        v-show="isServicesEnabled && userIsActive"
                        id="menuBtnServices"
                        data-testid="menu-btn-services-link"
                        to="/services"
                        variant="nav"
                        class="my-3 px-3 px-md-4"
                        :class="{ selected: isServices }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col title="Services" cols="auto" class="pr-md-4">
                                <hg-icon
                                    icon="fa-solid fa-hand-holding-medical"
                                    size="large"
                                    square
                                />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                data-testid="servicesLabel"
                                class="button-text pl-3"
                            >
                                <span>Services</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Reports button -->
                    <hg-button
                        v-show="userIsActive"
                        id="menuBtnReports"
                        data-testid="menu-btn-reports-link"
                        to="/reports"
                        variant="nav"
                        class="mt-3 px-3 px-md-4"
                        :class="{ selected: isReports }"
                    >
                        <b-row class="align-items-center" no-gutters>
                            <b-col
                                title="Export Records"
                                cols="auto"
                                class="pr-md-4"
                            >
                                <hg-icon
                                    icon="cloud-arrow-down"
                                    size="large"
                                    square
                                />
                            </b-col>
                            <b-col
                                v-show="isFullyOpen"
                                id="export-records-col"
                                class="button-text pl-3"
                            >
                                <span>Export Records</span>
                            </b-col>
                        </b-row>
                        <TutorialComponent
                            :preference-type="exportTutorialPreference"
                            target="menuBtnReports"
                            :show="showExportTutorial"
                            custom-class="elevation-1"
                            placement="right"
                        >
                            <div
                                data-testid="exportRecordsPopover"
                                class="popover-content"
                            >
                                Download and print health records, such as your
                                immunization history and more.
                            </div>
                        </TutorialComponent>
                    </hg-button>
                    <br />
                </b-col>
            </b-row>

            <b-row class="sidebar-footer" no-gutters>
                <b-col>
                    <!-- Collapse Button -->
                    <span v-show="!isMobileWidth">
                        <hr />
                        <hg-button variant="nav" @click="toggleOpen">
                            <b-row
                                class="align-items-center"
                                :class="{ 'mx-2': isOpen }"
                                no-gutters
                            >
                                <b-col
                                    :title="`${
                                        isOpen ? 'Collapse' : 'Expand'
                                    } Menu`"
                                    :class="{ 'ml-auto col-3': isOpen }"
                                >
                                    <hg-icon
                                        icon="angle-double-left"
                                        size="large"
                                        data-testid="sidebarToggle"
                                        class="arrow-icon p-2"
                                        aria-hidden="true"
                                    />
                                </b-col>
                            </b-row>
                        </hg-button>
                    </span>

                    <!-- Feedback section -->
                    <FeedbackComponent />
                </b-col>
            </b-row>
        </nav>

        <!-- Dark Overlay element -->
        <div v-show="isOverlayVisible" class="overlay" @click="toggleOpen" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.wrapper {
    display: flex;
    align-items: stretch;
}

#sidebar {
    min-width: 300px;
    max-width: 300px;
    background: $primary;

    transition: all 0.3s;

    height: 100%;
    z-index: $z_sidebar;
    position: static;
    display: flex;
    flex-direction: column;

    &.collapsed {
        min-width: 72px;
        max-width: 72px;
    }

    /* Small devices */
    @media (max-width: 767px) {
        min-width: 185px;
        max-width: 185px;

        display: absolute;
        position: fixed;
        top: 0px;
        padding-top: 80px;
        overflow-y: auto;

        &.collapsed {
            min-width: 0px !important;
            max-width: 0px !important;
            height: 100vh;
        }

        &.collapsed .row-container {
            display: none;
        }

        &.collapsed .sidebar-footer {
            display: none;
        }

        &.arrow-icon {
            display: none;
        }
    }
}

.button-text {
    text-align: left;
}

hr {
    border-top: 2px solid white;
    margin-left: 10px;
    margin-right: 10px;
}

#sidebar.collapsed .arrow-icon {
    transform: scaleX(-1);
}

.overlay {
    display: block;
    opacity: 1;
    position: fixed;
    /* full screen */
    width: 100vw;
    height: 100vh;
    /* transparent black */
    background: rgba(0, 0, 0, 0.7);
    /* middle layer, i.e. appears below the sidebar */
    z-index: $z_overlay;
    /* animate the transition */
    transition: all 0.5s ease-in-out;
    top: 0px;
    overflow: hidden;
}

.row-container {
    flex: 1 0 auto;
}

.sidebar-footer {
    width: 100%;
    flex-shrink: 0;
    position: sticky;
    position: -webkit-sticky;
    bottom: 0rem;
    align-self: flex-end;
    background-color: $primary;
}

/* Small Devices*/
@media (max-width: 470px) {
    .popover-content {
        max-width: 8rem;
    }
    .bs-popover-right {
        margin-left: 14rem !important;
    }
}
</style>

<style lang="scss">
@import "@/assets/scss/_variables.scss";

.popover.elevation-1 {
    z-index: calc($z_popover + 1);
}
</style>
