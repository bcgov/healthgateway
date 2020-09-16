<template>
    <b-navbar toggleable="lg" type="dark">
        <!-- Navbar content -->
        <b-navbar-nav>
            <b-nav-item class="nav-link" to="/termsOfService"
                >Terms Of Service</b-nav-item
            >

            <b-nav-item class="nav-link" href="/release-notes"
                >Release Notes
            </b-nav-item>

            <b-nav-item class="nav-link" href="/contact-us"
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

<script lang="ts">
import Vue from "vue";
export default Vue.extend({});
</script>
