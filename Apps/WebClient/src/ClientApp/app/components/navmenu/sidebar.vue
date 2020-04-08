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
  z-index: 9990;
  position: static;
}

#sidebar.collapsed {
  min-width: 80px;
  max-width: 80px;
}

#sidebar.collapsed .button-container {
  border-color: $primary !important;
  margin: 0px;
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
  z-index: 998;
  /* animate the transition */
  transition: all 0.5s ease-in-out;
  top: 0px;
}

/* Small Devices*/
@media (max-width: 767px) {
  #sidebar {
    display: absolute;
    position: fixed;
    top: 0px;
    padding-top: 70px;
  }

  #sidebar.collapsed {
    min-width: 0px;
    max-width: 0px;
    height: 100vh;
  }

  #sidebar.collapsed .row-container {
    display: none;
  }
}
</style>

<template>
  <div v-if="oidcIsAuthenticated" class="wrapper">
    <!-- Sidebar -->
    <nav id="sidebar" :class="{ collapsed: isCollapsed }">
      <b-row class="row-container m-0 p-0">
        <b-col class="m-0 p-0">
          <!-- Profile Button -->
          <router-link id="menuBtnProfile" to="/profile">
            <b-row
              class="align-items-center name-wrapper my-4"
              :class="{ 'm-4': !isCollapsed }"
            >
              <b-col class="" :class="{ 'col-4': !isCollapsed }">
                <font-awesome-icon
                  icon="user-circle"
                  class="button-icon"
                  size="3x"
                />
              </b-col>
              <b-col v-if="!isCollapsed" cols="8" class="button-title d-none">
                {{ name }}
              </b-col>
            </b-row>
          </router-link>

          <hr class="mb-3 mt-0 p-2" />

          <!-- Note button -->
          <b-row
            class="align-items-center border rounded-pill p-1 button-container  my-4"
            :class="{ 'm-4': !isCollapsed }"
          >
            <b-col :class="{ 'col-4': !isCollapsed }">
              <font-awesome-icon icon="edit" class="button-icon" size="2x" />
            </b-col>
            <b-col v-if="!isCollapsed" cols="8" class="button-title d-none">
              <span>Add a Note</span>
            </b-col>
          </b-row>

          <!-- Print Button -->
          <b-row
            class="align-items-center border rounded-pill p-1 button-container  my-4"
            :class="{ 'm-4': !isCollapsed }"
          >
            <b-col class="" :class="{ 'col-4': !isCollapsed }">
              <font-awesome-icon
                icon="print"
                class="button-icon m-auto"
                size="2x"
              />
            </b-col>
            <b-col v-if="!isCollapsed" cols="8" class="button-title d-none">
              <span>Print</span>
            </b-col>
          </b-row>

          <br />

          <!-- Collapse Button -->
          <b-row
            class="align-items-center button-container my-4"
            :class="{ 'm-4': !isCollapsed }"
          >
            <b-col :class="{ 'ml-auto col-auto': !isCollapsed }">
              <font-awesome-icon
                class="arrow-icon"
                icon="angle-double-left"
                aria-hidden="true"
                size="2x"
                @click="toggleSideBar"
              />
            </b-col>
          </b-row>
        </b-col>
      </b-row>
    </nav>

    <!-- Dark Overlay element -->
    <div v-if="!isCollapsed" class="overlay d-block d-md-none"></div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component, Watch } from "vue-property-decorator";
import { Getter } from "vuex-class";
import { IAuthenticationService } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";

const auth: string = "auth";

@Component
export default class SidebarComponent extends Vue {
  @Getter("oidcIsAuthenticated", {
    namespace: auth
  })
  oidcIsAuthenticated!: boolean;

  private authenticationService!: IAuthenticationService;
  private isCollapsed: boolean = true;
  private name: string = "";

  private transition!: Element | null;

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
  }

  private toggleSideBar() {
    this.isCollapsed = !this.isCollapsed;
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
}
</script>
