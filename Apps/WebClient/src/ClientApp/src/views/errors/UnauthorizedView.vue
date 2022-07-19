<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";

import PageErrorComponent from "@/components/PageErrorComponent.vue";
import { PageError } from "@/models/errors";

@Component({
    components: {
        ErrorComponent: PageErrorComponent,
    },
})
export default class UnauthorizedView extends Vue {
    public errorDescription: PageError = new PageError(
        "401",
        "Unauthorized",
        "You do not have permission to view this page."
    );
}
</script>

<template>
    <div>
        <ErrorComponent :error="errorDescription" />
    </div>
</template>
