<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.column-wrapper {
    border: 1px;
}

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
<template>
    <div>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col
                id="healthInsights"
                class="col-12 col-md-10 col-lg-9 column-wrapper"
            >
                <PageTitleComponent
                    :title="`Health Gateway Medication History Report`"
                />
                <div>
                    <p>
                        Download a copy of your PharmaNet record of prescription
                        medication dispenses. This report will generate your
                        full history in the PharmaNet system.
                    </p>
                </div>
            </b-col>
        </b-row>
        <b-row>
            <b-col>
                <img
                    class="mx-auto d-block"
                    src="@/assets/images/reports/reports.png"
                    width="200"
                    height="auto"
                    alt="..."
                />
            </b-col>
        </b-row>
        <b-row>
            <b-col>
                <b-button
                    variant="primary"
                    class="mx-auto mt-3 d-block"
                    @click="showConfirmationModal"
                >
                    Download your report
                </b-button>
            </b-col>
        </b-row>
        <MessageModalComponent
            ref="messageModal"
            title="Sensitive Document Download"
            message="The file that you are downloading contains personal information. If you are on a public computer, please ensure that the file is deleted before you log off."
            @submit="generateMedicationHistoryPdf"
        />
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import PageTitleComponent from "@/components/pageTitle.vue";
import { ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import BannerError from "@/models/bannerError";
import MessageModalComponent from "@/components/modal/genericMessage.vue";

@Component({
    components: {
        PageTitleComponent,
        MessageModalComponent,
    },
})
export default class ReportsView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    @Ref("messageModal")
    readonly messageModal!: MessageModalComponent;
    private showConfirmationModal() {
        this.messageModal.showModal();
    }
    private generateMedicationHistoryPdf() {
        this.logger.debug("generating Medication History PDF...");
    }
}
</script>
