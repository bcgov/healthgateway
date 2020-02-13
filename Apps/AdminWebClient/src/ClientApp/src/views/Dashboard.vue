<style scoped>
.v-btn {
  background-size: 100%;
  width: 386px;
  height: 96px;
  display: block;
  margin: 10px auto 50px;
  border: none;
}
</style>

<template>
  <v-layout class="fill-height">
    <v-row justify="center">
      Dashboard goes here
    </v-row>
  </v-layout>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

@Component
export default class Dashboard extends Vue {
  public name = "Dashboard";
}
</script>
