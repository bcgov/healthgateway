<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

import type { WebClientConfiguration } from "@/models/configData";

@Component
export default class LogoutCompleteView extends Vue {
    @Action("signOutOidcCallback", { namespace: "auth" })
    logoutCallback!: () => void;
    @Getter("webClient", { namespace: "config" })
    config!: WebClientConfiguration;

    private created() {
        setTimeout(() => {
            if (this.$route.path == "/logoutComplete") {
                this.$router.push({ path: "/" });
            }
        }, Number(this.config.timeouts.logoutRedirect));
    }

    private mounted() {
        this.logoutCallback();
    }
}
</script>

<template>
    <div class="row justify-content-center h-100 pt-5">
        <div class="col-lg-6 col-md-6 pt-2">
            <div class="shadow-lg p-3 mb-5 bg-white rounded border">
                <h3>
                    <strong>You signed out of your account</strong>
                </h3>
                <p>It's a good idea to close all browser windows.</p>
            </div>
        </div>
    </div>
</template>
