<template>
    <b-navbar toggleable="lg" type="dark">
        <!-- Navbar content -->
        <b-navbar-nav>
            <b-nav-item class="nav-link" to="/termsOfService" target="_blank"
                >Terms Of Service</b-nav-item
            >

            <b-nav-item
                class="nav-link"
                href="https://github.com/bcgov/healthgateway/wiki"
                target="_blank"
                >Release Notes</b-nav-item
            >

            <b-nav-item class="nav-link" to="/contactUs" target="_blank"
                >Contact Us</b-nav-item
            >

            <b-nav-item
                class="nav-link"
                href="https://www2.gov.bc.ca/gov/content/health/healthgateway"
                target="_blank"
                >About Us</b-nav-item
            >

            <b-nav-item class="nav-link" href="/contact-us"
                >Contact Us</b-nav-item
            >

            <b-nav-item class="nav-link" href="/termsOfService"
                >Terms of Service</b-nav-item
            >
        </b-navbar-nav>
    </b-navbar>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({});
</script>
