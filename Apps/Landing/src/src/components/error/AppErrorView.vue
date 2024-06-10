<script setup lang="ts">
import { computed } from "vue";

import { AppErrorType } from "@/constants/errorType";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

const isTooManyRequests = computed(
    () => appStore.appError === AppErrorType.TooManyRequests
);
</script>

<template>
    <v-app>
        <v-app-bar class="px-3 px-sm-5" color="white" height="82">
            <a href="/">
                <v-img
                    alt="Go to Health Gateway home page"
                    src="@/assets/images/gov/hg-logo.png"
                    width="151"
                    height="82"
                />
            </a>
            <v-toolbar-title>Application Error!</v-toolbar-title>
        </v-app-bar>
        <v-main>
            <v-container class="fill-height">
                <v-row justify="center">
                    <v-col sm="10" md="6">
                        <v-alert
                            v-if="isTooManyRequests"
                            type="warning"
                            icon="circle-exclamation"
                            data-testid="app-warning"
                            variant="outlined"
                            border
                        >
                            We are unable to complete all actions because the
                            site is too busy. Please try again later.
                        </v-alert>
                        <v-alert
                            v-else
                            type="error"
                            data-testid="app-error"
                            variant="outlined"
                            border
                        >
                            Unable to load application. Please try refreshing
                            the page or come back later.
                        </v-alert>
                    </v-col>
                </v-row>
            </v-container>
        </v-main>
    </v-app>
</template>
