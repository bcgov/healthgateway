<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";
@Component
export default class FooterComponent extends Vue {
    @Getter("oidcIsAuthenticated", {
        namespace: "auth",
    })
    oidcIsAuthenticated!: boolean;

    @Getter("userIsRegistered", {
        namespace: "user",
    })
    userIsRegistered!: boolean;

    @Getter("isValidIdentityProvider", { namespace: "auth" })
    isValidIdentityProvider!: boolean;

    private get showFooter(): boolean {
        return (
            !this.oidcIsAuthenticated ||
            (this.userIsRegistered && this.isValidIdentityProvider)
        );
    }
}
</script>

<template>
    <b-navbar v-show="showFooter" toggleable="lg" type="dark">
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
