<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import BreadcrumbComponent from "@/components/navmenu/BreadcrumbComponent.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { ResultError } from "@/models/errors";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

@Component({
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        HtmlTextAreaComponent,
    },
})
export default class TermsOfServiceView extends Vue {
    @Action("setTooManyRequestsWarning", { namespace: "errorBanner" })
    setTooManyRequestsWarning!: (params: { key: string }) => void;

    private logger!: ILogger;
    private userProfileService!: IUserProfileService;
    private isLoading = true;
    private hasErrors = false;
    private errorMessage = "";

    private termsOfService = "";

    private breadcrumbItems: BreadcrumbItem[] = [
        {
            text: "Terms of Service",
            to: "/termsOfService",
            active: true,
            dataTestId: "breadcrumb-terms-of-service",
        },
    ];

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.userProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );
        this.loadTermsOfService();
    }

    private loadTermsOfService(): void {
        this.isLoading = true;
        this.userProfileService
            .getTermsOfService()
            .then((result) => {
                this.logger.debug(
                    "Terms Of Service retrieved: " + JSON.stringify(result)
                );
                this.termsOfService = result.content;
            })
            .catch((err: ResultError) => {
                this.logger.error(err);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.hasErrors = true;
                    this.errorMessage = "Please refresh your browser.";
                }
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
</script>

<template>
    <div>
        <BreadcrumbComponent :items="breadcrumbItems" />
        <LoadingComponent :is-loading="isLoading" />
        <b-row>
            <b-col class="col-12 col-lg-9 column-wrapper">
                <b-row>
                    <b-col>
                        <b-alert :show="hasErrors" dismissible variant="danger">
                            <h4>Error</h4>
                            <p>
                                An unexpected error occured while processing the
                                request:
                            </p>
                            <span>{{ errorMessage }}</span>
                        </b-alert>
                    </b-col>
                </b-row>
                <page-title title="Terms of Service" />
                <div v-if="!isLoading">
                    <HtmlTextAreaComponent :input="termsOfService" />
                </div>
            </b-col>
        </b-row>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#termsOfService {
    background-color: $light_background;
    color: $soft_text;
}
</style>
