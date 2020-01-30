<template>
  <v-content>
    <div id="core-view">
      <v-fade-transition mode="out-in">
        <router-view />
      </v-fade-transition>
    </div>
    <core-footer v-if="$route.name !== 'Maps'" />
  </v-content>
</template>

<script>
export default {
  metaInfo() {
    return {
      title: 'Vuetify Material Dashboard',
    },
  },
};
</script>

<style>
#core-view {
  padding-bottom: 100px;
}
</style>
