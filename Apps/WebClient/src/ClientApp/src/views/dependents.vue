<script lang="ts">
import Vue from "vue";
import { Component, Ref } from "vue-property-decorator";
import LoadingComponent from "@/components/loading.vue";
import NewDependentComponent from "@/components/modal/newDependent.vue";
import DependentCardComponent from "@/components/dependentCard.vue";
import type { Dependent } from "@/models/dependent";
import { IDependentService, ILogger } from "@/services/interfaces";
import container from "@/plugins/inversify.config";
import { SERVICE_IDENTIFIER } from "@/plugins/inversify";
import ErrorTranslator from "@/utility/errorTranslator";
import { Action, Getter } from "vuex-class";
import User from "@/models/user";
import BannerError from "@/models/bannerError";

@Component({
    components: {
        LoadingComponent,
        DependentCardComponent,
        NewDependentComponent,
    },
})
export default class DependentsView extends Vue {
    @Ref("newDependentModal")
    readonly newDependentModal!: NewDependentComponent;

    @Getter("user", { namespace: "user" }) user!: User;

    @Action("addError", { namespace: "errorBanner" })
    addError!: (error: BannerError) => void;

    private logger!: ILogger;
    private dependentService!: IDependentService;

    private isLoading = true;
    private dependents: Dependent[] = [];

    private mounted() {
        this.logger = container.get<ILogger>(SERVICE_IDENTIFIER.Logger);
        this.dependentService = container.get<IDependentService>(
            SERVICE_IDENTIFIER.DependentService
        );
        this.fetchDependents();
    }

    private fetchDependents() {
        this.isLoading = true;
        this.dependentService
            .getAll(this.user.hdid)
            .then((results) => {
                this.dependents = results;
            })
            .catch((err) => {
                this.logger.error(err);
                this.addError(
                    ErrorTranslator.toBannerError("Fetch Dependents Error", err)
                );
            })
            .finally(() => {
                this.isLoading = false;
            });
    }

    private showModal() {
        this.newDependentModal.showModal();
    }

    private hideModal() {
        this.newDependentModal.hideModal();
    }

    private needsUpdate() {
        this.fetchDependents();
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
                        <b-row id="pageTitle">
                            <b-col cols="7">
                                <h1 id="Subject">Dependents</h1>
                            </b-col>
                            <b-col cols="5" align-self="end">
                                <b-btn
                                    data-testid="addNewDependentBtn"
                                    variant="primary"
                                    class="float-right"
                                    @click="showModal()"
                                >
                                    <font-awesome-icon
                                        icon="user-plus"
                                        class="mr-2"
                                    >
                                    </font-awesome-icon
                                    >Add a new dependent</b-btn
                                >
                            </b-col>
                        </b-row>
                        <hr />
                    </b-col>
                </b-row>
                <b-row
                    v-for="dependent in dependents"
                    :key="dependent.hdid"
                    class="mt-2"
                >
                    <b-col>
                        <DependentCardComponent
                            :dependent="dependent"
                            @needs-update="needsUpdate"
                        />
                    </b-col>
                </b-row>
            </b-col>
        </b-row>
        <NewDependentComponent
            ref="newDependentModal"
            @show="showModal"
            @handle-submit="fetchDependents"
        />
    </div>
</template>
<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";

#pageTitle {
    color: $primary;
}

hr {
    border-top: 2px solid $primary;
}
</style>
