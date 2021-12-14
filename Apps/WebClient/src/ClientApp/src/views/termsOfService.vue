<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import HtmlTextAreaComponent from "@/components/htmlTextarea.vue";
import LoadingComponent from "@/components/loading.vue";
import BreadcrumbComponent from "@/components/navmenu/breadcrumb.vue";
import BreadcrumbItem from "@/models/breadcrumbItem";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { ILogger, IUserProfileService } from "@/services/interfaces";

@Component({
    components: {
        BreadcrumbComponent,
        LoadingComponent,
        HtmlTextAreaComponent,
    },
})
export default class TermsOfServiceView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);

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
        },
    ];

    private mounted() {
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
            .catch((err) => {
                this.logger.error(err);
                this.handleError("Please refresh your browser.");
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private handleError(error: string): void {
        this.hasErrors = true;
        this.errorMessage = error;
        this.logger.error(error);
    }
}
</script>

<template>
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
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
                    <b-row class="mb-3">
                        <b-col>
                            <HtmlTextAreaComponent :input="termsOfService" />
                        </b-col>
                    </b-row>
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
