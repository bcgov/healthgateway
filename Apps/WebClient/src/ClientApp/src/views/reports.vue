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
        <TimelineLoadingComponent v-if="isLoading"></TimelineLoadingComponent>
        <b-row class="my-3 fluid justify-content-md-center">
            <b-col
                id="healthInsights"
                class="col-12 col-md-10 col-lg-9 column-wrapper"
            >
                <PageTitleComponent
                    :title="`Health Gateway Drug History Report`"
                />
                <div>
                    <p>
                        This is where we can explain a bit what this page is
                        about. Lorem Ipsum is a way of writing bdas skaow dowsk
                        peosas
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
                <b-button variant="info" class="mx-auto d-block">
                    Download your report
                </b-button>
            </b-col>
        </b-row>
    </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Component, Ref, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import PageTitleComponent from "@/components/pageTitle.vue";
import ErrorCardComponent from "@/components/errorCard.vue";
import User from "@/models/user";
import { ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import BannerError from "@/models/bannerError";

const namespace: string = "user";

@Component({
    components: {
        PageTitleComponent,
        ErrorCard: ErrorCardComponent,
    },
})
export default class ReportsView extends Vue {
    @Getter("user", { namespace }) user!: User;
    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
}
</script>
