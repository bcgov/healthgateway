<template>
  <div id="app-root" class="fill-body">
    <header>
      <environment exclude="Production">
        <div class="text-center bg-warning small">
          Non-production environment:
          <b>{{host}}</b>
        </div>
      </environment>
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

@Component({
  components: {
    NavHeader: HeaderComponent,
    NavFooter: FooterComponent
  }
})
export default class AppComponent extends Vue {
  host: string = window.location.hostname.toLocaleUpperCase();

  constructor() {
    super();
    console.log(this.host);
  }
}
</script>

