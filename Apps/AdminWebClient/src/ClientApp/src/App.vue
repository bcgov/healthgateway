<template>
    <v-app id="healthGatewayAdmin">
        <ToolbarComponent />
        <MainNavbar title="HealthGateway Admin" />
        <v-content>
            <router-view />
        </v-content>
        <Footer />
    </v-app>
</template>

<script lang="ts">
import MainNavbar from "@/components/core/MainNavbar.vue";
import Footer from "@/components/core/Footer.vue";
import ToolbarComponent from "@/components/core/Toolbar.vue";

import { Component, Vue } from "vue-property-decorator";
import { config, library } from "@fortawesome/fontawesome-svg-core";

import { faEdit } from "@fortawesome/free-solid-svg-icons";
import { faTrash } from "@fortawesome/free-solid-svg-icons"
library.add(faEdit);
library.add(faTrash)

@Component({
    components: { MainNavbar, Footer, ToolbarComponent }
})
export default class App extends Vue {}
</script>
