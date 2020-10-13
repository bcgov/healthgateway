<script lang="ts">
import Vue from "vue";
import LoadingComponent from "@/components/loading.vue";
import { Component } from "vue-property-decorator";

@Component({
    components: {
        LoadingComponent,
    },
})
export default class NewDependentComponent extends Vue {
    private isVisible = false;
    private isLoading = true;

    public showModal(): void {
        this.isVisible = true;
    }

    public hideModal(): void {
        this.isVisible = false;
    }

    private mounted() {
        this.isLoading = false;
    }

    private handleOk(bvModalEvt: Event) {
        // Prevent modal from closing
        bvModalEvt.preventDefault();
        // Trigger submit handler
        this.handleSubmit();
    }

    private handleSubmit() {
        // Hide the modal manually
        this.$nextTick(() => {
            this.hideModal();
        });
    }
}
</script>

<template>
    <b-modal
        id="new-dependent-modal"
        v-model="isVisible"
        data-testid="newDependentModal"
        title="New Dependent"
        size="lg"
        header-bg-variant="primary"
        header-text-variant="light"
        centered
    >
        <b-row>
            <b-col>
                <form>
                    <b-row data-testid="newDependentModalText">
                        <b-col>
                            <b-row class="mb-2">
                                <b-col>
                                    <label for="firstName">First Name</label>
                                    <b-input
                                        id="firstName"
                                        data-testid="firstNameInput"
                                        placeholder="John"
                                    ></b-input>
                                </b-col>
                                <b-col>
                                    <label for="lastName">Last Name</label>
                                    <b-input
                                        id="lastName"
                                        data-testid="lastNameInput"
                                        placeholder="Doe"
                                    ></b-input>
                                </b-col>
                                <b-col>
                                    <label for="birthdate">Date of Birth</label>

                                    <b-form-input
                                        id="birthdate"
                                        data-testid="birthdateInput"
                                        placeholder="YYYY-MM-DD"
                                    ></b-form-input>
                                </b-col>
                            </b-row>
                            <b-row class="mb-4">
                                <b-col>
                                    <label for="phn">PHN</label>
                                    <b-form-input
                                        id="phn"
                                        data-testid="phnInput"
                                        placeholder="P347492"
                                    ></b-form-input>
                                </b-col>
                                <b-col>
                                    <b-row>
                                        <label for="gender">Gender</label>
                                    </b-row>
                                    <b-row>
                                        <b-dropdown
                                            id="gender"
                                            data-testid="genderInput"
                                            text="Select Options"
                                            variant="outline-secondary"
                                        >
                                            <b-dropdown-item>
                                                Male
                                            </b-dropdown-item>
                                            <b-dropdown-item>
                                                Female
                                            </b-dropdown-item>
                                            <b-dropdown-item>
                                                Other
                                            </b-dropdown-item>
                                            <b-dropdown-item>
                                                Prefer not to say
                                            </b-dropdown-item>
                                        </b-dropdown>
                                    </b-row>
                                </b-col>
                            </b-row>
                            <b-ro class="mb-2">
                                <b-col
                                    ><b-checkbox
                                        id="termsCheckbox"
                                        data-testid="termsCheckbox"
                                        >I confirm that I am the parent or
                                        guardian for this child, pursuant to
                                        Part 4, Division 3 of the Family Law
                                        Act, the Adoption Act, and/or the Child,
                                        Family and Community Services
                                        Act.</b-checkbox
                                    ></b-col
                                >
                            </b-ro>
                        </b-col>
                    </b-row>
                </form>
            </b-col>
        </b-row>
        <template #modal-footer>
            <b-row>
                <div class="mr-2">
                    <b-btn variant="primary">Register dependent</b-btn>
                </div>
                <div>
                    <b-btn variant="secondary">Cancel</b-btn>
                </div>
            </b-row>
        </template>
        <LoadingComponent :is-loading="isLoading"></LoadingComponent>
    </b-modal>
</template>

<style lang="scss" scoped>
@import "@/assets/scss/_variables.scss";
</style>
