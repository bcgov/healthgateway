<template>
    <v-app id="healthGatewayAdmin">
        <ToolbarComponent />
        <MainNavbar title="HealthGateway Admin" />
        <v-main>
            <router-view />
        </v-main>
        <Footer />
    </v-app>
</template>

<script lang="ts">
import { library } from "@fortawesome/fontawesome-svg-core";
import { faEdit, faTrash } from "@fortawesome/free-solid-svg-icons";
import { Component, Vue } from "vue-property-decorator";

import Footer from "@/components/core/Footer.vue";
import MainNavbar from "@/components/core/MainNavbar.vue";
import ToolbarComponent from "@/components/core/Toolbar.vue";
library.add(faEdit);
library.add(faTrash);

@Component({
    components: { MainNavbar, Footer, ToolbarComponent },
})
export default class App extends Vue {}
</script>
