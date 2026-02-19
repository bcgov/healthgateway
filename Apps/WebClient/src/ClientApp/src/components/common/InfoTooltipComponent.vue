<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from "vue";

interface Props {
    text?: string;
    size?: string;
    maxWidth?: number;
    tooltipTestid?: string;
    contentClass?: string;
}
const props = withDefaults(defineProps<Props>(), {
    text: undefined,
    size: "small",
    maxWidth: 300,
    tooltipTestid: undefined,
    contentClass: undefined,
});

const openedFromClick = ref(false);
const openedFromHover = ref(false);

const hasText = computed(() => !!props.text);

function handleClick() {
    openedFromClick.value = !openedFromClick.value;
    openedFromHover.value = false;
    if (openedFromClick.value) {
        setTimeout(() => {
            document.addEventListener("click", closeOnOutsideClick, {
                once: true,
            });
        });
    }
}

function closeOnOutsideClick() {
    if (openedFromClick.value) {
        openedFromClick.value = false;
    }
}

onBeforeUnmount(() => {
    // Remove click event if the event hasn't been triggered once.
    document.removeEventListener("click", closeOnOutsideClick);
});
</script>

<template>
    <v-tooltip
        :max-width="maxWidth"
        :model-value="openedFromClick || openedFromHover"
        :open-on-click="false"
        :open-on-hover="false"
        :content-class="contentClass"
    >
        <template #activator="{ props: slotProps }">
            <v-icon
                v-bind="{ ...slotProps, ...$attrs }"
                icon="far fa-circle-question"
                color="primary"
                :size="size"
                :data-testid="
                    tooltipTestid ? `${tooltipTestid}-icon` : undefined
                "
                @click="handleClick"
                @mouseenter="openedFromHover = true"
                @mouseleave="openedFromHover = false"
            />
        </template>
        <div :data-testid="tooltipTestid">
            <span v-if="hasText">{{ text }}</span>
            <slot v-else name="default" />
        </div>
    </v-tooltip>
</template>
