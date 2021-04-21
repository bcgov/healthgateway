<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class HgIcon extends Vue {
    @Prop({ required: false, default: "medium" }) size!: string;

    private get classes(): string[] {
        switch (this.size) {
            case "small":
                return ["hg-icon", "hg-small"];
            case "medium":
                return ["hg-icon", "hg-medium"];
            case "large":
                return ["hg-icon", "hg-large"];
            default:
                return [];
        }
    }
}
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
}
</style>
