<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faTimesCircle } from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import { ResultType } from "@/constants/resulttype";
import User from "@/models/user";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { IUserProfileService } from "@/services/interfaces";
library.add(faTimesCircle);

@Component
export default class ValidateEmailView extends Vue {
    @Prop() inviteKey!: string;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: (params: { hdid: string }) => Promise<boolean>;

    private isLoading = true;
    private resultStatus: ResultType | null = null;

    private get isSuccess() {
        return this.resultStatus === ResultType.Success;
    }
    private get isActionRequired() {
        return this.resultStatus === ResultType.ActionRequired;
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
                this.resultStatus = result.resultStatus;
                if (this.resultStatus == ResultType.Success) {
                    this.checkRegistration({ hdid: this.user.hdid });
                    setTimeout(
                        () => this.$router.push({ path: "/timeline" }),
                        2000
                    );
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
                <h4 v-if="isLoading">We are verifying your email...</h4>
                <h4 v-else-if="isSuccess" class="text-success">
                    Your email was successfully verified!
                </h4>
                <h4 v-else-if="isActionRequired">
                    Your verification link is expired. Resend your verification
                    email from the
                    <router-link to="/profile"> Profile Page</router-link>.
                </h4>
                <h4 v-else>
                    Something is not right, are you sure this is the correct
                    link?
                </h4>
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
