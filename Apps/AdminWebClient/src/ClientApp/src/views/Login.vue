<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import VueRouter from "vue-router";
import { Action, Getter } from "vuex-class";

import LoadingComponent from "@/components/core/Loading.vue";
import { UserRoles } from "@/constants/userRoles";

const namespace = "auth";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class LoginView extends Vue {
    public name = "Dashboard";
    @Action("login", { namespace }) private login!: (params: {
        redirectPath: string;
    }) => Promise<void>;

    @Getter("isAuthenticated", { namespace }) private isAuthenticated!: boolean;
    @Getter("roles", { namespace: "auth" })
    private roles!: string[];

    private isLoading = true;
    private redirectPath = "";
    private routeHandler!: VueRouter;

    private get isSupportOnly(): boolean {
        return (
            this.roles.length === 1 && this.roles[0] === UserRoles.SupportUser
        );
    }

    private mounted() {
        this.isLoading = true;
        console.log(this.roles);

        if (this.$route.query.redirect && this.$route.query.redirect !== "") {
            this.redirectPath = this.$route.query.redirect.toString();
        } else {
            if (this.isSupportOnly) {
                this.redirectPath = "/covidcard";
            } else {
                this.redirectPath = "/";
            }
        }
        this.routeHandler = this.$router;
        if (this.isAuthenticated) {
            this.routeHandler.push({ path: this.redirectPath });
        }

        console.log("path", this.redirectPath);

        this.login({ redirectPath: this.redirectPath }).then(() => {
            if (this.isSupportOnly) {
                this.redirectPath = "/covidcard";
            }

            if (this.isAuthenticated) {
                this.routeHandler.push({ path: this.redirectPath });
            }
        });
    }
}
</script>

<template>
    <v-layout class="fill-height">
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>

        <v-row justify="center">Redirecting...</v-row>
    </v-layout>
</template>
