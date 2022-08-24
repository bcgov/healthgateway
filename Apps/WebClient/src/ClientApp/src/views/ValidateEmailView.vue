<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faCheckCircle,
    faTimesCircle,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";

library.add(faCheckCircle, faTimesCircle);

@Component
export default class ValidateEmailView extends Vue {
    @Prop()
    inviteKey!: string;

    @Action("validateEmail", { namespace: "user" })
    validateEmail!: (params: {
        inviteKey: string;
    }) => Promise<RequestResult<boolean>>;

    @Getter("user", { namespace: "user" })
    user!: User;

    private isLoading = true;
    private validatedValue: boolean?;
    private resultStatus: ResultType?;

    private get isVerified(): boolean {
        return this.resultStatus === ResultType.Success && this.validatedValue;
    }

    private get isAlreadyVerified(): boolean {
        return this.resultStatus === ResultType.Error && this.validatedValue;
    }

    private get validationFailed(): boolean {
        return this.validatedValue === false;
    }

    private mounted(): void {
        this.verifyEmail();
    }

    private verifyEmail(): void {
        this.isLoading = true;
        this.validateEmail({ inviteKey: this.inviteKey })
            .then((result) => {
                this.validatedValue = result.resourcePayload;
                this.resultStatus = result.resultStatus;
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
</script>

<template>
    <b-container class="text-center title pt-4">
        <h4 v-if="isLoading" data-testid="verifingInvite">
            We are verifying your email...
        </h4>
        <div v-else-if="isVerified">
            <hg-icon
                icon="check-circle"
                size="extra-large"
                aria-hidden="true"
                class="text-success"
            />
            <h4 data-testid="verifiedInvite">
                Your email address has been verified
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                to="/home"
            >
                Continue
            </hg-button>
        </div>
        <div v-else-if="isAlreadyVerified">
            <hg-icon
                icon="check-circle"
                size="extra-large"
                aria-hidden="true"
                class="text-success"
            />
            <h4 data-testid="alreadyVerifiedInvite">
                Your email address is already verified
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                to="/home"
            >
                Continue
            </hg-button>
        </div>
        <div v-else-if="validationFailed">
            <hg-icon
                icon="times-circle"
                size="large"
                aria-hidden="true"
                class="text-danger"
            />
            <h4 data-testid="expiredInvite">
                Your link is expired or incorrect. Please resend verification
                email from your profile page
            </h4>
            <hg-button
                data-testid="continueButton"
                variant="primary"
                @click="$router.push({ path: '/profile' })"
            >
                Continue
            </hg-button>
        </div>
    </b-container>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

.title {
    color: $primary;
    font-size: 2.1em;
}
</style>
