<script lang="ts">
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";

import { ResultType } from "@/constants/resulttype";
import type BannerFeedback from "@/models/bannerFeedback";

@Component
export default class BannerFeedbackComponent extends Vue {
    @Prop() showFeedback!: boolean;
    @Prop() feedback!: BannerFeedback;

    private showBanner = false;

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
    <v-snackbar v-model="showSnackbar" :color="vuetifyType" top multi-line>
        <div>{{ feedback.title }}</div>
        <div>{{ feedback.message }}</div>
    </v-snackbar>
</template>
