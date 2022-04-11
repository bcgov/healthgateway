<script lang="ts">
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component({
    components: {
        FontAwesomeIcon,
    },
})
export default class HgIconComponent extends Vue {
    @Prop({ required: false, default: "medium" }) size!: string;
    @Prop({ required: false, default: false }) square!: boolean;

    private get classes(): string[] {
        const result = ["hg-icon"];

        switch (this.size) {
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
            default:
                return [];
        }

        if (this.square === true) {
            result.push("hg-square");
        }

        return result;
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
    &.hg-extra-large {
        font-size: 2rem;
    }
    &.hg-square {
        width: 1em;
    }
}
</style>
