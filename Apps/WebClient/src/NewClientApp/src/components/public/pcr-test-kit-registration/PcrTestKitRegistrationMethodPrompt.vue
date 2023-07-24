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
    <v-container>
        <v-row>
            <v-col>
                <h2 class="text-h5 font-weight-bold">
                    Register your COVID‑19 test kit using one of the following
                    methods:
                </h2>
            </v-col>
        </v-row>
        <v-row class="mt-12">
            <v-col>
                <v-row align="center">
                    <v-col class="text-center">
                        <HgButtonComponent
                            id="btnLogin"
                            aria-label="BC Services Card Login"
                            data-testid="btn-login"
                            variant="primary"
                            text="Log in with BC Services Card"
                            @click="setDataSource(PcrDataSource.Keycloak)"
                        />
                    </v-col>
                </v-row>
                <v-row align="center">
                    <v-col>
                        <hr />
                    </v-col>
                    <v-col cols="auto">
                        <h3 class="text-h6 text-medium-emphasis">OR</h3>
                    </v-col>
                    <v-col>
                        <hr />
                    </v-col>
                </v-row>
                <v-row align="center">
                    <v-col class="text-center">
                        <HgButtonComponent
                            id="btn-manual"
                            data-testid="btn-manual"
                            variant="secondary"
                            class="manual-enter-button"
                            text="Manually Enter Your Information"
                            @click="setDataSource(PcrDataSource.Manual)"
                        />
                    </v-col>
                </v-row>
            </v-col>
        </v-row>
    </v-container>
</template>
