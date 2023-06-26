<script setup lang="ts">
import { computed } from "vue";

import { AppErrorType } from "@/constants/errorType";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

const isTooManyRequests = computed<boolean>(() => {
    return appStore.appError === AppErrorType.TooManyRequests;
});
</script>

<template>
    <v-layout>
        <v-app-bar
            class="border-b-md border-accent border-opacity-100"
            color="primary"
        >
            <v-img
                alt="Go to Health Gateway home page"
                src="@/assets/images/gov/hg-logo-rev.svg"
                max-width="143px"
                class="pa-1"
            />
            <v-toolbar-title>Application Error!</v-toolbar-title>
        </v-app-bar>
        <v-main>
            <v-container>
                <v-row justify="center">
                    <v-col sm="10" md="6">
                        <v-alert
                            v-if="isTooManyRequests"
                            type="warning"
                            data-testid="app-warning"
                        >
                            We are unable to complete all actions because the
                            site is too busy. Please try again later.
                        </v-alert>
                        <v-alert v-else type="error" data-testid="app-error">
                            Unable to load application. Please try refreshing
                            the page or come back later.
                        </v-alert>
                    </v-col>
                </v-row>
            </v-container>
        </v-main>
    </v-layout>
</template>
