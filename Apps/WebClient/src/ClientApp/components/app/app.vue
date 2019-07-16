<template>
  <div id="app-root">
    <header>
      <environment exclude="Production">
        <div class="text-center bg-warning small">
          Non-production environment:
          <b>{{host}}</b>
        </div>
      </environment>
      <header-component />
    </header>
    <div>
      <router-view></router-view>
    </div>
    <footer-component />
  </div>
</template>


<script lang='ts' src="./app.ts">
</script>

