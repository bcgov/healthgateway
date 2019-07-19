<template>
  <div id="app-root" class="fill-body">
    <header>
      <environment exclude="Production">
        <div class="text-center bg-warning small">
          Non-production environment:
          <b>{{host}}</b>
        </div>
      </environment>
      <header-component />
    </header>
    <main>
      <div class="container">
        <router-view></router-view>
      </div>
    </main>
    <footer-component />
  </div>
</template>


<script lang='ts'>
import Vue from "vue";
import { Component } from "vue-property-decorator";

import HeaderMenu from "./components/navmenu/header.vue";
import FooterMenu from "./components/navmenu/footer.vue";

@Component({
  components: {
    HeaderComponent: HeaderMenu,
    FooterComponent: FooterMenu
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

