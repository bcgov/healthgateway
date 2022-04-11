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
    @Action("oidcSignInCallback", { namespace: "auth" })
    oidcSignInCallback!: () => Promise<string>;

    @Action("clearStorage", { namespace: "auth" }) clearStorage!: () => void;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: () => Promise<boolean>;

    @Getter("user", { namespace: "user" }) user!: User;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;

    private logger!: ILogger;

    private created() {
        this.oidcSignInCallback()
            .then((redirectPath) => {
                this.logger.debug(
                    `oidcSignInCallback for user: ${JSON.stringify(this.user)}`
                );

                // If the idp is valid, check the registration status and continue the route.
                // Otherwise the router will handle the path.
                if (this.isValidIdentityProvider) {
                    this.checkRegistration().then(() => {
                        this.$router
                            .push({ path: redirectPath })
                            .catch((error) => {
                                console.warn(error.message);
                            });
                    });
                } else {
                    this.$router.push({ path: redirectPath }).catch((error) => {
                        console.warn(error.message);
                    });
                }
            })
            .catch((err) => {
                // Login failed redirect it back to the login page.
                console.error("LoginCallback Error:", err);
                this.clearStorage();
                this.$router.push({
                    path: "/login",
                    query: { isRetry: "true" },
                });
            });
    }

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
    }
}
</script>

<template>
    <div>Waiting...</div>
</template>
