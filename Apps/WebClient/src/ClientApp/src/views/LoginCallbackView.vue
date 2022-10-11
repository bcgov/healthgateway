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

    @Action("retrieveEssentialData", { namespace: "user" })
    retrieveEssentialData!: () => Promise<void>;

    @Action("retrieveProfile", { namespace: "user" })
    retrieveProfile!: () => Promise<void>;

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

            // If the identity provider is valid, the essential user data can be retrieved.
            // If the identity provider is invalid, the router will redirect to the appropriate error page.
            if (this.isValidIdentityProvider) {
                await this.retrieveEssentialData();
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
