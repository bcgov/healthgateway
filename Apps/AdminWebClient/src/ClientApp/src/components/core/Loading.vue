<template>
    <v-dialog v-model="isLoading" fullscreen dark transition="fade-transition">
        <v-container
            fluid
            fill-height
            style="background-color: rgba(255, 255, 255, 0.5);"
        >
            <v-layout justify-center align-center>
                <v-progress-circular indeterminate> </v-progress-circular>
            </v-layout>
        </v-container>
    </v-dialog>
</template>

<script lang="ts">
import { Action, Getter } from "vuex-class";
import { Component, Vue, Prop } from "vue-property-decorator";

@Component
export default class LoadingComponent extends Vue {
    @Prop() isLoading!: boolean;
}
</script>
