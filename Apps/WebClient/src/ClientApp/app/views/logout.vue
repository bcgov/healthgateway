<template>
  <div class="row justify-content-center h-100 pt-5">
    <div class="col-lg-6 col-md-6 pt-2">
      <div
        v-if="!isAuthenticated"
        class="shadow-lg p-3 mb-5 bg-white rounded border"
      >
        <h3>
          <strong>You signed out of your account</strong>
        </h3>
        <p>It's a good idea to close all browser windows.</p>
      </div>
      <div v-else>
        <h3>Signing out...</h3>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

const namespace = "auth";

@Component
export default class LoginComponent extends Vue {
  @Action("logout", { namespace }) logout: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;

  mounted() {
    if (this.isAuthenticated) {
      this.logout();
    }
  }
}
</script>
