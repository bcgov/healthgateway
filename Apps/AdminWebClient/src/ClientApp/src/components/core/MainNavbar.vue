<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Vue, Prop, Watch } from "vue-property-decorator";

@Component
export default class MainNavbar extends Vue {
    @Action("setState", { namespace: "drawer" }) private setDrawerState!: ({
        isDrawerOpen
    }: any) => void;
    @Getter("isOpen", { namespace: "drawer" }) private isDrawerOpen!: boolean;
    @Getter("isAuthorized", { namespace: "auth" })
    private isAuthorized!: boolean;

    private isOpen = true;
    private drawer = false;

    private color = "success";

    private logo = "favicon.ico";
    private image = "./assets/images/background.jpg";

    private items = [
        {
            title: "Dashboard",
            icon: "view_quilt",
            to: "/"
        },
        {
            title: "JobScheduler",
            icon: "schedule",
            to: "/job-scheduler"
        },
        {
            title: "Beta Invites",
            icon: "account_box",
            to: "/beta-invites"
        },
        {
            title: "Resend Emails",
            icon: "email",
            to: "/admin-email"
        },
        {
            title: "Communications",
            icon: "email",
            to: "/communication"
        },
        { title: "Feedback Review", icon: "comment", to: "/user-feedback" },
        {
            title: "System Analytics",
            icon: "fa-download",
            to: "/stats"
        }
    ];

    private mounted() {
        this.isOpen = this.isDrawerOpen;
    }

    @Watch("isOpen")
    private onDrawerChange(state: boolean) {
        this.setDrawerState({ isDrawerOpen: state });
    }

    @Watch("isDrawerOpen")
    private onStateDrawerChange(state: boolean) {
        this.isOpen = state;
    }
}
</script>

<template>
    <v-navigation-drawer
        v-if="isAuthorized"
        id="app-drawer"
        v-model="isOpen"
        app
        dark
        mobile-breakpoint="959"
        width="260"
    >
        <v-img
            :src="image"
            height="100%"
            width="100%"
            gradient="to top right, rgba(25,32,72,.3), rgba(25,32,72,.9)"
        >
            <v-layout class="fill-height">
                <v-list width="260">
                    <v-list-item>
                        <v-list-item-avatar color="white">
                            <v-img :src="logo" height="34" contain />
                        </v-list-item-avatar>
                        <v-list-item-content>
                            <v-list-item-title class="title">
                                HealthGateway
                            </v-list-item-title>

                            <v-list-item-subtitle>
                                Admin
                            </v-list-item-subtitle>
                        </v-list-item-content>
                    </v-list-item>
                    <v-divider />
                    <v-list-item
                        v-for="(item, i) in items"
                        :key="i"
                        :to="item.to"
                        :active-class="color"
                        class="ma-3"
                    >
                        <v-list-item-action>
                            <v-icon>{{ item.icon }}</v-icon>
                        </v-list-item-action>
                        <v-list-item-title v-text="item.title" />
                    </v-list-item>
                </v-list>
            </v-layout>
        </v-img>
    </v-navigation-drawer>
</template>

<style lang="scss" scoped>
.v-list-item {
    border-radius: 4px;
}
</style>
