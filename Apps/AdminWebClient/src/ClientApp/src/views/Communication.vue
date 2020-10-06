<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import CommunicationTable from "../components/core/CommunicationTable.vue";
import { ResultType } from "@/constants/resulttype";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
        CommunicationTable
    }
})
export default class CommunicationView extends Vue {
    private isLoading = false;
    private showFeedback = false;
    private bannerFeedback: BannerFeedback = {
        type: ResultType.NONE,
        title: "",
        message: ""
    };

    private shouldShowFeedback(show: boolean) {
        this.showFeedback = show;
    }

    private isFinishedLoading(loading: boolean) {
        this.isLoading = loading;
    }

    private bannerFeedbackInfo(banner: BannerFeedback) {
        this.bannerFeedback = banner;
    }
}
</script>

<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        ></BannerFeedbackComponent>
        <v-row>
            <v-col no-gutters>
                <CommunicationTable
                    @is-finished-loading="isFinishedLoading"
                    @should-show-feedback="shouldShowFeedback"
                    @banner-feedback-info="bannerFeedbackInfo"
                />
            </v-col>
        </v-row>
    </v-container>
</template>
