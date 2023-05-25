<script setup lang="ts">
import { onMounted } from "vue";
import { useStore } from "vue-composition-wrapper";

const store = useStore();

function signOut(): void {
    store.dispatch("auth/signOut");
}

onMounted(() => {
    signOut();
});
</script>

<template>
    <b-row class="justify-content-center h-100 pt-5">
        <b-col md="6">
            <div class="p-3 mb-5">
                <h3>Signing out...</h3>
            </div>
        </b-col>
    </b-row>
</template>
