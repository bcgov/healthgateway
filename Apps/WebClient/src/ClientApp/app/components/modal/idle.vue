<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-footer {
  justify-content: flex-start;
}
</style>
<template>
  <b-modal
    ref="idle-modal"
    v-model="visible"
    header-bg-variant="primary"
    header-text-variant="light"
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
import EventBus from "@/eventbus";
import User from "@/models/user";

@Component
export default class IdleComponent extends Vue {
  @Ref("idle-modal") readonly modal!: HTMLElement;
  @Action("authenticateOidcSilent", { namespace: "auth" })
  authenticateOidcSilent!: () => Promise<User | null>;

  private totalTime: number = 60;
  private visible: boolean = false;
  private timerHandle?: number;
  private timeoutHandle?: number;

  @Emit()
  public show() {
    if (!this.visible) {
      this.modal.show();
      var self = this;
      this.timerHandle = setInterval(() => self.countdown(), 1000);
      this.timeoutHandle = setTimeout(() => {
        EventBus.$emit("idleLogoutWarning", true);
        self.logout();
      }, 1000 * 60);
    }
  }
  @Emit()
  public logout() {
    this.$router.push("/logout");
  }

  private refresh(bvModalEvt: Event) {
    this.authenticateOidcSilent();
    this.reset();
  }

  private reset() {
    this.totalTime = 60;
    clearTimeout(this.timeoutHandle);
    clearInterval(this.timerHandle);
    EventBus.$emit("idleLogoutWarning", false);
  }

  private countdown() {
    this.totalTime--;
  }
}
</script>
