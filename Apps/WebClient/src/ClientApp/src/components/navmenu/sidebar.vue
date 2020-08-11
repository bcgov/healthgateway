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
    border-top: 1px solid $soft_text;
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

/* Small Devices*/
@media (max-width: 767px) {
    #sidebar {
        display: absolute;
        position: fixed;
        top: 0px;
        padding-top: 80px;
        overflow-y: scroll;
    }

    #sidebar.collapsed {
        min-width: 0px;
        max-width: 0px;
        height: 100vh;
    }

    #sidebar.collapsed .row-container {
        display: none;
    }

    #sidebar.collapsed .sidebar-footer {
        display: none;
    }

    #sidebar .arrow-icon {
        display: none;
    }
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
}
</style>

<template>
    <div v-show="oidcIsAuthenticated && userIsRegistered" class="wrapper">
        <!-- Sidebar -->
        <nav id="sidebar" :class="{ collapsed: !isOpen }">
            <b-row class="row-container m-0 p-0">
                <b-col class="m-0 p-0">
                    <!-- Profile Button -->
                    <router-link id="menuBtnProfile" to="/profile" class="my-4">
                        <b-row
                            class="align-items-center name-wrapper my-4 button-container"
                            :class="{ selected: isProfile }"
                        >
                            <b-col
                                v-show="isOpen"
                                class="button-spacer"
                                cols="1"
                            ></b-col>
                            <b-col title="Profile" :class="{ 'col-3': isOpen }">
                                <font-awesome-icon
                                    icon="user-circle"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-if="isOpen"
                                cols="7"
                                class="button-title d-none"
                                >{{ name }}</b-col
                            >
                        </b-row>
                    </router-link>
                    <div v-show="isProfile">
                        <!-- Terms of Service button -->
                        <router-link
                            id="termsOfService"
                            variant="primary"
                            to="/termsOfService"
                            class="p-0"
                        >
                            <b-row
                                class="align-items-center border rounded-pill p-2 button-container my-4"
                                :class="{ 'sub-menu': isOpen }"
                            >
                                <b-col
                                    title="Terms of Service"
                                    :class="{ 'col-4': isOpen }"
                                >
                                    <font-awesome-icon
                                        icon="file-alt"
                                        class="button-icon sub-menu"
                                        size="2x"
                                    />
                                </b-col>
                                <b-col
                                    v-if="isOpen"
                                    cols="8"
                                    class="button-title sub-menu d-none"
                                >
                                    <span>Terms of Service</span>
                                </b-col>
                            </b-row>
                        </router-link>

                        <!-- Release Notes Button -->
                        <b-link
                            id="releaseNotes"
                            variant="primary"
                            href="https://github.com/bcgov/healthgateway/wiki"
                            target="_blank"
                        >
                            <b-row
                                class="align-items-center border rounded-pill p-2 button-container my-4"
                                :class="{ 'sub-menu': isOpen }"
                            >
                                <b-col
                                    title="Release Notes"
                                    :class="{ 'col-4': isOpen }"
                                >
                                    <font-awesome-icon
                                        icon="chart-bar"
                                        class="button-icon sub-menu m-auto"
                                        size="2x"
                                    />
                                </b-col>
                                <b-col
                                    v-if="isOpen"
                                    cols="8"
                                    class="button-title sub-menu d-none"
                                >
                                    <span>Release Notes</span>
                                </b-col>
                            </b-row>
                        </b-link>
                    </div>
                    <!-- Timeline button -->
                    <router-link
                        id="menuBtnTimeline"
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
                                    icon="clipboard-list"
                                    class="button-icon"
                                    size="3x"
                                />
                            </b-col>
                            <b-col
                                v-if="isOpen"
                                cols="7"
                                class="button-title d-none"
                            >
                                <span>Timeline</span>
                            </b-col>
                        </b-row>
                    </router-link>
                    <div v-show="isTimeline">
                        <!-- Note button -->
                        <b-row
                            v-show="isNoteEnabled"
                            id="add-a-note-row"
                            class="align-items-center border rounded-pill py-2 button-container my-4"
                            :class="{ 'sub-menu': isOpen }"
                            @click="createNote"
                        >
                            <b-col
                                id="add-a-note-btn"
                                title="Add a Note todo"
                                :class="{ 'col-4': isOpen }"
                            >
                                <font-awesome-icon
                                    icon="edit"
                                    class="button-icon sub-menu m-auto"
                                    size="2x"
                                />
                            </b-col>
                            <b-col
                                v-if="isOpen"
                                cols="8"
                                class="button-title sub-menu d-none"
                            >
                                <span>Add a Note</span>
                            </b-col>
                        </b-row>
                        <b-popover
                            ref="popover"
                            triggers="manual"
                            :show.sync="showTutorialPopover"
                            target="add-a-note-row"
                            class="popover"
                            fallback-placement="clockwise"
                            placement="right"
                            variant="dark"
                            boundary="viewport"
                        >
                            <div>
                                <b-button
                                    class="pop-over-close"
                                    @click="dismissTutorial"
                                    >x</b-button
                                >
                            </div>
                            <div class="popover-content">
                                Add Notes to track your important health events
                                e.g. Broke ankle in Cuba
                            </div>
                        </b-popover>
                        <!-- Print Button -->
                        <b-row
                            class="align-items-center border rounded-pill py-2 button-container my-4"
                            :class="{ 'sub-menu': isOpen }"
                            @click="printView"
                        >
                            <b-col title="Print" :class="{ 'col-4': isOpen }">
                                <font-awesome-icon
                                    icon="print"
                                    class="button-icon sub-menu m-auto"
                                    size="2x"
                                />
                            </b-col>
                            <b-col
                                v-if="isOpen"
                                cols="8"
                                class="button-title sub-menu d-none"
                            >
                                <span>Print</span>
                            </b-col>
                        </b-row>
                    </div>
                    <br />
                </b-col>
            </b-row>

            <b-row class="sidebar-footer m-0 p-0">
                <b-col class="m-0 p-0">
                    <!-- Collapse Button -->
                    <b-row
                        class="align-items-center my-4"
                        :class="[isOpen ? 'mx-4' : 'button-container']"
                    >
                        <b-col class :class="{ 'ml-auto col-4': isOpen }">
                            <font-awesome-icon
                                class="arrow-icon p-2"
                                icon="angle-double-left"
                                aria-hidden="true"
                                size="3x"
                                @click="toggleOpen"
                            />
                        </b-col>
                    </b-row>
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

