<script setup lang="ts">
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
import { computed, nextTick, onMounted, watch } from "vue";
import { useRoute, useStore } from "vue-composition-wrapper";

import FeedbackComponent from "@/components/FeedbackComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";

library.add(
    faAngleDoubleLeft,
    faBoxArchive,
    faCheckCircle,
    faCloudArrowDown,
    faHandHoldingMedical,
    faHome,
    faUserFriends
);

const route = useRoute();
const store = useStore();

const isMobileWidth = computed<boolean>(() => store.getters["isMobile"]);
const isOffline = computed<boolean>(() => store.getters["config/isOffline"]);
const config = computed<WebClientConfiguration>(
    () => store.getters["config/webClient"]
);
const isOpen = computed<boolean>(() => store.getters["navbar/isSidebarOpen"]);
const isAnimating = computed<boolean>(
    () => store.getters["navbar/isSidebarAnimating"]
);
const oidcIsAuthenticated = computed<boolean>(
    () => store.getters["auth/oidcIsAuthenticated"]
);
const isValidIdentityProvider = computed<boolean>(
    () => store.getters["user/isValidIdentityProvider"]
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

const isQueuePage = computed(
    () =>
        route.value.path.toLowerCase() === "/queue" ||
        route.value.path.toLowerCase() === "/busy"
);
const isSidebarAvailable = computed(
    () =>
        !isOffline.value &&
        oidcIsAuthenticated.value &&
        isValidIdentityProvider.value &&
        userIsRegistered.value &&
        userIsActive.value &&
        !patientRetrievalFailed.value &&
        !isQueuePage.value
);
const isFullyOpen = computed(() => isOpen.value && !isAnimating.value);
const isOverlayVisible = computed(() => isOpen.value && isMobileWidth.value);
const isHome = computed(() => route.value.path == "/home");
const isTimeline = computed(() => route.value.path == "/timeline");
const isCovid19 = computed(() => route.value.path == "/covid19");
const isServices = computed(() => route.value.path == "/services");
const isReports = computed(() => route.value.path == "/reports");
const isDependentEnabled = computed(
    () => config.value.featureToggleConfiguration.dependents.enabled
);
const isServicesEnabled = computed(
    () => config.value.featureToggleConfiguration.services.enabled
);
const isDependents = computed(() => route.value.path == "/dependents");

function toggleSidebar(): void {
    store.dispatch("navbar/toggleSidebar");
}

function setSidebarStoppedAnimating(): void {
    store.dispatch("navbar/setSidebarStoppedAnimating");
}

function handleSidebarChanged(): void {
    if (isMobileWidth.value && isOpen.value) {
        document.body.classList.add("overflow-hidden");
    } else {
        document.body.classList.remove("overflow-hidden");
    }
}

function clearOverlay(): void {
    if (isOverlayVisible.value) {
        toggleSidebar();
    }
}

watch(route, () => clearOverlay());
watch([isOpen, isMobileWidth], () => handleSidebarChanged());

onMounted(async () => {
    await nextTick();

    // set up listener to monitor sidebar collapsing and expanding
    const sidebar = document.querySelector("#sidebar");
    sidebar?.addEventListener("transitionend", (event: Event) => {
        const transitionEvent = event as TransitionEvent;
        if (
            sidebar !== transitionEvent.target ||
            transitionEvent.propertyName !== "max-width"
        ) {
            return;
        }

        setSidebarStoppedAnimating();
    });
});
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
                        data-testid="menu-btn-timeline-link"
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
                    </hg-button>
                    <br />
                </b-col>
            </b-row>

            <b-row class="sidebar-footer" no-gutters>
                <b-col>
                    <!-- Collapse Button -->
                    <span v-show="!isMobileWidth">
                        <hr />
                        <hg-button variant="nav" @click="toggleSidebar">
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
        <div v-show="isOverlayVisible" class="overlay" @click="toggleSidebar" />
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
