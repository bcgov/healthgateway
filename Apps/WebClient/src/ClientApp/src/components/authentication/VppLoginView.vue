<script setup lang="ts">
import { computed } from "vue";

import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { useConfigStore } from "@/stores/config";

const configStore = useConfigStore();
const webClientConfig = computed(() => configStore.webConfig);

function openAccessMyHealth() {
    const url: string | undefined = webClientConfig.value.accessMyHealthUrl;
    if (!url) return;
    window.location.assign(url);
}

function openHgHome() {
    const baseUrl = window.location.origin;
    window.location.assign(`${baseUrl}/login`);
}
</script>

<template>
    <div class="h-100 bg-grey-lighten-4 flex-grow-1 d-flex flex-column">
        <div class="bg-primary d-print-none">
            <router-link id="homeLink" to="/" aria-label="Return to home page">
                <img
                    class="img-fluid ma-3"
                    src="@/assets/images/gov/bcid-logo-rev-en.svg"
                    width="152"
                    alt="BC Mark"
                />
            </router-link>
        </div>
        <div class="d-flex flex-column">
            <div
                class="vpp-login-form bg-white rounded elevation-6 ma-2 ma-sm-4 py-6 py-sm-16 px-4 px-sm-16 align-self-center"
            >
                <p class="text-center">
                    <strong>Sign in with BC Services Card</strong>
                </p>
                <p class="text-center">
                    You can use your BC Services Card to sign into your Health
                    Gateway account.
                </p>

                <div class="d-flex flex-column flex-sm-row ga-2 justify-center">
                    <HgButtonComponent
                        variant="secondary"
                        text="Do not sign in"
                        :uppercase="false"
                        data-testid="do-not-sign-in-button"
                        @click="openAccessMyHealth()"
                    />
                    <HgButtonComponent
                        variant="primary"
                        text="Sign in"
                        :uppercase="false"
                        data-testid="sign-in-button"
                        @click="openHgHome()"
                    />
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss" scoped>
.vpp-login-form {
    width: 100%;
    max-width: 600px;
}
</style>
