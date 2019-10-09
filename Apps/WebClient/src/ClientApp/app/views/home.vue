<template>
  <div class="container">
    <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    <div class="row pt-5">
      <div class="col-lg-12 col-md-12 text-center">
        <h1>Welcome to Health Gateway!</h1>
        <h2 v-if="phn.length > 0">PHN: {{ phn }}</h2>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import LoadingComponent from "@/components/loading.vue";
import { State, Action, Getter } from "vuex-class";
import User from "@/models/user";

const namespace: string = "user";

@Component({
  components: {
    LoadingComponent
  }
})
export default class HomeComponent extends Vue {
  @Action("getPatientData", { namespace }) getPatientData;
  @Getter("user", { namespace }) user: User;

  private isLoading: boolean = false;
  private phn: string = "";

  mounted() {
    console.log(this.user.hdid);
    this.isLoading = true;
    this.getPatientData({ hdid: this.user.hdid })
      .then(() => {
        if (this.user.phn.length === 0) {
          this.phn = "Not Found";
        } else {
          this.phn = this.user.phn;
        }

        console.log(this.phn);
      })
      .catch(error => {
        console.log("Could not retrieve patient data");
        this.phn = "Error contacting server";
      })
      .finally(() => {
        this.isLoading = false;
        console.log(this.user);
      });
  }
}
</script>
