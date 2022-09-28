<script lang="ts">
import Vue from "vue";
import { Component, Watch } from "vue-property-decorator";
import { Action } from "vuex-class";

@Component
export default class IdleComponent extends Vue {
    @Action("setVisibleState", { namespace: "idle" })
    setVisibleState!: (isVisible: boolean) => void;

    private readonly maxCountdownTime = 60;
    private logoutCountdown = this.maxCountdownTime;
    private isVisible = false;
    private timerHandle = 0;
    private timeoutHandle = 0;

    @Watch("logoutCountdown")
    private onCountdownUpdate(): void {
        if (this.logoutCountdown <= 0) {
            this.$router.push("/logout");
            window.clearTimeout(this.timeoutHandle);
            window.clearInterval(this.timerHandle);
        }
    }

    public show(): void {
        if (!this.isVisible) {
            this.timerHandle = window.setInterval(() => this.countdown(), 1000);
            this.timeoutHandle = window.setTimeout(
                () => this.setVisibleState(false),
                1000 * this.maxCountdownTime
            );
        }

        this.isVisible = true;
        this.setVisibleState(true);
    }

    private reset(): void {
        this.logoutCountdown = this.maxCountdownTime;
        window.clearTimeout(this.timeoutHandle);
        window.clearInterval(this.timerHandle);
        this.setVisibleState(false);
    }

    private countdown(): void {
        this.logoutCountdown--;
    }
}
</script>

<template>
    <b-modal
        id="idle-modal"
        v-model="isVisible"
        data-testid="idleModal"
        header-bg-variant="primary"
        header-text-variant="light"
        :ok-only="true"
        title="Are you still there?"
        ok-title="I'm here!"
        centered
        @ok="reset"
        @hidden="reset"
    >
        <b-row>
            <b-col data-testid="idleModalText">
                You will be automatically logged out in
                {{ logoutCountdown }} seconds.
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
