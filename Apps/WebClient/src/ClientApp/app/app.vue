<style>
.container {
  min-height: 100vh;
}
</style>

<template>
  <div id="app-root" class="fill-body">
    <header>
      <NavHeader />
    </header>
    <main>
      <div class="container">
        <router-view></router-view>
      </div>
    </main>
    <NavFooter />
    <IdleComponent ref="idleModal" />
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
