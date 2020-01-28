<template>
  <div id="mainnavbar">
    <v-navigation-drawer
      persistent
      :mini-variant="miniVariant"
      :clipped="clipped"
      v-model="drawer"
      enable-resize-watcher
      fixed
      app
    >
      <v-list>
        <v-list-item value="true" v-for="(item, i) in items" :key="i" :to="item.link">
          <v-list-item-action>
            <v-icon v-html="item.icon"></v-icon>
          </v-list-item-action>
          <v-list-item-content>
            <v-list-item-title v-text="item.title" />
          </v-list-item-content>
        </v-list-item>
      </v-list>
    </v-navigation-drawer>

    <v-app-bar app :clipped-left="clipped" color="dark-blue" dark>
      <v-app-bar-nav-icon @click.stop="drawer = !drawer"></v-app-bar-nav-icon>
      <v-btn class="d-none d-lg-flex" icon @click.stop="miniVariant = !miniVariant">
        <v-icon v-html="miniVariant ? 'chevron_right' : 'chevron_left'"></v-icon>
      </v-btn>
      <v-icon class="mx-4" v-html="icon"></v-icon>
      <v-toolbar-title class="mr-12 align-center" v-text="title"/>
      <v-spacer></v-spacer>
      <v-avatar v-if="username" size="36px">
        <img src="https://graph.facebook.com/689357002/picture" v-bind:alt="username" />
      </v-avatar>
    </v-app-bar>
  </div>
</template>

<script lang="ts">
import { Action, Getter } from 'vuex-class';
import { Component, Vue, Prop } from 'vue-property-decorator';

@Component
export default class MainNavbar extends Vue {
  @Prop({ default: 'HealthGateway Admin' }) public title!: string;
  @Prop({ default: 'fas fa-wrench' }) public icon!: string;

  private username: string = 'User';
  private clipped: boolean = true;
  private drawer: boolean = false;
  private miniVariant: boolean = false;
  private right: boolean = true;
  private items = [
    { title: 'Dashboard', icon: 'view_list', link: '/dashboard' },
  ];
}
</script>



