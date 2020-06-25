<template>
    <v-snackbar v-model="showSnackbar" :color="vuetifyType" top multi-line>
        <span>{{ feedback.message }}</span>
    </v-snackbar>
</template>

<script lang="ts">
import Vue from "vue";
import { Prop, Component, Watch } from "vue-property-decorator";
import BannerFeedback from "@/models/bannerFeedback";
import { ResultType } from "@/constants/resulttype";

@Component
export default class BannerFeedbackComponent extends Vue {
    @Prop() showFeedback!: boolean;
    @Prop() feedback!: BannerFeedback;

    private showBanner: boolean = false;

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
