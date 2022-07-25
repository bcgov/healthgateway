<script lang="ts">
import { Component, Vue, Watch } from "vue-property-decorator";
import { Action, Getter } from "vuex-class";

@Component
export default class ToolbarComponent extends Vue {
    @Action("setState", { namespace: "drawer" })
    private setDrawerState!: (params: { isDrawerOpen: boolean }) => void;

    @Getter("isOpen", { namespace: "drawer" })
    private isDrawerOpen!: boolean;

    @Getter("isAuthenticated", { namespace: "auth" })
    private isLoggedIn!: boolean;

    private title = "";
    private responsive = false;

    private mounted() {
        this.onResponsiveInverted();
        window.addEventListener("resize", this.onResponsiveInverted);
    }

    private beforeDestroy() {
        window.removeEventListener("resize", this.onResponsiveInverted);
    }

    @Watch("$route")
    private onIsAppIdleChanged() {
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
