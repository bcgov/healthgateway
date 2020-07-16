<template>
    <v-layout class="fill-height">
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>

        <v-row justify="center">Redirecting...</v-row>
    </v-layout>
</template>

<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import VueRouter, { Route } from "vue-router";
import { State, Action, Getter } from "vuex-class";
import LoadingComponent from "@/components/core/Loading.vue";

const namespace: string = "auth";

@Component({
    components: {
        LoadingComponent
    }
})
export default class LoginView extends Vue {
    public name = "Dashboard";
    @Action("login", { namespace }) private login!: ({
        redirectPath
    }: any) => Promise<void>;
    @Getter("isAuthenticated", { namespace }) private isAuthenticated!: boolean;

    private isLoading: boolean = true;
    private redirectPath: string = "";
    private routeHandler!: VueRouter;

    mounted() {
        this.isLoading = true;
        if (this.$route.query.redirect && this.$route.query.redirect !== "") {
            this.redirectPath = this.$route.query.redirect.toString();
        } else {
            this.redirectPath = "/";
        }
        this.routeHandler = this.$router;
        if (this.isAuthenticated) {
            this.routeHandler.push({ path: this.redirectPath });
        }

        console.log("path", this.redirectPath);

        this.login({ redirectPath: this.redirectPath }).then(result => {
            if (this.isAuthenticated) {
                this.routeHandler.push({ path: this.redirectPath });
            }
        });
    }
}
</script>
