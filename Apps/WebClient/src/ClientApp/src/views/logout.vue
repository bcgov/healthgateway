<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action } from "vuex-class";

const namespace = "auth";

@Component
export default class LogoutView extends Vue {
    @Action("signOutOidc", { namespace }) logout!: () => void;

    private mounted() {
        this.logout();
    }
}
</script>

<template>
    <div class="row justify-content-center h-100 pt-5">
        <div class="col-lg-6 col-md-6 pt-2">
            <div>
                <h3>Signing out...</h3>
            </div>
        </div>
    </div>
</template>
