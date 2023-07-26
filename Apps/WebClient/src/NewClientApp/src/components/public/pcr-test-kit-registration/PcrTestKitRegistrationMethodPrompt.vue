<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
import { PcrDataSource } from "@/constants/pcrTestDataSource";

const emit = defineEmits<{
    (e: "on-data-source-selected", pcrDataSource: PcrDataSource): void;
}>();

const setDataSource = (dataSource: PcrDataSource) => {
    emit("on-data-source-selected", dataSource);
};
</script>

<template>
    <h2 class="text-h5 font-weight-bold">
        Register your COVID‑19 test kit using one of the following methods:
    </h2>
    <div class="mt-12 text-center">
        <HgButtonComponent
            id="btnLogin"
            aria-label="BC Services Card Login"
            data-testid="btn-login"
            variant="primary"
            text="Log in with BC Services Card"
            @click="setDataSource(PcrDataSource.Keycloak)"
        />
        <v-row class="my-4" align="center">
            <v-col><v-divider role="presentation" /></v-col>
            <v-col cols="auto">
                <h2 class="text-h6 font-weight-bold text-medium-emphasis">
                    OR
                </h2>
            </v-col>
            <v-col><v-divider role="presentation" /></v-col>
        </v-row>
        <HgButtonComponent
            id="btn-manual"
            data-testid="btn-manual"
            variant="secondary"
            class="manual-enter-button"
            text="Manually Enter Your Information"
            @click="setDataSource(PcrDataSource.Manual)"
        />
    </div>
</template>
