<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faExclamationTriangle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import { sameAs } from "vuelidate/lib/validators";
import { Validation } from "vuelidate/vuelidate";
import { Action, Getter } from "vuex-class";

import HtmlTextAreaComponent from "@/components/HtmlTextAreaComponent.vue";
import LoadingComponent from "@/components/LoadingComponent.vue";
import { ErrorSourceType, ErrorType } from "@/constants/errorType";
import type { WebClientConfiguration } from "@/models/configData";
import { ResultError } from "@/models/errors";
import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger, IUserProfileService } from "@/services/interfaces";

library.add(faExclamationTriangle);

@Component({
    components: {
        LoadingComponent,
        HtmlTextAreaComponent,
    },
})
export default class AcceptTermsOfServiceView extends Vue {
    @Action("updateAcceptedTerms", { namespace: "user" })
    updateAcceptedTerms!: (params: {
        termsOfServiceId: string;
    }) => Promise<void>;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (params: {
        errorType: ErrorType;
        source: ErrorSourceType;
        traceId: string | undefined;
    }) => void;

    @Action("setTooManyRequestsError", { namespace: "errorBanner" })
    setTooManyRequestsError!: (params: { key: string }) => void;

    @Action("setTooManyRequestsWarning", { namespace: "errorBanner" })
    setTooManyRequestsWarning!: (params: { key: string }) => void;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("webClient", { namespace: "config" })
    webClientConfig!: WebClientConfiguration;

    @Ref("termsOfServiceForm")
    form!: HTMLFormElement;

    private accepted = false;
    private email = "";

    private userProfileService!: IUserProfileService;
    private loadingTermsOfService = true;

    private logger!: ILogger;

    private termsOfServiceId = "";
    private termsOfService = "";

    private get isLoading(): boolean {
        return this.loadingTermsOfService;
    }

    private mounted(): void {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.userProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );

        this.loadTermsOfService();
    }

    private validations(): unknown {
        return {
            accepted: { isChecked: sameAs(() => true) },
        };
    }

    private loadTermsOfService(): void {
        this.loadingTermsOfService = true;
        this.userProfileService
            .getTermsOfService()
            .then((result) => {
                this.logger.debug(
                    `getTermsOfService result: ${JSON.stringify(result)}`
                );
                this.termsOfServiceId = result.id;
                this.termsOfService = result.content;
            })
            .catch((err: ResultError) => {
                this.logger.error(err);
                if (err.statusCode === 429) {
                    this.setTooManyRequestsWarning({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Retrieve,
                        source: ErrorSourceType.TermsOfService,
                        traceId: undefined,
                    });
                }
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private onSubmit(event: Event): void {
        this.$v.$touch();
        if (this.$v.$invalid) {
            event.preventDefault();
            return;
        }

        this.loadingTermsOfService = true;

        this.updateAcceptedTerms({ termsOfServiceId: this.termsOfServiceId })
            .then(() => this.redirect())
            .catch((err: ResultError) => {
                this.logger.error(
                    `Error updating accepted terms of service: ${JSON.stringify(
                        err
                    )}`
                );
                if (err.statusCode === 429) {
                    this.setTooManyRequestsError({ key: "page" });
                } else {
                    this.addError({
                        errorType: ErrorType.Update,
                        source: ErrorSourceType.Profile,
                        traceId: err.traceId,
                    });
                }
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });

        event.preventDefault();
    }

    private redirect(): void {
        this.logger.debug("Redirecting to /home");
        this.$router.push({ path: "/home" });
    }
}
</script>

<template>
    <div>
        <LoadingComponent :is-loading="isLoading" />
        <b-container v-if="!isLoading && termsOfService !== ''">
            <div>
                <b-form ref="termsOfServiceForm" @submit.prevent="onSubmit">
                    <page-title
                        title="Update to our Terms of Service"
                        data-testid="tos-page-title"
                    />
                    <b-row class="mb-3">
                        <b-col>
                            <HtmlTextAreaComponent
                                class="termsOfService"
                                :input="termsOfService"
                                data-testid="tos-text-area-component"
                            />
                        </b-col>
                    </b-row>
                    <b-row class="mb-3">
                        <b-col>
                            <b-form-checkbox
                                id="accept-tos-checkbox"
                                v-model="accepted"
                                data-testid="accept-tos-checkbox"
                                class="accept"
                                :state="isValid($v.accepted)"
                            >
                                I agree to the terms of service above
                            </b-form-checkbox>
                            <b-form-invalid-feedback
                                :state="isValid($v.accepted)"
                            >
                                You must accept the terms of service.
                            </b-form-invalid-feedback>
                        </b-col>
                    </b-row>
                    <b-row class="mb-5">
                        <b-col class="justify-content-right">
                            <hg-button
                                class="px-5 float-right"
                                type="submit"
                                data-testid="continue-btn"
                                variant="primary"
                                :disabled="!accepted"
                                >Continue</hg-button
                            >
                        </b-col>
                    </b-row>
                </b-form>
            </div>
        </b-container>
    </div>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.accept label {
    color: $primary;
}

.termsOfService {
    max-height: 460px;
    overflow-y: scroll;
    box-shadow: 0 0 2px #00000070;
    border: none;
}
</style>
