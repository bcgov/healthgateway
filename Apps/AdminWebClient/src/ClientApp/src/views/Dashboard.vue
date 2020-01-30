<template>
  <v-parallax dark src="@/assets/background.jpg" height="700">
    <v-container app>
    </v-container>
  </v-parallax>
</template>

<script lang="ts">
import { Component, Vue } from 'vue-property-decorator';

@Component
export default class Dashboard extends Vue {
  public name = 'Dashboard';
}
</script>

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
