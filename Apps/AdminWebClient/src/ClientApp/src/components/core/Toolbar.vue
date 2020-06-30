<style scoped>
#core-toolbar a {
    text-decoration: none;
}
.toolbar-items {
    text-decoration: none;
}

a {
    color: #e5e9ec !important;
    caret-color: #f4f4f4 !important;
}
</style>

<template>
    <v-app-bar id="core-toolbar" app flat>
        <div class="v-toolbar-title">
            <v-toolbar-title class="tertiary--text font-weight-light">
                <v-btn
                    v-if="responsive"
                    class="default v-btn--simple"
                    dark
                    icon
                    @click.stop="toggleDrawer"
                >
                    <v-icon>mdi-view-list</v-icon>
                </v-btn>
                <strong>{{ title }}</strong>
            </v-toolbar-title>
        </div>

        <v-spacer />
        <v-toolbar-items>
            <v-flex align-center layout py-2>
                <router-link
                    v-if="isLoggedIn"
                    v-ripple
                    class="toolbar-items"
                    to="/signoff"
                    color="tertiary"
                >
                    Logout
                    <v-icon>mdi-logout</v-icon>
                </router-link>
                <router-link
                    v-else
                    v-ripple
                    class="toolbar-items"
                    to="/"
                    color="tertiary"
                >
                    Login
                    <v-icon>mdi-login</v-icon>
                </router-link>
            </v-flex>
        </v-toolbar-items>
    </v-app-bar>
</template>

<script lang="ts">
import { Component, Vue, Watch, Ref } from "vue-property-decorator";
import { Route } from "vue-router";
import { State, Action, Getter } from "vuex-class";

import { mapMutations } from "vuex";

@Component
export default class ToolbarComponent extends Vue {
    @Action("setState", { namespace: "drawer" }) private setDrawerState!: ({
        isDrawerOpen
    }: any) => void;
    @Getter("isOpen", { namespace: "drawer" }) private isDrawerOpen!: boolean;
    @Getter("isAuthenticated", { namespace: "auth" })
    private isLoggedIn!: boolean;

    private title: string = "";
    private responsive: boolean = false;

    mounted() {
        this.onResponsiveInverted();
        window.addEventListener("resize", this.onResponsiveInverted);
    }

    beforeDestroy() {
        window.removeEventListener("resize", this.onResponsiveInverted);
    }

    @Watch("$route")
    public onIsAppIdleChanged(idle: boolean) {
        this.title = this.$route.name || "";
    }

    private toggleDrawer() {
        this.setDrawerState({ isDrawerOpen: !this.isDrawerOpen });
    }
    private onResponsiveInverted() {
        if (window.innerWidth < 959) {
            this.responsive = true;
        } else {
            this.responsive = false;
        }
    }
}
</script>
