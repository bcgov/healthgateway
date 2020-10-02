<script lang="ts">
import Vue from "vue";
import { Component, Emit } from "vue-property-decorator";
import { Action } from "vuex-class";
import EventBus, { EventMessageName } from "@/eventbus";

@Component
export default class IdleComponent extends Vue {
    @Action("authenticateOidcSilent", { namespace: "auth" })
    authenticateOidcSilent!: () => Promise<void>;

    private readonly modalId: string = "idle-modal";
    private totalTime = 60;
    private visible = false;
    private timerHandle?: number;
    private timeoutHandle?: number;
    private eventBus = EventBus;

    @Emit()
    public show(): void {
        if (!this.visible) {
            this.$bvModal.show(this.modalId);
            this.timerHandle = window.setInterval(() => this.countdown(), 1000);
            this.timeoutHandle = window.setTimeout(() => {
                this.eventBus.$emit(EventMessageName.IdleLogoutWarning, true);
                this.logout();
            }, 1000 * 60);
        }
    }

    @Emit()
    public logout(): void {
        this.$router.push("/logout");
    }

    private refresh() {
        this.authenticateOidcSilent();
        this.reset();
    }

    private reset() {
        this.totalTime = 60;
        window.clearTimeout(this.timeoutHandle);
        window.clearInterval(this.timerHandle);
        this.eventBus.$emit(EventMessageName.IdleLogoutWarning, false);
    }

    private countdown() {
        this.totalTime--;
    }
}
</script>

<template>
    <b-modal
        id="idle-modal"
        v-model="visible"
        data-testid="idleModal"
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
            <b-col data-testid="idleModalText">
                You will be automatically logged out in {{ totalTime }} seconds.
            </b-col>
        </b-row>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.modal-footer {
    justify-content: flex-start;
}
</style>
