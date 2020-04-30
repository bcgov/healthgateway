<style lang="scss">
@import "@/assets/scss/_variables.scss";

@media print {
  .navbar {
    display: flex !important;
  }

  .no-print,
  .no-print * {
    display: none !important;
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
  z-index: $z_top_layer;
}
</style>

<template>
  <div id="app-root" class="container-fluid-fill d-flex h-100 flex-column">
    <div v-if="!isProduction" class="devBanner">
      <div class="text-center bg-warning small">
        Non-production environment:
        <b>{{ host }}</b>
      </div>
    </div>

    <header>
      <NavHeader />
    </header>
    <b-row class="p-0 m-0">
      <NavSidebar class="no-print" />

      <main class="col fill-height">
        <router-view></router-view>
        <IdleComponent ref="idleModal" />
      </main>
    </b-row>

    <footer class="footer no-print">
      <NavFooter />
    </footer>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Watch, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";
import Process, { EnvironmentType } from "@/constants/process.ts";

// Load Bootstrap general plugins
import {
  LayoutPlugin,
  NavPlugin,
  NavbarPlugin,
  ModalPlugin,
  ButtonPlugin,
  CardPlugin,
  LinkPlugin,
  FormPlugin,
  FormInputPlugin,
  FormCheckboxPlugin,
  FormTextareaPlugin,
  AlertPlugin,
  SpinnerPlugin,
  InputGroupPlugin,
  PaginationNavPlugin
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

// Load general icons
import { config, library } from "@fortawesome/fontawesome-svg-core";
// Prevent auto adding CSS to the header since that breaks Content security policies.
config.autoAddCss = false;
// Add font awesome styles manually
import "@fortawesome/fontawesome-svg-core/styles.css";

import {
  faUser,
  faUserCircle,
  faAddressCard,
  faUserSecret,
  faEdit,
  faChevronUp,
  faChevronDown,
  faSpinner,
  faCheckCircle,
  faTimesCircle,
  faEllipsisV,
  faPrint,
  faAngleDoubleLeft,
  faFileAlt,
  faChartBar,
  faCommentAlt
} from "@fortawesome/free-solid-svg-icons";
library.add(
  faUser,
  faUserCircle,
  faAddressCard,
  faUserSecret,
  faEdit,
  faChevronUp,
  faChevronDown,
  faSpinner,
  faCheckCircle,
  faTimesCircle,
  faEllipsisV,
  faPrint,
  faAngleDoubleLeft,
  faFileAlt,
  faChartBar,
  faCommentAlt
);

import HeaderComponent from "@/components/navmenu/navHeader.vue";
import IdleComponent from "@/components/modal/idle.vue";
import FooterComponent from "@/components/navmenu/navFooter.vue";
import SidebarComponent from "@/components/navmenu/sidebar.vue";

@Component({
  components: {
    NavHeader: HeaderComponent,
    NavFooter: FooterComponent,
    NavSidebar: SidebarComponent,
    IdleComponent
  }
})
export default class AppComponent extends Vue {
  @Ref("idleModal") readonly idleModal?: IdleComponent;
  @Getter("oidcIsAuthenticated", { namespace: "auth" })
  oidcIsAuthenticated?: boolean;

  private readonly host: string = window.location.hostname.toLocaleUpperCase();
  private readonly isProduction: boolean =
    Process.NODE_ENV == EnvironmentType.production &&
    (this.host.startsWith("HEALTHGATEWAY") ||
      this.host.startsWith("WWW.HEALTHGATEWAY"));

  constructor() {
    super();
    console.log("Node ENV", Process.NODE_ENV);
    console.log("host", this.host);
  }

  @Watch("isAppIdle")
  public onIsAppIdleChanged(idle: boolean) {
    if (idle && this.oidcIsAuthenticated) {
      this.idleModal.show();
    }
  }
}
</script>
