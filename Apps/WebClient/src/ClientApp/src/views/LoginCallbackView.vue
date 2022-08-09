<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import User from "@/models/user";
import container from "@/plugins/container";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import { ILogger } from "@/services/interfaces";

@Component
export default class LoginCallbackView extends Vue {
    @Action("signIn", { namespace: "auth" })
    signIn!: (params: {
        redirectPath: string;
        idpHint?: string;
    }) => Promise<void>;

    @Action("clearStorage", { namespace: "auth" })
    clearStorage!: () => void;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

    @Getter("user", { namespace: "user" })
    user!: User;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;

    private logger!: ILogger;
    private redirectPath = "/home";

    private async created(): Promise<void> {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);

        if (this.$route.query.redirect && this.$route.query.redirect !== "") {
            this.redirectPath = this.$route.query.redirect.toString();
        }

        try {
            await this.signIn({ redirectPath: this.redirectPath });
            this.logger.debug(`signIn for user: ${JSON.stringify(this.user)}`);

            // If the idp is valid, check the registration status and continue the route.
            // Otherwise the router will handle the path.
            if (this.isValidIdentityProvider) {
                await this.checkRegistration();
            }

            this.$router
                .push({ path: this.redirectPath })
                .catch((error) => this.logger.warn(error.message));
        } catch (error) {
            // login failed - redirect back to the login page
            this.logger.error(`LoginCallback Error: ${JSON.stringify(error)}`);
            this.clearStorage();
            this.$router.push({
                path: "/login",
                query: { isRetry: "true" },
            });
        }
    }
}
</script>

<template>
    <div>Waiting...</div>
</template>
