<script setup lang="ts">
import "vue-loading-overlay/dist/vue-loading.css";

import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faEdit,
    faPills,
    faSyringe,
    faVial,
} from "@fortawesome/free-solid-svg-icons";
import { computed, onMounted, onUnmounted, ref } from "vue";
import LoadingOverlay from "vue-loading-overlay";

library.add(faEdit, faVial, faPills, faSyringe);

interface Props {
    isLoading: boolean;
    isCustom?: boolean;
    fullScreen?: boolean;
    text?: string;
}
const props = withDefaults(defineProps<Props>(), {
    isCustom: false,
    fullScreen: true,
    text: undefined,
});

const step = ref(0);
const intervalId = ref(0);
const ellipsis = computed(() => ".".padEnd(step.value + 1, "."));

onMounted(() => {
    if (props.isCustom) {
        resetTimeout();
    }
});

onUnmounted(() => {
    window.clearInterval(intervalId.value);
});

function resetTimeout(): void {
    intervalId.value = window.setInterval(() => {
        step.value++;
        if (step.value >= 4) {
            step.value = 0;
            resetAnimation("first");
            resetAnimation("second");
            resetAnimation("third");
            resetAnimation("fourth");
        }
    }, 2000);
}

function resetAnimation(elementId: string): boolean {
    const el = document.getElementById(elementId);
    if (el == null) {
        return false;
    }
    el.style.animation = "none";
    el.offsetHeight; /* trigger reflow */
    el.style.animation = "";
    return true;
}
</script>

<template>
    <div
        v-show="isLoading"
        class="vld-parent"
        :class="isCustom && fullScreen ? 'fullScreen' : 'block'"
    >
        <LoadingOverlay
            v-if="!isCustom"
            :active="isLoading"
            data-testid="loadingSpinner"
            :is-full-page="true"
        >
            <template v-if="text" #after>
                <div class="m-3">{{ text }}</div>
            </template>
        </LoadingOverlay>
        <div v-else>
            <div class="spinner" data-testid="timelineLoading">
                <div id="first" class="double-bounce">
                    <hg-icon icon="pills" size="large" />
                </div>
                <div id="second" class="double-bounce">
                    <hg-icon icon="vial" size="large" />
                </div>
                <div id="third" class="double-bounce">
                    <hg-icon icon="syringe" size="large" />
                </div>
                <div id="fourth" class="double-bounce">
                    <hg-icon icon="edit" size="large" />
                </div>
            </div>
            <div class="text">
                <h5 class="d-none d-sm-inline">
                    Gathering your health records
                    {{ ellipsis }}
                </h5>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.fullScreen {
    position: fixed;
    z-index: $z_loading_overlay;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(254, 254, 254, 0.5);
    display: table;
    transition: opacity 0.3s ease;
}

.block {
    position: absolute;
    z-index: $z_loading_overlay;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(254, 254, 254, 0.5);
    display: table;
    transition: opacity 0.3s ease;
}

.spinner {
    width: 60px;
    height: 60px;
    position: absolute;
    margin: 60px auto;

    top: 50%;
    left: 50%;
    margin-top: -30px;
    margin-left: -30px;
}

.text {
    color: $primary;
    width: 280px;
    height: 100px;
    position: absolute;
    margin: 100px auto;
    top: 50%;
    left: 50%;
    margin-top: 55px;
    margin-left: -130px;
    text-shadow: 4px 4px 4px $primary_text;
}

.double-bounce {
    opacity: 0;
    background-color: $primary;
    color: $primary_text;
    display: flex;
    justify-content: center;
    align-items: center;
    width: 100%;
    height: 100%;
    border-radius: 50%;
    position: absolute;
    top: 0;
    left: 0;
    animation-name: sk-bounce;
    animation-duration: 2s;
    animation-timing-function: ease-in-out;
    animation-direction: alternate;
    animation-iteration-count: 1;
    animation-play-state: running;
    animation-fill-mode: none;
}

#first {
    animation-delay: 0s;
}

#second {
    animation-delay: 2s;
}

#third {
    animation-delay: 4s;
}

#fourth {
    animation-delay: 6s;
}

@keyframes sk-bounce {
    0% {
        opacity: 1;
        transform: scale(0.01);
    }
    50% {
        opacity: 1;
        transform: scale(1);
    }
    100% {
        opacity: 1;
        transform: scale(0.01);
    }
}
</style>
