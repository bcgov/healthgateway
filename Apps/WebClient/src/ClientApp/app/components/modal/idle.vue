<style lang="scss">
@import "../../assets/scss/_variables.scss";
.text-large {
  font-size: 250%;
}
.modal-header {
  background-color: $primary;
  color: $primary_text;

  button,
  button:hover {
    color: #fff;
  }
}
.modal-footer {
  justify-content: flex-start;
}
</style>
<template>
  <b-modal
    ref="idle-modal"
    v-model="visible"
    header-class="modal-header"
    footer-class="modal-footer"
    :ok-only="true"
    title="Are you still there?"
    ok-title="I'm here!"
    centered
    @ok="refresh"
    @hidden="refresh"
  >
    <b-row>
      <b-col>
        You will be automatically logged out in {{ totalTime }} seconds.
      </b-col>
    </b-row>
  </b-modal>
</template>

<script lang="ts">
import Vue from "vue";
import { Ref, Emit, Component } from "vue-property-decorator";
import { Action } from "vuex-class";

@Component
export default class IdleComponent extends Vue {
  @Ref("idle-modal") readonly modal: HTMLElement;
  @Action("authenticateOidcSilent", { namespace: "auth" })
  authenticateOidcSilent;

  private totalTime: number = 60;
  private visible: boolean = false;

  @Emit()
  public show() {
    if (!this.visible) {
      this.modal.show();
      var self = this;
      this.timer = setInterval(() => self.countdown(), 1000);
      this.timeout = setTimeout(() => self.logout(), 1000 * 60);
    }
  }
  @Emit()
  public logout() {
    this.$router.push("/logout");
  }

  private refresh(bvModalEvt) {
    this.authenticateOidcSilent();
    this.reset();
  }

  private reset() {
    this.totalTime = 60;
    clearTimeout(this.timeout);
    clearInterval(this.timer);
  }

  private countdown() {
    this.totalTime--;
  }
}
</script>
