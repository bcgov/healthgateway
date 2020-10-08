<script lang="ts">
import Vue from "vue";
import { Component } from "vue-property-decorator";
import { ILogger } from "@/services/interfaces";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import container from "@/plugins/inversify.config";
import LoadingComponent from "@/components/loading.vue";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class DependentsView extends Vue {
    private logger: ILogger = container.get(SERVICE_IDENTIFIER.Logger);
    private isLoading = true;
    private hasErrors = false;
    private errorMessage = "";

    private handleError(error: string): void {
        this.hasErrors = true;
        this.errorMessage = error;
        this.logger.error(error);
    }

    private mounted() {
        this.isLoading = false;
    }
}
</script>
<template>
    <div>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
        <b-row class="my-3 fluid">
            <b-col class="col-12 col-lg-9 column-wrapper">
                <b-row>
                    <b-col>
                        <b-alert :show="hasErrors" dismissible variant="danger">
                            <h4>Error</h4>
                            <p>
                                An unexpected error occured while processing the
                                request:
                            </p>
                            <span>{{ errorMessage }}</span>
                        </b-alert>
                    </b-col>
                </b-row>
                <b-row>
                    <b-col>
                        <div id="pageTitle">
                            <h1 id="Subject">Dependents</h1>
                            <hr />
                        </div>
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
    </div>
</template>
<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

#pageTitle hr {
    border-top: 2px solid $primary;
}
</style>
