<template>
  <div>
    <b-navbar toggleable="lg" type="dark" variant="info" class="bg-primary border-bottom border-bcid-gold">
      <!-- Brand -->
      <b-navbar-brand href="#">
        <a href="http://www2.gov.bc.ca" class="navbar-brand h-100">
          <img
            src="/content/images/gov/WorkBC_lrg_RGB_rev.svg"
            alt="WorkBC"
            title="WorkBC"
            class="work-bc-logo h-100"
          />
        </a>
        Health Gateway
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
          <router-link class="nav-link" to="/counter">
            <span class="fa fa-plus"></span> Counter
          </router-link>
          <router-link class="nav-link" to="/fetchdata">
            <span class="fa fa-th-list"></span> Fetch data
          </router-link>
        </b-navbar-nav>

        <ul class="navbar-nav ml-auto">
          <li class="nav-item">
            <span class="navbar-text text-light">Hi User...</span>
          </li>
          <li class="nav-item">
            <a class="nav-link" href="/logout">Logout</a>
          </li>
        </ul>
      </b-collapse>
    </b-navbar>
  </div>
</template>

<style src="./navmenu.css" />
