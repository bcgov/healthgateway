<template>
  <v-layout class="fill-height">
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <v-row justify="center">You have been logged out</v-row>
  </v-layout>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";
import LoadingComponent from "@/components/core/Loading.vue";

const namespace: string = "auth";

@Component({
  components: {
    LoadingComponent
  }
})
export default class LoginView extends Vue {
  public name = "Dashboard";
  @Action("logout", { namespace }) logout: any;
  @Getter("isAuthenticated", { namespace }) isAuthenticated: boolean;
  private isLoading: boolean = true;

  mounted() {
    this.isLoading = true;
    if (this.isAuthenticated) {
      this.logout().then(result => {
        this.isLoading = false;
      });
    } else {
      this.isLoading = false;
    }
  }
}
</script>
