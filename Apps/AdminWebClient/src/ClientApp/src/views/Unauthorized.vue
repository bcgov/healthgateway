<script lang="ts">
import { Component, Vue } from "vue-property-decorator";
import { Getter } from "vuex-class";

@Component
export default class UnauthorizedView extends Vue {
    @Getter("isAuthenticated", { namespace: "auth" })
    private isAuthenticated!: boolean;

    private mounted(): void {
        if (!this.isAuthenticated) {
            this.$router.replace({ path: "/" });
        }
    }
}
</script>

<template>
    <v-container class="fill-height justify-center">
        <v-sheet elevation="1" class="pa-8" width="100%" max-width="500px">
            <div class="d-flex justify-center mb-4">
                <img
                    src="assets/images/health-gateway-logo.svg"
                    title="Health Gateway Logo"
                />
            </div>
            <h4 class="text-h4 text-center">
                <span class="d-none d-sm-block">Health Gateway Admin</span>
                <span class="d-block d-sm-none">HG Admin</span>
            </h4>
            <p class="my-4 pa-4 text-center">
                You are unauthorized to access this application. Please contact
                your administrator to get access to Health Gateway Admin.
            </p>

            <v-btn to="/signoff" color="primary" large block>Log Out</v-btn>
        </v-sheet>
    </v-container>
</template>
