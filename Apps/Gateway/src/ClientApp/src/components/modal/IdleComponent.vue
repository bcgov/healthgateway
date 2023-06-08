<script setup lang="ts">
import { BvModalEvent } from "bootstrap-vue";
import { computed, ref } from "vue";
import { useRouter, useStore } from "vue-composition-wrapper";

import { ReliableTimer } from "@/utility/reliableTimer";

const router = useRouter();
const store = useStore();

defineExpose({ show });

const isVisible = computed<boolean>(() => store.getters["idle/isVisible"]);

function setVisibleState(isVisible: boolean): void {
    store.dispatch("idle/setVisibleState", isVisible);
}

const intervalId = ref<ReturnType<typeof setInterval>>();
const countdownTimer = ref<ReliableTimer>();
const notifyStillHere = ref<() => void>();
const remainingTime = ref(Number.MAX_SAFE_INTEGER);

const remainingSeconds = computed(() => Math.ceil(remainingTime.value / 1000));

function show(countdownTime: number, notifyStillHereFunc: () => void): void {
    if (isVisible.value) {
        return;
    }

    notifyStillHere.value = notifyStillHereFunc;

    intervalId.value = setInterval(() => update(), 1000);
    countdownTimer.value = new ReliableTimer(() => logout(), countdownTime);
    countdownTimer.value.start();
    update();

    setVisibleState(true);
}

function update(): void {
    if (countdownTimer.value === undefined) {
        return;
    }
    remainingTime.value = countdownTimer.value.remainingTime;
}

function handleHide(event: BvModalEvent): void {
    if (event.trigger !== null) {
        // hide was caused by user interaction
        if (notifyStillHere.value !== undefined) {
            notifyStillHere.value();
        }
        setVisibleState(false);
    }

    clearInterval(intervalId.value);
    countdownTimer.value?.cancel();
}

function logout(): void {
    router.push("/logout");
    setVisibleState(false);
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
