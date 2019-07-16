<template>
  <b-navbar toggleable="lg" type="dark">
    <!-- Brand -->
    <b-navbar-brand href="https://www2.gov.bc.ca">
      <img
        class="img-fluid d-none d-md-block"
        src="../../content/images/gov/bcid-logo-rev-en.svg"
        width="181"
        height="44"
        alt="B.C. Government Logo"
      />
      <img
        class="img-fluid d-md-none"
        src="../../content/images/gov/bcid-symbol-rev.svg"
        width="64"
        height="44"
        alt="B.C. Government Logo"
      />
    </b-navbar-brand>

    <b-navbar-toggle target="nav-collapse"></b-navbar-toggle>

    <!-- Navbar links -->
    <b-collapse id="nav-collapse" is-nav >
      <b-navbar-nav v-if="userType === 'user'">
        <router-link class="nav-link" to="/home" :exact="true">
          <span class="fa fa-home"></span> Home
        </router-link>
        <router-link class="nav-link" to="/home/immunizations">
          <span class="fa fa-syringe"></span> Immunizations
        </router-link>
      </b-navbar-nav>

      <!-- Right aligned nav items -->
      <b-navbar-nav class="ml-auto">
        <b-nav-item-dropdown text="Hi User..." v-if="isAuthenticated" right>
          <b-dropdown-item href="#">Logout</b-dropdown-item>
        </b-nav-item-dropdown>
        <router-link class="nav-link" to="/home" :exact="true" v-else>
          <span class="fa fa-home"></span> Login
        </router-link>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
</template>


<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  computed: {
    isAuthenticated() {
      //return this.$store.state.areas;
      return true;
    },
    userType() {
      //return this.$store.state.areas;
      return 'user';
    }
  }
});
</script>