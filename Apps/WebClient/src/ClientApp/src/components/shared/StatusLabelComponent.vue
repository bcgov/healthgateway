<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

@Component
export default class StatusLabelComponent extends Vue {
    @Prop({ required: true }) status!: string;
    @Prop({ required: false, default: "" }) variant!: string;
    @Prop({ required: false, default: "Status" }) heading!: string;

    private get classes(): string[] {
        switch (this.variant) {
            case "success":
                return ["text-success"];
            case "danger":
                return ["text-danger"];
            default:
                return [];
        }
    }
}
</script>

<template>
    <div class="hg-status-label">
        <span class="text-muted">{{ heading }}: </span>
        <span v-bind="$attrs" :class="classes">{{ status }}</span>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.hg-status-label {
    color: $hg-text-primary !important;

    .text-muted {
        color: $hg-text-secondary !important;
    }

    .text-success {
        color: $hg-state-success !important;
    }

    .text-danger {
        color: $hg-state-danger !important;
    }
}
</style>
