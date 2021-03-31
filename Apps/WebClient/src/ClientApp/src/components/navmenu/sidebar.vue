<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faQuestion, faStream } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import FeedbackComponent from "@/components/feedback.vue";
import UserPreferenceType from "@/constants/userPreferenceType";
import EventBus, { EventMessageName } from "@/eventbus";
import type { WebClientConfiguration } from "@/models/configData";
import User from "@/models/user";
import type { UserPreference } from "@/models/userPreference";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
library.add(faStream, faQuestion);

const auth = "auth";
const user = "user";
const navbar = "navbar";
const config = "config";

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

    @Action("toggleSidebar", { namespace: navbar }) toggleSidebar!: () => void;

    @Action("setSidebarState", { namespace: navbar }) setSidebarState!: (
        isOpen: boolean
    ) => void;

    @Getter("isMobile") isMobileWidth!: boolean;

    @Getter("isSidebarOpen", { namespace: navbar }) isOpen!: boolean;

    @Getter("oidcIsAuthenticated", {
        namespace: auth,
    })
    oidcIsAuthenticated!: boolean;

    @Getter("userIsRegistered", {
        namespace: user,
    })
    userIsRegistered!: boolean;

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isValidIdentityProvider", {
        namespace: auth,
    })
    isValidIdentityProvider!: boolean;

    @Getter("userIsActive", { namespace: "user" })
    isActiveProfile!: boolean;

    @Getter("isOffline", {
        namespace: config,
    })
    isOffline!: boolean;

    private UserPreferenceType = UserPreferenceType;

    private eventBus = EventBus;

    private logger!: ILogger;

    private isNoteTutorialEnabled = false;
    private isExportTutorialEnabled = false;

    @Watch("$route")
    private onRouteChanged() {
        this.clearOverlay();
    }

    @Watch("isOpen")
    private onIsOpen() {
        this.isNoteTutorialEnabled = false;
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

            this.isNoteTutorialEnabled = true;
            this.isExportTutorialEnabled = true;

            document.querySelectorAll(".button-title").forEach((button) => {
                if (transition?.classList.contains("collapsed")) {
                    button?.classList.add("d-none");
                } else {
                    button?.classList.remove("d-none");
                }
            });
        });
    }

    private toggleOpen() {
        this.toggleSidebar();
    }

    private clearOverlay() {
        if (this.isOverlayVisible) {
            this.toggleSidebar();
        }
    }

    private createNote() {
        this.clearOverlay();
        this.eventBus.$emit(EventMessageName.CreateNote);
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

    private get showNoteTutorial(): boolean {
        return (
            this.isPreferenceActive(
                this.user.preferences[UserPreferenceType.TutorialMenuNote]
            ) &&
            this.isNoteTutorialEnabled &&
            this.isTimeline &&
            this.isActiveProfile
        );
    }

    private set showNoteTutorial(value: boolean) {
        this.isNoteTutorialEnabled = value;
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

    private get isTimeline(): boolean {
        return this.$route.path == "/timeline";
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

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }

    private get isDependentEnabled(): boolean {
        return this.config.modules["Dependent"];
    }

    private get isDependents(): boolean {
        return this.$route.path == "/dependents";
    }

    private get isFAQ(): boolean {
        return this.$route.path == "/faq";
    }
}
</script>

<template>
    <div
        v-show="
            oidcIsAuthenticated &&
            userIsRegistered &&
            isValidIdentityProvider &&
            !isOffline
        "
        class="wrapper"
    >
        <!-- Sidebar -->
        <nav id="sidebar" data-testid="sidebar" :class="{ collapsed: !isOpen }">
            <b-row class="row-container m-0 p-0">
                <b-col class="m-0 p-0">
                    <!-- Timeline button -->
                    <router-link
                        v-show="isActiveProfile"
                        id="menuBtnTimeline"
                        data-testid="menuBtnTimelineLink"
                        to="/timeline"
                        class="my-4"
                    >
                        <b-row
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isTimeline }"
                        >
                            <b-col
                                v-show="isOpen"
                                cols="1"
                                class="button-spacer"
                            ></b-col>
                            <b-col
                                title="Timeline"
                                :class="{ 'col-3': isOpen }"
                            >
                                <font-awesome-icon
                                    icon="stream"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                data-testid="timelineLabel"
                                cols="7"
                                class="button-title"
                            >
                                <span>Timeline</span>
                            </b-col>
                        </b-row>
                    </router-link>
                    <div v-show="isTimeline && isActiveProfile">
                        <!-- Note button -->
                        <b-row
                            v-show="isNoteEnabled"
                            id="add-a-note-row"
                            data-testid="addNoteBtn"
                            class="align-items-center border rounded-pill py-2 button-container my-4"
                            :class="{ 'sub-menu': isOpen }"
                            @click="createNote"
                        >
                            <b-col
                                id="add-a-note-btn"
                                title="Add a Note"
                                :class="{ 'col-4': isOpen }"
                            >
                                <font-awesome-icon
                                    icon="edit"
                                    class="button-icon sub-menu m-auto"
                                    size="2x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                cols="8"
                                class="button-title sub-menu"
                            >
                                <span>Add a Note</span>
                            </b-col>
                        </b-row>
                        <b-popover
                            ref="popover"
                            triggers="manual"
                            :show.sync="showNoteTutorial"
                            target="add-a-note-row"
                            custom-class="popover-style"
                            fallback-placement="clockwise"
                            placement="right"
                            variant="dark"
                            boundary="viewport"
                        >
                            <div>
                                <b-button
                                    class="pop-over-close"
                                    @click="
                                        dismissTutorial(
                                            user.preferences[
                                                UserPreferenceType
                                                    .TutorialMenuNote
                                            ]
                                        )
                                    "
                                    >x</b-button
                                >
                            </div>
                            <div
                                data-testid="notesPopover"
                                class="popover-content"
                            >
                                Add Notes to track your important health events
                                e.g. Broke ankle in Cuba
                            </div>
                        </b-popover>
                    </div>
                    <router-link
                        v-show="isDependentEnabled && isActiveProfile"
                        id="menuBtnDependents"
                        data-testid="menuBtnDependentsLink"
                        to="/dependents"
                        class="my-4"
                    >
                        <b-row
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isDependents }"
                        >
                            <b-col
                                v-show="isOpen"
                                cols="1"
                                class="button-spacer"
                            ></b-col>
                            <b-col title="Reports" :class="{ 'col-3': isOpen }">
                                <font-awesome-icon
                                    icon="user-friends"
                                    class="button-icon"
                                    size="2x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                cols="7"
                                class="button-title"
                            >
                                <span>Dependents</span>
                            </b-col>
                        </b-row>
                    </router-link>
                    <!-- Reports button -->
                    <router-link
                        v-show="isActiveProfile"
                        id="menuBtnReports"
                        data-testid="menuBtnReportsLink"
                        to="/reports"
                        class="my-4"
                    >
                        <b-row
                            id="export-records-row"
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isReports }"
                        >
                            <b-col
                                v-show="isOpen"
                                cols="1"
                                class="button-spacer"
                            ></b-col>
                            <b-col
                                title="Export Records"
                                :class="{ 'col-3': isOpen }"
                            >
                                <font-awesome-icon
                                    icon="clipboard-list"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                id="export-records-col"
                                cols="7"
                                class="button-title"
                            >
                                <span>Export Records</span>
                            </b-col>
                        </b-row>
                        <b-popover
                            ref="popover-export-records"
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
                                <b-button
                                    class="pop-over-close"
                                    @click="
                                        dismissTutorial(
                                            user.preferences[
                                                UserPreferenceType
                                                    .TutorialMenuExport
                                            ]
                                        )
                                    "
                                    >x</b-button
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
                    </router-link>
                    <!-- Health Insights button -->
                    <router-link
                        v-show="isActiveProfile"
                        id="menuBtnHealthInsights"
                        data-testid="menuBtnHealthInsightsLink"
                        to="/healthInsights"
                        class="my-4"
                    >
                        <b-row
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isHealthInsights }"
                        >
                            <b-col
                                v-show="isOpen"
                                cols="1"
                                class="button-spacer"
                            ></b-col>
                            <b-col
                                title="Health Insights"
                                :class="{ 'col-3': isOpen }"
                            >
                                <font-awesome-icon
                                    icon="chart-line"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                cols="7"
                                class="button-title"
                            >
                                <span>Health Insights</span>
                            </b-col>
                        </b-row>
                    </router-link>

                    <!-- FAQ button -->
                    <router-link
                        id="menuBtnFAQ"
                        data-testid="menuBtnFAQ"
                        to="/faq"
                        class="my-4"
                    >
                        <b-row
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isFAQ }"
                        >
                            <b-col
                                v-show="isOpen"
                                cols="1"
                                class="button-spacer"
                            ></b-col>
                            <b-col title="FAQ" :class="{ 'col-3': isOpen }">
                                <font-awesome-icon
                                    icon="question"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-show="isOpen"
                                cols="7"
                                class="button-title"
                            >
                                <span>FAQ</span>
                            </b-col>
                        </b-row>
                    </router-link>
                    <br />
                </b-col>
            </b-row>

            <b-row class="sidebar-footer m-0 p-0">
                <b-col class="m-0 p-0">
                    <!-- Collapse Button -->
                    <span v-show="!isMobileWidth">
                        <hr />
                        <b-row
                            class="align-items-center my-4"
                            :class="[isOpen ? 'mx-4' : 'button-container']"
                        >
                            <b-col
                                :title="`${
                                    isOpen ? 'Collapse' : 'Expand'
                                } Menu`"
                                :class="{ 'ml-auto col-2': isOpen }"
                            >
                                <font-awesome-icon
                                    data-testid="sidebarToggle"
                                    class="arrow-icon p-2"
                                    icon="angle-double-left"
                                    aria-hidden="true"
                                    size="3x"
                                    @click="toggleOpen"
                                />
                            </b-col>
                        </b-row>
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

.sidebarDivider {
    border-top: 10px solid;
    color: white;
}

.wrapper {
    display: flex;
    align-items: stretch;
}

#sidebar {
    min-width: 300px;
    max-width: 300px;
    background: $primary;
    color: #fff;
    transition: all 0.3s;
    text-align: center;
    height: 100%;
    z-index: $z_sidebar;
    position: static;
    display: flex;
    flex-direction: column;
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

#sidebar.collapsed {
    min-width: 80px;
    max-width: 80px;
}

#sidebar.collapsed .button-container {
    border-color: $primary !important;
    margin: 0px;
    border-radius: 0px !important;
}

#sidebar .button-container {
    margin-right: 0em;
    &.sub-menu {
        margin-left: 5em;
        margin-right: 2em;
        @media (max-width: 767px) {
            margin-left: 3em;
            margin-right: 0.5em;
        }
    }
    &.selected {
        background-color: $lightBlue;
        .button-spacer {
            background-color: $aquaBlue !important;
        }
    }
}

#sidebar .button-spacer {
    width: 100%;
    height: 100%;
}

#sidebar .button-container:hover {
    text-decoration: underline;
    cursor: pointer;
    background-color: $lightBlue !important;
}

#sidebar .button-icon {
    display: inline-block;
    margin: auto !important;
    &.sub-menu {
        font-size: 1.3em;
    }
    @media (max-width: 767px) {
        font-size: 1.5em;
        &.sub-menu {
            font-size: 1em;
        }
    }
}

#sidebar .button-title {
    display: inline;
    font-size: 1.3em;
    text-align: left;
    margin: 0px;
    padding: 0px;
    &.sub-menu {
        font-size: 1em;
    }
    @media (max-width: 767px) {
        font-size: 1.1em;
        &.sub-menu {
            font-size: 0.8em;
        }
    }
}

#sidebar .name-wrapper {
    height: 70px;
    display: flex;
    align-items: center;
    @media (max-width: 767px) {
        height: 55px;
    }
}

#sidebar hr {
    border-top: 2px solid white;
    margin-left: 10px;
    margin-right: 10px;
}

#sidebar.collapsed .arrow-icon {
    transform: scaleX(-1);
}

#sidebar .arrow-icon:hover {
    text-decoration: underline;
    cursor: pointer;
    background-color: $lightBlue !important;
}

#sidebar a {
    text-decoration: none;
    color: white !important;
    caret-color: white !important;
}

#sidebar a:hover {
    text-decoration: underline;
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

#sidebar .row-container {
    flex: 1 0 auto;
}

#sidebar .sidebar-footer {
    width: 100%;
    flex-shrink: 0;
    position: sticky;
    bottom: 0rem;
    align-self: flex-end;
    background-color: $primary;
}

.popover-body {
    padding: 0.5px 0px 0.5px 0px !important;
}

.pop-over-close {
    float: right;
    padding-top: 0px;
    color: black;
    border: none;
    background-color: transparent;
}

.popover-content {
    max-width: 20rem;
    color: black;
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
