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
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.container";
import { IUserProfileService } from "@/services/interfaces";

library.add(faCheckCircle, faTimesCircle);

@Component
export default class ValidateEmailView extends Vue {
    @Prop() inviteKey!: string;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

    private isLoading = true;
    private validatedValue = false;
    private resultStatus: ResultType | null = null;

    private get isVerified() {
        return this.resultStatus === ResultType.Success && this.validatedValue;
    }

    private get isAlreadyVerified() {
        return this.resultStatus === ResultType.Error && this.validatedValue;
    }

    private mounted() {
        this.verifyEmail();
    }

    private verifyEmail() {
        this.isLoading = true;
        const userProfileService: IUserProfileService = container.get(
            SERVICE_IDENTIFIER.UserProfileService
        );
        userProfileService
            .validateEmail(this.user.hdid, this.inviteKey)
            .then((result) => {
                this.validatedValue = result.resourcePayload;
                this.resultStatus = result.resultStatus;
                if (this.resultStatus == ResultType.Success) {
                    this.checkRegistration();
                }
            })
            .finally(() => {
                this.isLoading = false;
            });
    }
}
</script>

<template>
    <b-container>
        <b-row class="pt-5">
            <b-col class="text-center mb-5 title">
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
                <div v-else>
                    <hg-icon
                        icon="times-circle"
                        size="large"
                        aria-hidden="true"
                        class="text-danger"
                    />
                    <h4 data-testid="expiredInvite">
                        Your link is expired or incorrect. Please resend
                        verification email from your profile page
                    </h4>
                    <hg-button
                        data-testid="continueButton"
                        variant="primary"
                        @click="$router.push({ path: '/profile' })"
                    >
                        Continue
                    </hg-button>
                </div>
            </b-col>
        </b-row>
    </b-container>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
.title {
    color: $primary;
    font-size: 2.1em;
}
</style>
