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

#sidebar .button-container:hover {
  text-decoration: underline;
  cursor: pointer;
  background-color: $lightBlue !important;
}

#sidebar .button-icon {
  display: inline-block;
  margin: auto !important;
}

#sidebar .button-title {
  display: inline;
  font-size: 1.3em;
  text-align: left;
  margin: 0px;
  padding: 0px;
}

#sidebar .name-wrapper {
  height: 70px;
  display: flex;
  align-items: center;
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
  }

  #sidebar.collapsed {
    min-width: 0px;
    max-width: 0px;
    height: 100vh;
  }

  #sidebar.collapsed .row-container {
    display: none;
  }

  #sidebar .arrow-icon {
    display: none;
  }
}
</style>

<template>
  <div v-show="oidcIsAuthenticated" class="wrapper">
    <!-- Sidebar -->
    <nav id="sidebar" :class="{ collapsed: !isOpen }">
      <b-row class="row-container m-0 p-0">
        <b-col class="m-0 p-0">
          <!-- Profile Button -->
          <router-link id="menuBtnProfile" to="/profile" class="my-4">
            <b-row
              class="align-items-center name-wrapper my-4 button-container"
              :class="{ 'm-4': isOpen }"
            >
              <b-col class="" :class="{ 'col-4': isOpen }">
                <font-awesome-icon
                  icon="user-circle"
                  class="button-icon"
                  size="3x"
                />
              </b-col>
              <b-col v-if="isOpen" cols="8" class="button-title d-none">
                {{ name }}
              </b-col>
            </b-row>
          </router-link>

          <hr class="mb-3 mt-0 p-2" />

          <!-- Timeline button -->
          <b-row
            v-show="!isTimeline"
            class="align-items-center border rounded-pill p-1 button-container  my-4"
            :class="{ 'mx-4': isOpen }"
            @click="goToTimeline"
          >
            <b-col :class="{ 'col-4': isOpen }">
              <font-awesome-icon icon="stream" class="button-icon" size="2x" />
            </b-col>
            <b-col v-if="isOpen" cols="8" class="button-title d-none">
              <span>Timeline</span>
            </b-col>
          </b-row>

          <div v-show="isTimeline">
            <!-- Note button -->
            <b-row
              v-show="isNoteEnabled"
              class="align-items-center border rounded-pill p-1 button-container  my-4"
              :class="{ 'mx-4': isOpen }"
              @click="createNote"
            >
              <b-col :class="{ 'col-4': isOpen }">
                <font-awesome-icon icon="edit" class="button-icon" size="2x" />
              </b-col>
              <b-col v-if="isOpen" cols="8" class="button-title d-none">
                <span>Add a Note</span>
              </b-col>
            </b-row>

            <!-- Print Button -->
            <b-row
              class="align-items-center border rounded-pill p-1 button-container my-4"
              :class="{ 'mx-4': isOpen }"
              @click="printView"
            >
              <b-col class="" :class="{ 'col-4': isOpen }">
                <font-awesome-icon
                  icon="print"
                  class="button-icon m-auto"
                  size="2x"
                />
              </b-col>
              <b-col v-if="isOpen" cols="8" class="button-title d-none">
                <span>Print</span>
              </b-col>
            </b-row>
          </div>

          <br />

          <!-- Collapse Button -->
          <b-row
            class="align-items-center my-4"
            :class="[isOpen ? 'mx-4' : 'button-container']"
          >
            <b-col class="" :class="{ 'ml-auto col-4': isOpen }">
              <font-awesome-icon
                class="arrow-icon p-2"
                icon="angle-double-left"
                aria-hidden="true"
                size="3x"
                @click="toggleOpen"
              />
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </nav>

    <!-- Dark Overlay element -->
    <div v-show="isOverlayVisible" class="overlay" @click="toggleOpen"></div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component, Watch } from "vue-property-decorator";
import { Getter, Action } from "vuex-class";
import { IAuthenticationService } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import VueRouter, { Route } from "vue-router";
import EventBus from "@/eventbus";
import { WebClientConfiguration } from "@/models/configData";
import { library } from "@fortawesome/fontawesome-svg-core";
import { faStream } from "@fortawesome/free-solid-svg-icons";
library.add(faStream);

const auth: string = "auth";
const sidebar: string = "sidebar";

@Component
export default class SidebarComponent extends Vue {
  @Action("toggleSidebar", { namespace: sidebar }) toggleSidebar: any;
  @Getter("isOpen", { namespace: sidebar }) isOpen!: boolean;
  @Getter("oidcIsAuthenticated", {
    namespace: auth
  })
  oidcIsAuthenticated!: boolean;
  @Getter("webClient", { namespace: "config" }) config!: WebClientConfiguration;

  private authenticationService!: IAuthenticationService;
  private name: string = "";
  private windowWidth: number = 0;
  private $bodyElement!: HTMLBodyElement | null;

  @Watch("oidcIsAuthenticated")
  onPropertyChanged() {
    // If there is no name in the scope, retrieve it from the service.
    if (this.oidcIsAuthenticated && !this.name) {
      this.loadName();
    }
  }

  @Watch("$route")
  onRouteChanged() {
    this.clearOverlay();
  }

  @Watch("isOpen")
  onIsOpen(newValue: boolean, oldValue: boolean) {
    // Make sure that scroll is disabled when the overlay is active
    if (this.$bodyElement !== null) {
      if (this.isOverlayVisible) {
        this.$bodyElement.style.position = "fixed";
      } else this.$bodyElement.style.removeProperty("position");
    }
  }

  mounted() {
    this.authenticationService = container.get(
      SERVICE_IDENTIFIER.AuthenticationService
    );
    if (this.oidcIsAuthenticated) {
      this.loadName();
    }

    // Setup the transition listener to avoid text wrapping
    var transition = document.querySelector("#sidebar");
    transition?.addEventListener("transitionend", function(event: Event) {
      let transitionEvent = event as TransitionEvent;
      if (
        transition !== transitionEvent.target ||
        transitionEvent.propertyName !== "max-width"
      ) {
        return;
      }

      var buttonText = document
        .querySelectorAll(".button-title")
        .forEach(button => {
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
    this.$bodyElement = document.querySelector("body");
  }

  beforeDestroy() {
    window.removeEventListener("resize", this.onResize);
  }

  private toggleOpen() {
    this.toggleSidebar();
  }

  private loadName(): void {
    this.authenticationService.getOidcUserProfile().then(oidcUser => {
      if (oidcUser) {
        this.name = this.getFullname(oidcUser.given_name, oidcUser.family_name);
      }
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
    EventBus.$emit("timelineCreateNote");
    this.clearOverlay();
  }

  private printView() {
    EventBus.$emit("timelinePrintView");
    this.clearOverlay();
  }

  private goToTimeline() {
    this.$router.push({ path: "/timeline" });
  }

  private onResize() {
    this.windowWidth = window.innerWidth;
  }

  private get isOverlayVisible() {
    return this.isOpen && this.windowWidth < 768;
  }

  private get isTimeline(): boolean {
    return this.$route.path == "/timeline";
  }

  private get isNoteEnabled(): boolean {
    return this.config.modules["Note"];
  }
}
</script>
