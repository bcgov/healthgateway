<script setup lang="ts">
import { computed, ref } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import HgIconButtonComponent from "@/components/common/HgIconButtonComponent.vue";
import { useAppStore } from "@/stores/app";
import { ReliableTimer } from "@/utility/reliableTimer";

const emit = defineEmits<{
    (e: "response-received"): void;
    (e: "expired"): void;
}>();

defineExpose({ showDialog });

const appStore = useAppStore();

const intervalId = ref<ReturnType<typeof setInterval>>();
const countdownTimer = ref<ReliableTimer>();
const remainingTime = ref(Number.MAX_SAFE_INTEGER);

const isVisible = computed(() => appStore.isIdle);
const remainingSeconds = computed(() => Math.ceil(remainingTime.value / 1000));

function setVisible(value: boolean): void {
    appStore.setIsIdle(value);
}

function showDialog(countdownDuration: number): void {
    if (isVisible.value) {
        return;
    }

    intervalId.value = setInterval(update, 1000);
    countdownTimer.value = new ReliableTimer(expire, countdownDuration);
    countdownTimer.value.start();
    update();

    setVisible(true);
}

function update(): void {
    if (countdownTimer.value === undefined) {
        return;
    }

    remainingTime.value = countdownTimer.value.remainingTime;
}

function notifyNotIdle(): void {
    emit("response-received");
    hideDialog();
}

function expire(): void {
    emit("expired");
    hideDialog();
}

function hideDialog(): void {
    setVisible(false);
    clearInterval(intervalId.value);
    countdownTimer.value?.cancel();
}
</script>

<template>
    <v-dialog
        :model-value="isVisible"
        data-testid="idleModal"
        persistent
        no-click-animation
    >
        <div class="d-flex justify-center">
            <v-card max-width="700px">
                <v-card-title class="bg-primary px-0">
                    <v-toolbar
                        title="Are you still there?"
                        density="compact"
                        color="primary"
                    >
                        <HgIconButtonComponent
                            icon="close"
                            aria-label="Close"
                            @click="notifyNotIdle"
                        />
                    </v-toolbar>
                </v-card-title>
                <v-card-text class="text-body-1 pa-4">
                    <p data-testid="idleModalText">
                        You will be automatically logged out in
                        {{ remainingSeconds }} seconds.
                    </p>
                </v-card-text>
                <v-card-actions class="justify-end border-t-sm pa-4">
                    <HgButtonComponent
                        variant="primary"
                        text="I'm here!"
                        @click="notifyNotIdle"
                    />
                </v-card-actions>
            </v-card>
        </div>
    </v-dialog>
</template>
