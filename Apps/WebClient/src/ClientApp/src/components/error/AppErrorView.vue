<script setup lang="ts">
import { computed } from "vue";

import HgAlertComponent from "@/components/common/HgAlertComponent.vue";
import { AppErrorType } from "@/constants/errorType";
import { useAppStore } from "@/stores/app";

const appStore = useAppStore();

const isTooManyRequests = computed(
    () => appStore.appError === AppErrorType.TooManyRequests
);
</script>

<template>
    <v-app>
        <v-app-bar
            class="d-flex align-center border-b-md border-background border-opacity-100 d-print-none"
            color="background"
        >
            <a href="/" style="display: inline-block">
                <img
                    src="@/assets/images/gov/hg-logo-rev.svg"
                    alt="Go to Health Gateway home page"
                    style="height: 48px; width: auto; padding-right: 16px"
                />
            </a>
            <v-toolbar-title style="font-size: 1.5rem">
                Application Error!
            </v-toolbar-title>
        </v-app-bar>
        <v-main>
            <v-container class="fill-height">
                <v-row justify="center">
                    <v-col sm="10" md="6">
                        <HgAlertComponent
                            v-if="isTooManyRequests"
                            type="warning"
                            data-testid="app-warning"
                            variant="outlined"
                            :center-content="true"
                        >
                            We are unable to complete all actions because the
                            site is too busy. Please try again later.
                        </HgAlertComponent>
                        <HgAlertComponent
                            v-else
                            type="error"
                            data-testid="app-error"
                            variant="outlined"
                            :center-content="true"
                        >
                            Unable to load application. Please try refreshing
                            the page or come back later.
                        </HgAlertComponent>
                    </v-col>
                </v-row>
            </v-container>
        </v-main>
    </v-app>
</template>
