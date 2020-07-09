<style scoped lang="scss">
.error-message {
    color: #ff5252 !important;
}
</style>
<template>
    <v-container>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <BannerFeedbackComponent
            :show-feedback.sync="showFeedback"
            :feedback="bannerFeedback"
            class="mt-5"
        ></BannerFeedbackComponent>
        <v-row>
            <v-col>
                <v-row>
                    <v-col no-gutters>
                        <CommunicationTable
                            @is-finished-loading="isFinishedLoading"
                            @should-show-feedback="shouldShowFeedback"
                            @banner-feedback-info="bannerFeedbackInfo"
                        />
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>

<script lang="ts">
import { Component, Vue, Watch } from "vue-property-decorator";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/core/Loading.vue";
import BannerFeedbackComponent from "@/components/core/BannerFeedback.vue";
import BannerFeedback from "@/models/bannerFeedback";
import CommunicationTable from "../components/core/CommunicationTable.vue";
import EmailCommunication from "@/components/core/EmailCommunication.vue";
import Communication from "@/models/communication";
import { ResultType } from "@/constants/resulttype";
import { ICommunicationService } from "@/services/interfaces";
import { faWater } from "@fortawesome/free-solid-svg-icons";
import { required } from "vee-validate/dist/rules";
import moment from "moment";

@Component({
    components: {
        LoadingComponent,
        BannerFeedbackComponent,
        CommunicationTable,
        EmailCommunication
    }
})
export default class CommunicationView extends Vue {
    private isLoading: boolean = false;
    private showFeedback: boolean = false;
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
