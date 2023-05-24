<script setup lang="ts">
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { computed, defineProps, withDefaults } from "vue";

interface Props {
    size?: string;
    square?: boolean;
}
const props = withDefaults(defineProps<Props>(), {
    size: "medium",
    square: false,
});

const classes = computed<string[]>(() => {
    const result = ["hg-icon"];

    switch (props.size) {
        case "small":
            result.push("hg-small");
            break;
        case "medium":
            result.push("hg-medium");
            break;
        case "large":
            result.push("hg-large");
            break;
        case "extra-large":
            result.push("hg-extra-large");
            break;
        case "extra-extra-large":
            result.push("hg-extra-extra-large");
            break;
        default:
            return [];
    }

    if (props.square) {
        result.push("hg-square");
    }

    return result;
});
</script>

<template>
    <font-awesome-icon v-bind="$attrs" :class="classes" v-on="$listeners">
    </font-awesome-icon>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-icon {
    box-sizing: content-box;
    height: 1em;

    &.hg-small {
        font-size: 0.875rem;
    }

    &.hg-medium {
        font-size: 1rem;
    }

    &.hg-large {
        font-size: 1.5rem;
    }

    &.hg-extra-large {
        font-size: 2rem;
    }

    &.hg-extra-extra-large {
        font-size: 2.81rem;
    }

    &.hg-square {
        width: 1em;
    }
}
</style>
