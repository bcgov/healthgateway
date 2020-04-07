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
}

#sidebar.collapsed {
  min-width: 80px;
  max-width: 80px;
}

#sidebar.collapsed .button-container {
  border-color: $primary !important;
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
  height: 100px;
  display: flex;
  align-items: center;
}

.no-wrap {
  white-space: nowrap;
  overflow: hidden;
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
</style>

<template>
  <div class="wrapper">
    <!-- Sidebar -->
    <nav id="sidebar" :class="{ collapsed: isCollapsed }">
      <b-row class="row-container no-gutters">
        <b-col class="no-gutters" :class="{ 'px-4': !isCollapsed }">

          <!-- Profile Button -->
          <router-link id="menuBtnProfile" to="/profile">
            <b-row class="align-items-center no-gutters name-wrapper my-2">
              <b-col class="no-gutters" :class="{ 'col-4': !isCollapsed }">
                <font-awesome-icon
                  icon="user-circle"
                  class="button-icon"
                  size="3x"
                />
              </b-col>
              <b-col v-if="!isCollapsed" cols="7" class="button-title no-wrap ">
                {{ name }}
              </b-col>
            </b-row>
          </router-link>

          <hr class="mb-4 mt-0 p-0" />

          <!-- Note button -->
          <b-row
            class="align-items-center no-gutters border rounded-pill my-4 button-container"
          >
            <b-col class="no-gutters" :class="{ 'col-4': !isCollapsed }">
              <font-awesome-icon icon="edit" class="button-icon" size="2x" />
            </b-col>
            <b-col v-if="!isCollapsed" cols="7" class="button-title no-wrap">
              <span>Add a Note</span>
            </b-col>
          </b-row>

          <!-- Print Button -->
          <b-row
            class="align-items-center no-gutters border rounded-pill my-4 p-2 button-container"
          >
            <b-col class="no-gutters" :class="{ 'col-4': !isCollapsed }">
              <font-awesome-icon
                icon="print"
                class="button-icon m-auto"
                size="2x"
              />
            </b-col>
            <b-col v-if="!isCollapsed" cols="7" class="button-title">
              <span>Print</span>
            </b-col>
          </b-row>

          <br />

          <!-- Collapse Button -->
          <b-row
            class="align-items-center no-gutters button-container px-3 mt-4"
          >
            <b-col
              class="no-gutters"
              :class="{ 'ml-auto col-auto': !isCollapsed }"
            >
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
  private isCollapsed: boolean = false;
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

      var buttonText = document.querySelector(".button-title ");
      if (transition?.classList.contains("collapsed")) {
        buttonText?.classList.add("no-wrap");
      } else {
        buttonText?.classList.remove("no-wrap");
      }
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
