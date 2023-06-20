<template>
    <v-app>
        <HeaderComponent />
        <v-main>
            <HelloWorld />
        </v-main>
    </v-app>
</template>

<script setup lang="ts">
import HelloWorld from "@/components/HelloWorld.vue";
import HeaderComponent from "@/components/navmenu/HeaderComponent.vue";
</script>
