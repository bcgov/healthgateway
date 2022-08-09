<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import VueRouter from "vue-router";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/core/Loading.vue";

const { isNavigationFailure, NavigationFailureType } = VueRouter;

@Component({
    components: {
        LoadingComponent,
    },
})
export default class LoginView extends Vue {
    @Action("login", { namespace: "auth" })
    private login!: (params: { redirectPath: string }) => Promise<void>;

    @Getter("isAuthenticated", { namespace: "auth" })
    private isAuthenticated!: boolean;

    private routeHandler!: VueRouter;

    private mounted() {
        let redirectPath = "/";
        if (this.$route.query.redirect !== "") {
            redirectPath = this.$route.query.redirect.toString();
        }

        this.routeHandler = this.$router;
        this.redirectIfAuthenticated(redirectPath);

        console.log("Redirect path: " + redirectPath);

        this.login({ redirectPath }).then(() =>
            this.redirectIfAuthenticated(redirectPath)
        );
    }

    private redirectIfAuthenticated(path: string): void {
        if (this.isAuthenticated) {
            this.routeHandler.push({ path }).catch((err) => {
                if (
                    !isNavigationFailure(err, NavigationFailureType.redirected)
                ) {
                    console.error(err);
                }
            });
        }
    }
}
</script>

<template>
    <v-layout class="fill-height">
        <LoadingComponent :is-loading="true" />

        <v-row justify="center">Redirecting...</v-row>
    </v-layout>
</template>
