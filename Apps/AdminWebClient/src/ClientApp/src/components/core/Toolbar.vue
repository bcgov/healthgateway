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
  <v-app-bar app id="core-toolbar" flat>
    <div class="v-toolbar-title">
      <v-toolbar-title class="tertiary--text font-weight-light">
        <v-btn
          v-if="responsive"
          class="default v-btn--simple"
          dark
          icon
          @click.stop="onClickBtn"
        >
          <v-icon>mdi-view-list</v-icon>
        </v-btn>
        <strong>{{ title }}</strong>
      </v-toolbar-title>
    </div>

    <v-spacer />
    <v-toolbar-items v-if="isLoggedIn">
      <v-flex align-center layout py-2>
        <router-link
          v-ripple
          class="toolbar-items"
          to="/logout"
          color="tertiary"
        >
          Logout
          <v-icon>mdi-exit-to-app</v-icon>
        </router-link>
      </v-flex>
    </v-toolbar-items>
  </v-app-bar>
</template>

<script lang="ts">
import { Component, Vue, Watch, Ref } from "vue-property-decorator";
import { State, Action, Getter } from "vuex-class";

import { mapMutations } from "vuex";

const namespace: string = "drawer";

@Component
export default class ToolbarComponent extends Vue {
  @Action("setState", { namespace }) setDrawer: any;
  @Getter("isOpen", { namespace }) isOpen: boolean;
  @Getter("isAuthenticated", { namespace: "auth" }) isLoggedIn: boolean;

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
    this.title = this.$route.name;
  }

  onClickBtn() {
    console.log(this.isOpen)
    this.setDrawer({ isDrawerOpen: !this.isOpen });
  }
  onClick() {
    //
  }
  onResponsiveInverted() {
    if (window.innerWidth < 959) {
      this.responsive = true;
    } else {
      this.responsive = false;
    }
  }
}

/*export default {
  data: () => ({}),

  mounted() {
    this.onResponsiveInverted();
    window.addEventListener("resize", this.onResponsiveInverted);
  },
  beforeDestroy() {
    window.removeEventListener("resize", this.onResponsiveInverted);
  },

  methods: {
    ...mapMutations("app", ["setDrawer", "toggleDrawer"]),
    onClickBtn() {
      this.setDrawer(!this.$store.state.app.drawer);
    },
    onClick() {
      //
    },
    onResponsiveInverted() {
      if (window.innerWidth < 991) {
        this.responsive = true;
      } else {
        this.responsive = false;
      }
    }
  }
};*/
</script>
