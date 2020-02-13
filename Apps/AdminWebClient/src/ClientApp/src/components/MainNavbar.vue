<style lang="scss" scoped>
.v-list-item {
  border-radius: 4px;
}
</style>

<template>
  <v-navigation-drawer
    id="app-drawer"
    v-model="isDOpen"
    app
    dark
    mobile-break-point="959"
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

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Vue, Prop, Watch } from "vue-property-decorator";
import BackgroundImage from "@/assets/background.jpg";

const namespace: string = "drawer";

@Component
export default class MainNavbar extends Vue {
  @Prop({ default: "HealthGateway Admin" }) public title!: string;
  @Prop({ default: "fas fa-wrench" }) public icon!: string;
  @Action("setState", { namespace }) public setDrawer: any;
  @Getter("isOpen", { namespace }) public isOpen: boolean;

  private username: string = "User";
  private isDOpen: boolean = true;
  private drawer: boolean = false;
  private miniVariant: boolean = false;
  private right: boolean = true;

  private color: string = "success";

  private logo: string = "favicon.ico";
  private image: string = BackgroundImage;

  private items = [
    { title: "Dashboard", icon: "view_quilt", to: "/dashboard" },
    { title: "Hangfire", icon: "schedule", to: "/hangire" },
    { title: "Beta Invites", icon: "account_box", to: "/beta-invites" }
  ];

  mounted()
  {
    this.isDOpen = this.isOpen;
  }

  @Watch("isDOpen")
  public onDrawerChange(state: boolean) {
    this.setDrawer({isDrawerOpen:state});
  }

  @Watch("isOpen")
  public onStateDrawerChange(state: boolean) {
    this.isDOpen = state;
  }
}
</script>
