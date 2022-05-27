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
import { ResultError } from "@/models/requestResult";
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
    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

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

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        this.userProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );

        this.loadTermsOfService();
    }

    private validations() {
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
            .catch((err) => {
                this.logger.error(err);
                this.addError({
                    errorType: ErrorType.Retrieve,
                    source: ErrorSourceType.TermsOfService,
                    traceId: undefined,
                });
            })
            .finally(() => {
                this.loadingTermsOfService = false;
            });
    }

    private isValid(param: Validation): boolean | undefined {
        return param.$dirty ? !param.$invalid : undefined;
    }

    private onSubmit(event: Event) {
        this.$v.$touch();
        if (this.$v.$invalid) {
            event.preventDefault();
            return;
        }

        this.loadingTermsOfService = true;

        this.updateAcceptedTerms({ termsOfServiceId: this.termsOfServiceId })
            .then(() => {
                this.redirect();
            })
            .catch((err: ResultError) => {
                this.logger.error(
                    `Error updating accepted terms of service: ${JSON.stringify(
                        err
                    )}`
                );
                this.addError({
                    errorType: ErrorType.Update,
                    source: ErrorSourceType.Profile,
                    traceId: err.traceId,
                });
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
    <div class="m-3 m-md-4 flex-grow-1 d-flex flex-column">
        <LoadingComponent :is-loading="isLoading" />
        <b-container v-if="!isLoading && termsOfService !== ''">
            <div>
                <b-form ref="termsOfServiceForm" @submit.prevent="onSubmit">
                    <page-title title="Update to our Terms of Service" />
                    <b-row class="mb-3">
                        <b-col>
                            <HtmlTextAreaComponent
                                class="termsOfService"
                                :input="termsOfService"
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
