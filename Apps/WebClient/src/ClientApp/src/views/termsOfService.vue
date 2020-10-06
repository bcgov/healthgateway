<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { ILogger, IUserProfileService } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/loading.vue";
import HtmlTextAreaComponent from "@/components/htmlTextarea.vue";

@Component({
    components: {
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
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row class="my-3 fluid">
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
                <b-row>
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="Subject">Terms of Service</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
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

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}

#termsOfService {
    background-color: $light_background;
    color: $soft_text;
}
</style>
