<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { ResultType } from "@/constants/resulttype";
import { SnackbarPosition } from "@/constants/snackbarPosition";
import type BannerFeedback from "@/models/bannerFeedback";

@Component
export default class BannerFeedbackComponent extends Vue {
    @Prop() showFeedback!: boolean;
    @Prop() feedback!: BannerFeedback;
    @Prop({ default: SnackbarPosition.Top, required: false })
    position!: SnackbarPosition;

    private showBanner = false;

    private get snackbarAtTop(): boolean {
        return this.position === SnackbarPosition.Top;
    }

    private get snackbarAtBottom(): boolean {
        return this.position === SnackbarPosition.Bottom;
    }

    private get showSnackbar(): boolean {
        return this.showFeedback;
    }

    private set showSnackbar(show: boolean) {
        this.$emit("update:show-feedback", show);
    }

    private get vuetifyType(): string {
        switch (this.feedback.type) {
            case ResultType.Success: {
                return "success";
            }
            case ResultType.Error: {
                return "error";
            }
            default: {
                return "NONE";
            }
        }
    }
}
</script>

<template>
    <v-snackbar
        v-model="showSnackbar"
        :color="vuetifyType"
        :top="snackbarAtTop"
        :bottom="snackbarAtBottom"
        multi-line
    >
        <div>{{ feedback.title }}</div>
        <div>{{ feedback.message }}</div>
    </v-snackbar>
</template>
