<style>
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
</style>

<template>
  <div id="app-root" class="container-fluid-fill d-flex h-100 flex-column">
    <div v-if="!isProduction" exclude="Production">
      <div class="text-center bg-warning small">
        Non-production environment:
        <b>{{ host }}</b>
      </div>
    </div>
    <header>
      <NavHeader />
    </header>
    <main class="col">
      <router-view></router-view>
    </main>
    <footer class="footer">
      <NavFooter />
    </footer>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Watch, Ref } from "vue-property-decorator";
import { Getter } from "vuex-class";
import HeaderComponent from "@/components/navmenu/navHeader.vue";
import IdleComponent from "@/components/modal/idle.vue";
import FooterComponent from "@/components/navmenu/navFooter.vue";
import Process, { EnvironmentType } from "@/constants/process.ts";

@Component({
  components: {
    NavHeader: HeaderComponent,
    NavFooter: FooterComponent,
    IdleComponent
  }
})
export default class AppComponent extends Vue {
  @Ref("idleModal") readonly idleModal: IdleComponent;
  @Getter("oidcIsAuthenticated", { namespace: "auth" })
  oidcIsAuthenticated: boolean;

  private readonly host: string = window.location.hostname.toLocaleUpperCase();
  private readonly isProduction: boolean =
    Process.NODE_ENV == EnvironmentType.production;

  constructor() {
    super();
  }

  @Watch("isAppIdle")
  public onIsAppIdleChanged(idle: boolean) {
    console.log(`isAppIdle ${idle}`);
    if (idle && this.oidcIsAuthenticated) {
      this.idleModal.show();
    }
  }
}
</script>
