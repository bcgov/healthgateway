<script setup lang="ts">
import { computed } from "vue";
import { useRouter } from "vue-composition-wrapper";
import { useStore } from "vue-composition-wrapper";

const router = useRouter();
const store = useStore();

const tooBusy = computed<boolean>(() => store.getters["waitlist/tooBusy"]);

if (!tooBusy.value) {
    router.push({ path: "/" });
}
</script>

<template>
    <div
        class="flex-grow-1 d-flex flex-column align-items-center justify-content-center p-3 p-md-4"
    >
        <b-alert show variant="danger" data-testid="too-many-tickets">
            Can’t access Health Gateway. Please try again later.
        </b-alert>
        <img
            src="@/assets/images/queue/error.png"
            class="img-fluid"
            data-testid="queue-error-image"
            alt=""
        />
    </div>
</template>
