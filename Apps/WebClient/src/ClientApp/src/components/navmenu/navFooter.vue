<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";
@Component
export default class FooterComponent extends Vue {
    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;
}
</script>

<template>
    <b-navbar v-show="isValidIdentityProvider" toggleable="lg" type="dark">
        <!-- Navbar content -->
        <b-navbar-nav>
            <b-nav-item class="nav-link" to="/termsOfService"
                >Terms of Service</b-nav-item
            >

            <b-nav-item class="nav-link" to="/release-notes"
                >Release Notes
            </b-nav-item>

            <b-nav-item class="nav-link" to="/contact-us"
                >Contact Us</b-nav-item
            >

            <b-nav-item
                class="nav-link"
                href="https://www2.gov.bc.ca/gov/content/health/healthgateway"
                target="_blank"
                >About Us
            </b-nav-item>
        </b-navbar-nav>
    </b-navbar>
</template>
