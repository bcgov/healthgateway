<template>
  <div id="app">
    Welcome to the HG Sampler!
    <Samples />
  </div>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";

import Samples from "./components/Samples.vue";

@Component({
  components: {
    Samples,
  },
})
export default class App extends Vue {}
</script>

<style lang="scss">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
