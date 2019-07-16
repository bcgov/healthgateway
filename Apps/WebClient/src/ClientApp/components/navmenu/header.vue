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
    <b-collapse id="nav-collapse" is-nav>
      <b-navbar-nav>
        <router-link class="nav-link" to="/" :exact="true">
          <span class="fa fa-home"></span> Home
        </router-link>
        <router-link class="nav-link" to="/immunizations">
          <span class="fa fa-syringe"></span> Immunizations
        </router-link>
      </b-navbar-nav>

      <!-- Right aligned nav items -->
      <b-navbar-nav class="ml-auto">
        <b-nav-item-dropdown text="Hi User..." right>
          <b-dropdown-item href="#">Logout</b-dropdown-item>
        </b-nav-item-dropdown>
      </b-navbar-nav>
    </b-collapse>
  </b-navbar>
</template>