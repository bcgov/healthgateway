<style>
.container {
  min-height: 100vh;
}
</style>

<template>
  <div id="app-root" class="fill-body">
    <header>
      <span v-if="isDevelopment">
        <div class="text-center bg-warning small">
          Non-production environment:
          <b>{{host}}</b>
        </div>
      </span>
      <NavHeader />
    </header>
    <main>
      <div class="container">
        <router-view></router-view>
      </div>
    </main>
    <NavFooter />
  </div>
</template>

<script lang='ts'>
import Vue from "vue";
import { Component } from "vue-property-decorator";

import HeaderComponent from "./components/navmenu/navHeader.vue";
import FooterComponent from "./components/navmenu/navFooter.vue";
import Process, { EnvironmentType } from "@/constants/process";

@Component({
  components: {
    NavHeader: HeaderComponent,
    NavFooter: FooterComponent
  }
})
export default class AppComponent extends Vue {
  private readonly host: string = window.location.hostname.toLocaleUpperCase();
  private readonly isDevelopment: boolean = Process.NODE_ENV == EnvironmentType.development;
}
</script>