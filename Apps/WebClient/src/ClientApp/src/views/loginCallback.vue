<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import { ILogger } from "@/services/interfaces";
import User from "@/models/user";

@Component
export default class LoginCallbackView extends Vue {
    @Action("oidcSignInCallback", { namespace: "auth" })
    oidcSignInCallback!: () => Promise<string>;

    @Action("clearStorage", { namespace: "auth" }) clearStorage!: () => void;

    @Action("checkRegistration", { namespace: "user" })
    checkRegistration!: (params: { hdid: string }) => Promise<boolean>;

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
                    this.checkRegistration({ hdid: this.user.hdid }).then(
                        () => {
                            this.$router.push({ path: redirectPath });
                        }
                    );
                } else {
                    this.$router.push({ path: redirectPath });
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
