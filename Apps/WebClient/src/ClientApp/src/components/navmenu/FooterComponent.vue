<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Getter } from "vuex-class";

@Component
export default class FooterComponent extends Vue {
    @Getter("isOffline", { namespace: "config" })
    isOffline!: boolean;

    @Getter("oidcIsAuthenticated", { namespace: "auth" })
    oidcIsAuthenticated!: boolean;

    @Getter("isValidIdentityProvider", { namespace: "user" })
    isValidIdentityProvider!: boolean;

    @Getter("userIsRegistered", { namespace: "user" })
    userIsRegistered!: boolean;

    private get isFooterShown(): boolean {
        return (
            !this.isOffline &&
            (!this.oidcIsAuthenticated ||
                !this.isValidIdentityProvider ||
                this.userIsRegistered)
        );
    }
}
</script>

<template>
    <b-navbar
        v-show="isFooterShown"
        data-testid="footer"
        toggleable="lg"
        type="dark"
        aria-label="Footer Nav"
    >
        <!-- Navbar content -->
        <b-navbar-nav>
            <b-nav-item class="nav-link" to="/termsOfService">
                Terms of Service
            </b-nav-item>
            <b-nav-item class="nav-link" to="/release-notes">
                Release Notes
            </b-nav-item>
            <b-nav-item
                class="nav-link"
                href="https://www2.gov.bc.ca/gov/content/health/managing-your-health/health-gateway"
                target="_blank"
            >
                About Us
            </b-nav-item>
            <b-nav-item
                class="nav-link"
                href="https://www2.gov.bc.ca/gov/content?id=FE8BA7F9F1F0416CB2D24CF71C4BAF80"
                target="_blank"
            >
                FAQ
            </b-nav-item>
        </b-navbar-nav>
        <b-navbar-nav class="ml-auto">
            <b-nav-item class="nav-link" href="mailto:healthgateway@gov.bc.ca">
                Email: HealthGateway@gov.bc.ca
            </b-nav-item>
        </b-navbar-nav>
    </b-navbar>
</template>
