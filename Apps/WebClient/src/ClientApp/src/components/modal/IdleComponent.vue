<script lang="ts">
import { BvModalEvent } from "bootstrap-vue";
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { ReliableTimer } from "@/utility/reliableTimer";

@Component
export default class IdleComponent extends Vue {
    @Action("setVisibleState", { namespace: "idle" })
    private setVisibleState!: (isVisible: boolean) => void;

    @Getter("isVisible", { namespace: "idle" })
    private isVisible!: boolean;

    private intervalId?: ReturnType<typeof setInterval>;
    private countdownTimer?: ReliableTimer;
    private notifyStillHere?: () => void;

    private remainingTime = Number.MAX_SAFE_INTEGER;

    get remainingSeconds(): number {
        return Math.ceil(this.remainingTime / 1000);
    }

    show(countdownTime: number, notifyStillHere: () => void): void {
        if (this.isVisible) {
            return;
        }

        this.notifyStillHere = notifyStillHere;

        this.intervalId = setInterval(() => this.update(), 1000);
        this.countdownTimer = new ReliableTimer(
            () => this.logout(),
            countdownTime
        );
        this.countdownTimer.start();
        this.update();

        this.setVisibleState(true);
    }

    private update(): void {
        if (this.countdownTimer === undefined) {
            return;
        }
        this.remainingTime = this.countdownTimer.remainingTime;
    }

    private handleHide(event: BvModalEvent): void {
        if (event.trigger !== null) {
            // hide was caused by user interaction
            if (this.notifyStillHere !== undefined) {
                this.notifyStillHere();
            }
            this.setVisibleState(false);
        }

        clearInterval(this.intervalId);
        this.countdownTimer?.cancel();
    }

    private logout(): void {
        this.$router.push("/logout");
        this.setVisibleState(false);
    }
}
</script>

<template>
    <b-modal
        id="idle-modal"
        :visible="isVisible"
        data-testid="idleModal"
        header-bg-variant="primary"
        header-text-variant="light"
        :ok-only="true"
        title="Are you still there?"
        ok-title="I'm here!"
        centered
        @hide="handleHide"
    >
        <b-row>
            <b-col data-testid="idleModalText">
                You will be automatically logged out in
                {{ remainingSeconds }} seconds.
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
