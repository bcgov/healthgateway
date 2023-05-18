<script setup lang="ts">
import { computed, useSlots } from "vue";

interface Props {
    title: string;
}
defineProps<Props>();

const slots = useSlots();

const hasSlot = computed(() => slots.default !== undefined);
const hasPrependSlot = computed(() => slots.prepend !== undefined);
</script>

<template>
    <div id="pageTitle">
        <b-row no-gutters class="justify-content-between">
            <b-col cols="auto" sm>
                <slot v-if="hasPrependSlot" name="prepend" />
                <h1 id="subject" class="mb-0 mt-2 d-inline-block">
                    {{ title }}
                </h1>
            </b-col>
            <b-col v-if="hasSlot" class="mb-0 ml-2">
                <slot />
            </b-col>
        </b-row>
        <hr class="my-2" />
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle h1 {
    font-size: 1.625rem;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
