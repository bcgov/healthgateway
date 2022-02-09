<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faAngleDoubleLeft,
    faChartLine,
    faCheckCircle,
    faClipboardList,
    faEdit,
    faHome,
    faStream,
    faUserFriends,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import FeedbackComponent from "@/components/feedback.vue";
import UserPreferenceType from "@/constants/userPreferenceType";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import type { UserPreference } from "@/models/userPreference";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger } from "@/services/interfaces";

library.add(
    faAngleDoubleLeft,
    faChartLine,
    faCheckCircle,
    faClipboardList,
    faEdit,
    faHome,
    faStream,
    faUserFriends
);

@Component({
    components: {
        FeedbackComponent,
    },
})
export default class SidebarComponent extends Vue {
    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: { userPreference: UserPreference }) => void;

    @Action("createUserPreference", { namespace: "user" })
    createUserPreference!: (params: { userPreference: UserPreference }) => void;

    @Getter("isMobile")
    isMobileWidth!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Action("toggleSidebar", { namespace: "navbar" })
    toggleSidebar!: () => void;

    @Action("setSidebarState", { namespace: "navbar" })
    setSidebarState!: (isOpen: boolean) => void;

    @Getter("isSidebarOpen", { namespace: "navbar" })
    isOpen!: boolean;

    @Getter("isSidebarShown", { namespace: "navbar" })
    isSidebarShown!: boolean;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("userIsActive", { namespace: "user" })
    isActiveProfile!: boolean;

    private UserPreferenceType = UserPreferenceType;

    private logger!: ILogger;

    private isExportTutorialEnabled = false;

    @Watch("$route")
    private onRouteChanged() {
        this.clearOverlay();
    }

    @Watch("isOpen")
    private onIsOpen(val: boolean) {
        console.log("isOpen", val);
        this.isExportTutorialEnabled = false;
    }

    private created() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.$nextTick(() => {
            if (!this.isMobileWidth) {
                this.setSidebarState(true);
            }
        });
    }

    private mounted() {
        this.$nextTick().then(() => {
            // Setup the transition listener to avoid text wrapping
            var transition = document.querySelector("#sidebar");
            transition?.addEventListener("transitionend", (event: Event) => {
                let transitionEvent = event as TransitionEvent;
                if (
                    transition !== transitionEvent.target ||
                    transitionEvent.propertyName !== "max-width"
                ) {
                    return;
                }

                this.isExportTutorialEnabled = true;

                document.querySelectorAll(".button-text").forEach((button) => {
                    if (transition?.classList.contains("collapsed")) {
                        button?.classList.add("d-none");
                    } else {
                        button?.classList.remove("d-none");
                    }
                });
            });
        });

        this.isExportTutorialEnabled = true;
    }

    private toggleOpen() {
        this.toggleSidebar();
    }

    private clearOverlay() {
        if (this.isOverlayVisible) {
            this.toggleSidebar();
        }
    }

    private dismissTutorial(userPreference: UserPreference) {
        this.logger.debug(
            `Dismissing tutorial ${userPreference.preference}...`
        );
        userPreference.value = "false";
        if (userPreference.hdId != undefined) {
            this.updateUserPreference({
                userPreference: userPreference,
            });
        } else {
            userPreference.hdId = this.user.hdid;
            this.createUserPreference({
                userPreference: userPreference,
            });
        }
    }

    private isPreferenceActive(tutorialPopover: UserPreference): boolean {
        if (this.isMobileWidth) {
            return tutorialPopover?.value === "true" && this.isOpen;
        } else {
            return tutorialPopover?.value === "true";
        }
    }

    private get showExportTutorial(): boolean {
        return (
            this.isPreferenceActive(
                this.user.preferences[UserPreferenceType.TutorialMenuExport]
            ) && this.isExportTutorialEnabled
        );
    }

    private set showExportTutorial(value: boolean) {
        this.isExportTutorialEnabled = value;
    }

    private get isOverlayVisible() {
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

    private get isVaccinationStatusEnabled(): boolean {
        return this.config.modules["VaccinationStatus"];
    }

    private get isTermsOfService(): boolean {
        return this.$route.path == "/profile/termsOfService";
    }

    private get isUnderProfile(): boolean {
        return this.$route.path.startsWith("/profile");
    }

    private get isHealthInsights(): boolean {
        return this.$route.path == "/healthInsights";
    }

    private get isReports(): boolean {
        return this.$route.path == "/reports";
    }

    private get isDependentEnabled(): boolean {
        return this.config.modules["Dependent"];
    }

    private get isDependents(): boolean {
        return this.$route.path == "/dependents";
    }

    private get isPcrTest(): boolean {
        return this.$route.path.toLowerCase().startsWith("/pcrtest");
    }
}
</script>