<script lang="ts">
import Vue from "vue";
import { Component, Prop, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import { ILogger, IAuthenticationService } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import VueRouter, { Route } from "vue-router";
import EventBus from "@/eventbus";
import { WebClientConfiguration } from "@/models/configData";
import FeedbackComponent from "@/components/feedback.vue";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faStream } from "@fortawesome/free-solid-svg-icons";
import User from "@/models/user";
library.add(faStream);

const auth: string = "auth";
const user: string = "user";
const sidebar: string = "sidebar";

@Component({
    components: {
        FeedbackComponent,
    },
})
export default class SidebarComponent extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Action("updateUserPreference", { namespace: "user" })
    updateUserPreference!: (params: {
        hdid: string;
        name: string;
        value: string;
    }) => void;

    @Action("toggleSidebar", { namespace: sidebar }) toggleSidebar!: () => void;

    @Getter("isOpen", { namespace: sidebar }) isOpen!: boolean;

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

    private eventBus = EventBus;

    private authenticationService!: IAuthenticationService;
    private name: string = "";
    private windowWidth: number = 0;

    private isTutorialEnabled: boolean = false;

    @Watch("oidcIsAuthenticated")
    private onPropertyChanged() {
        // If there is no name in the scope, retrieve it from the service.
        if (this.oidcIsAuthenticated && !this.name) {
            this.loadName();
        }
    }

    @Watch("$route")
    private onRouteChanged() {
        this.clearOverlay();
    }

    @Watch("isOpen")
    private onIsOpen(newValue: boolean, oldValue: boolean) {
        this.isTutorialEnabled = false;
    }

    private mounted() {
        this.authenticationService = container.get(
            SERVICE_IDENTIFIER.AuthenticationService
        );
        if (this.oidcIsAuthenticated) {
            this.loadName();
        }

        var self = this;

        // Setup the transition listener to avoid text wrapping
        var transition = document.querySelector("#sidebar");
        transition?.addEventListener("transitionend", function (event: Event) {
            let transitionEvent = event as TransitionEvent;
            if (
                transition !== transitionEvent.target ||
                transitionEvent.propertyName !== "max-width"
            ) {
                return;
            } else {
                self.isTutorialEnabled = false;
            }

            self.isTutorialEnabled = true;

            var buttonText = document
                .querySelectorAll(".button-title")
                .forEach((button) => {
                    if (transition?.classList.contains("collapsed")) {
                        button?.classList.add("d-none");
                    } else {
                        button?.classList.remove("d-none");
                    }
                });
        });

        this.$nextTick(() => {
            window.addEventListener("resize", this.onResize);
        });
        this.windowWidth = window.innerWidth;
    }

    private beforeDestroy() {
        window.removeEventListener("resize", this.onResize);
    }

    private toggleOpen() {
        this.toggleSidebar();
    }

    private loadName(): void {
        this.authenticationService.getOidcUserProfile().then((oidcUser) => {
            if (oidcUser) {
                this.name = this.getFullname(
                    oidcUser.given_name,
                    oidcUser.family_name
                );
            }
            this.isTutorialEnabled = true;
        });
    }

    private getFullname(firstName: string, lastName: string): string {
        return firstName + " " + lastName;
    }

    private clearOverlay() {
        if (this.isOverlayVisible) {
            this.toggleSidebar();
        }
    }

    private createNote() {
        this.clearOverlay();
        this.eventBus.$emit("timelineCreateNote");
    }

    private dismissTutorial() {
        this.logger.debug("Dismissing tutorial...");
        this.updateUserPreference({
            hdid: this.user.hdid,
            name: "tutorialPopover",
            value: "false",
        });
    }

    private printView() {
        this.clearOverlay();
        this.eventBus.$emit("timelinePrintView");
    }

    private onResize() {
        this.windowWidth = window.innerWidth;
    }

    private get showTutorialPopover(): boolean {
        if (this.isMobileWidth) {
            return (
                this.isTutorialEnabled &&
                this.user.preferences["tutorialPopover"] === "true" &&
                this.isOpen
            );
        } else {
            return (
                this.isTutorialEnabled &&
                this.user.preferences["tutorialPopover"] === "true"
            );
        }
    }

    private set showTutorialPopover(value: boolean) {
        this.isTutorialEnabled = value;
    }

    private get isOverlayVisible() {
        return this.isOpen && this.isMobileWidth;
    }

    private get isMobileWidth(): boolean {
        return this.windowWidth < 768;
    }

    private get isTimeline(): boolean {
        let isTimeLine = this.$route.path == "/timeline";
        if (isTimeLine && !this.isTutorialEnabled)
            this.isTutorialEnabled = true;
        return isTimeLine;
    }

    private get isProfile(): boolean {
        return this.$route.path == "/profile";
    }

    private get isNoteEnabled(): boolean {
        return this.config.modules["Note"];
    }
}
</script>
