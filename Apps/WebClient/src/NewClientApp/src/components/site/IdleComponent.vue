<script setup lang="ts">
import { computed, ref, watch } from "vue";

import IdleDialogComponent from "@/components/site/IdleDialogComponent.vue";
import { Path } from "@/constants/path";
import router from "@/router";
import { useAuthStore } from "@/stores/auth";
import { useConfigStore } from "@/stores/config";
import { IdleDetector } from "@/utility/idleDetector";

const maxIdleDialogCountdown = 60000;

const authStore = useAuthStore();
const configStore = useConfigStore();

const idleDialog = ref<InstanceType<typeof IdleDialogComponent>>();

const timeBeforeIdle = computed(() => configStore.webConfig.timeouts.idle);
const idleDetector = computed(() =>
    timeBeforeIdle.value > 0
        ? new IdleDetector(
              handleIsIdle,
              timeBeforeIdle.value,
              authStore.oidcIsAuthenticated
          )
        : undefined
);

function handleIsIdle(timeIdle: number): void {
    if (!authStore.oidcIsAuthenticated) {
        return;
    }

    const countdownTime = maxIdleDialogCountdown - timeIdle;
    if (countdownTime <= 0) {
        router.push(Path.Logout);
    } else {
        idleDialog.value?.showDialog(countdownTime);
    }
}

watch(
    () => authStore.oidcIsAuthenticated,
    (value: boolean) => {
        // enable idle detector when authenticated and disable when not
        if (value) {
            idleDetector.value?.enable();
        } else {
            idleDetector.value?.disable();
        }
    }
);

// Start timer if the user refreshes the page while authenticated
if (authStore.oidcIsAuthenticated) {
    idleDetector.value?.enable();
}
</script>

<template>
    <IdleDialogComponent
        ref="idleDialog"
        @response-received="idleDetector?.enable()"
        @expired="router.push(Path.Logout)"
    />
</template>
