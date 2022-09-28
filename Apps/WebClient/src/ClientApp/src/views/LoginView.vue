<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import {
    faAddressCard,
    faUser,
    faUserSecret,
} from "@fortawesome/free-solid-svg-icons";
import Vue from "vue";
import { Component, Prop } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/LoadingComponent.vue";
import type { WebClientConfiguration } from "@/models/configData";
import { IdentityProviderConfiguration } from "@/models/configData";

library.add(faAddressCard, faUser, faUserSecret); // icons listed in config

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const options: any = {
    components: {
        LoadingComponent,
    },
};

@Component(options)
export default class LoginView extends Vue {
    @Prop() isRetry?: boolean;

    @Action("signIn", { namespace: "auth" })
    signIn!: (params: {
        redirectPath: string;
        idpHint?: string;
    }) => Promise<void>;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("userIsRegistered", { namespace: "user" })
    userIsRegistered!: boolean;

    @Getter("identityProviders", { namespace: "config" })
    identityProviders!: IdentityProviderConfiguration[];

    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private isLoading = true;
    private redirectPath = "";

    private mounted(): void {
        if (this.$route.query.redirect && this.$route.query.redirect !== "") {
            this.redirectPath = this.$route.query.redirect.toString();
        } else {
            this.redirectPath = "/home";
        }

        if (this.oidcIsAuthenticated && this.userIsRegistered) {
            this.$router.push({ path: this.redirectPath });
        } else if (this.oidcIsAuthenticated) {
            this.redirectPath = "/registration";
            this.$router.push({ path: this.redirectPath });
        } else if (
            !this.oidcIsAuthenticated &&
            this.identityProviders.length == 1
        ) {
            this.signInAndRedirect(this.identityProviders[0].hint);
        } else {
            this.isLoading = false;
        }
    }

    private get hasMultipleProviders(): boolean {
        return this.identityProviders.length > 1;
    }

    private signInAndRedirect(hint: string): void {
        this.signIn({
            redirectPath: this.redirectPath,
            idpHint: hint,
        }).then(() => {
            // if this code is reached, the user was already signed in
            this.$router.push({ path: this.redirectPath });
        });
    }
}
</script>

<template>
    <div class="container my-5">
        <LoadingComponent :is-loading="isLoading" />
        <b-alert
            style="max-width: 25rem"
            :show="isRetry"
            dismissible
            variant="danger"
        >
            <h4>Error</h4>
            <span
                >An unexpected error occured while processing the request,
                please try again.</span
            >
        </b-alert>
        <b-card
            v-if="identityProviders && identityProviders.length > 0"
            id="loginPicker"
            class="shadow-lg bg-white mx-auto"
            style="max-width: 25rem"
            align="center"
        >
            <h3 slot="header">Log In</h3>
            <b-card-body v-if="hasMultipleProviders || isRetry">
                <div v-for="provider in identityProviders" :key="provider.id">
                    <hg-button
                        :id="`${provider.id}Btn`"
                        :data-testid="`${provider.id}Btn`"
                        :disabled="provider.disabled"
                        variant="primary"
                        block
                        @click="signInAndRedirect(provider.hint)"
                    >
                        <b-row>
                            <b-col class="col-2">
                                <hg-icon
                                    :icon="`${provider.icon}`"
                                    size="medium"
                                />
                            </b-col>
                            <b-col class="text-left">
                                <span>{{ provider.name }}</span>
                            </b-col>
                        </b-row>
                    </hg-button>
                    <div
                        v-if="
                            identityProviders.indexOf(provider) <
                            identityProviders.length - 1
                        "
                    >
                        or
                    </div>
                </div>
            </b-card-body>
            <b-card-body v-else>
                <span
                    >Redirecting to
                    <strong>{{ identityProviders[0].name }}</strong
                    >...</span
                >
            </b-card-body>
        </b-card>
        <div v-else>No login providers configured</div>
    </div>
</template>
