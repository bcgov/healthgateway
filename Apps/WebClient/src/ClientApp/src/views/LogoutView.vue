<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { Action } from "vuex-class";

const namespace = "auth";

@Component
export default class LogoutView extends Vue {
    @Action("signOut", { namespace })
    signOut!: () => void;

    private mounted(): void {
        this.signOut();
    }
}
</script>

<template>
    <b-row class="justify-content-center h-100 pt-5">
        <b-col md="6">
            <div class="p-3 mb-5">
                <h3>Signing out...</h3>
            </div>
        </b-col>
    </b-row>
</template>
