<script setup lang="ts">
import CommunicationComponent from "@/components/site/CommunicationComponent.vue";
import DevBannerComponent from "@/components/site/DevBannerComponent.vue";
import FooterComponent from "@/components/site/FooterComponent.vue";
import HeaderComponent from "@/components/site/HeaderComponent.vue";
</script>

<template>
    <v-app>
        <DevBannerComponent />
        <HeaderComponent />
        <v-main class="position-relative">
            <CommunicationComponent />
            <v-container class="pt-6">
                <router-view />
            </v-container>
        </v-main>
        <FooterComponent :order="-1" />
    </v-app>
</template>

<style lang="scss" scoped>
.v-container {
    max-width: var(--site-max-width);
}
</style>
