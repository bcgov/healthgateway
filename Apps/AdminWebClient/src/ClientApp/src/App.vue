<template>
  <v-app id="mysegement">
    <MainNavbar title="HealthGateway Admin" />
    <v-content>
      <router-view />
    </v-content>
    <Footer />
  </v-app>
</template>

<script lang="ts">
import MainNavbar from "@/components/MainNavbar.vue";
import Footer from "@/components/Footer.vue";

import { Component, Vue } from "vue-property-decorator";

@Component({
  components: { MainNavbar, Footer }
})
export default class App extends Vue {}
</script>