<template>
    <div v-show="isSidebarShown && !isPcrTest" class="wrapper">
        <!-- Sidebar -->
        <nav id="sidebar" data-testid="sidebar" :class="{ collapsed: !isOpen }">
            <b-row class="row-container">
                <b-col>
                    <!-- Home button -->
                    <hg-button
                        v-show="isActiveProfile"
                        id="menuBtnHome"
                        data-testid="menu-btn-home-link"
                        to="/home"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isHome }"
                    >
                        <b-row class="align-items-center">
                            <b-col title="Home" :class="{ 'col-3': isOpen }">
                                <hg-icon icon="home" size="large" />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                data-testid="homeLabel"
                                class="button-text"
                            >
                                <span>Home</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Timeline button -->
                    <hg-button
                        v-show="isActiveProfile"
                        id="menuBtnTimeline"
                        data-testid="menu-btn-time-line-link"
                        to="/timeline"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isTimeline }"
                    >
                        <b-row class="align-items-center">
                            <b-col
                                title="Timeline"
                                :class="{ 'col-3': isOpen }"
                            >
                                <hg-icon icon="stream" size="large" />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                data-testid="timelineLabel"
                                class="button-text"
                            >
                                <span>Timeline</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- COVID-19 button -->
                    <hg-button
                        v-show="isVaccinationStatusEnabled && isActiveProfile"
                        id="menuBtnCovid19"
                        data-testid="menu-btn-covid19-link"
                        to="/covid19"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isCovid19 }"
                    >
                        <b-row class="align-items-center">
                            <b-col
                                title="COVID-19"
                                :class="{ 'col-3': isOpen }"
                            >
                                <hg-icon icon="check-circle" size="large" />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                data-testid="covid19Label"
                                class="button-text"
                            >
                                <span>COVID-19</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Dependents button -->
                    <hg-button
                        v-show="isDependentEnabled && isActiveProfile"
                        id="menuBtnDependents"
                        data-testid="menu-btn-dependents-link"
                        to="/dependents"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isDependents }"
                    >
                        <b-row class="align-items-center">
                            <b-col title="Reports" :class="{ 'col-3': isOpen }">
                                <hg-icon icon="user-friends" size="large" />
                            </b-col>
                            <b-col v-show="isOpen" class="button-text">
                                <span>Dependents</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <!-- Reports button -->
                    <hg-button
                        v-show="isActiveProfile"
                        id="menuBtnReports"
                        data-testid="menu-btn-reports-link"
                        to="/reports"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isReports }"
                    >
                        <b-row
                            id="export-records-row"
                            class="align-items-center"
                        >
                            <b-col
                                title="Export Records"
                                :class="{ 'col-3': isOpen }"
                            >
                                <hg-icon icon="clipboard-list" size="large" />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                id="export-records-col"
                                class="button-text"
                            >
                                <span>Export Records</span>
                            </b-col>
                        </b-row>
                        <b-popover
                            triggers="manual"
                            :show.sync="showExportTutorial"
                            target="export-records-row"
                            custom-class="popover-style"
                            fallback-placement="clockwise"
                            placement="right"
                            variant="dark"
                            boundary="viewport"
                        >
                            <div>
                                <hg-button
                                    class="float-right text-dark p-0 ml-2"
                                    variant="icon"
                                    @click="
                                        dismissTutorial(
                                            user.preferences[
                                                UserPreferenceType
                                                    .TutorialMenuExport
                                            ]
                                        )
                                    "
                                    >×</hg-button
                                >
                            </div>
                            <div
                                data-testid="exportRecordsPopover"
                                class="popover-content"
                            >
                                Download a pdf of your records. e.g. COVID-19
                                test proof for employers.
                            </div>
                        </b-popover>
                    </hg-button>
                    <!-- Health Insights button -->
                    <hg-button
                        v-show="isActiveProfile"
                        id="menuBtnHealthInsights"
                        data-testid="menu-btn-health-insights-link"
                        to="/healthInsights"
                        variant="nav"
                        class="my-3"
                        :class="{ selected: isHealthInsights }"
                    >
                        <b-row class="align-items-center">
                            <b-col
                                title="Health Insights"
                                :class="{ 'col-3': isOpen }"
                            >
                                <hg-icon icon="chart-line" size="large" />
                            </b-col>
                            <b-col v-show="isOpen" class="button-text">
                                <span>Health Insights</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <br />
                </b-col>
            </b-row>

            <b-row class="sidebar-footer m-0 p-0">
                <b-col class="m-0 p-0">
                    <!-- Collapse Button -->
                    <span v-show="!isMobileWidth">
                        <hr />
                        <hg-button variant="nav" @click="toggleOpen">
                            <b-row
                                class="align-items-center"
                                :class="[isOpen ? 'mx-2' : '']"
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
        <div
            v-show="isOverlayVisible"
            class="overlay"
            @click="toggleOpen"
        ></div>
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
        min-width: 80px;
        max-width: 80px;
    }

    /* Small devices */
    @media (max-width: 767px) {
        min-width: 185px;
        max-width: 185px;

        display: absolute;
        position: fixed;
        top: 0px;
        padding-top: 80px;
        overflow-y: scroll;

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
.popover-style {
    z-index: $z_popover;
}
</style>
