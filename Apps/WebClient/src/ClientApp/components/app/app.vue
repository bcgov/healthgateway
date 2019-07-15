<template>
  <div id="app-root">
    <header role="banner">
      <!--div
        id="header-main"
        class="navbar navbar-dark bg-primary fixed-top border-bottom border-bcid-gold"
      >
        <a href="http://www2.gov.bc.ca" class="navbar-brand h-100">
          <img
            src="/content/images/gov/WorkBC_lrg_RGB_rev.svg"
            alt="WorkBC"
            title="WorkBC"
            class="work-bc-logo h-100"
          />
        </a>
        <div>
          <a href role="button" id="language" lang="fr-CA" class="text-white">TODO:Français</a>
        </div>
      </div-->
      <menu-component />
    </header>

    <div class="container-fluid">
      <div class="row">
        <div class="col-sm-12">
          <router-view></router-view>
        </div>
      </div>
    </div>
  </div>
</template>


<script lang='ts' src="./app.ts">
</script>

