<script setup lang="ts">
import HgButtonComponent from "@/components/common/HgButtonComponent.vue";
interface Props {
    closeDelay?: number;
}
withDefaults(defineProps<Props>(), {
    closeDelay: 1000,
});
</script>

<template>
    <v-menu :close-delay="closeDelay" eager>
        <template #activator="{ props }">
            <HgButtonComponent
                aria-label="Privacy Statement"
                v-bind="props"
                tabindex="0"
                variant="link"
                data-testid="btn-privacy-statement"
                text="Privacy Statement"
                prepend-icon="info-circle"
                density="compact"
            />
        </template>
        <v-card max-width="700px">
            <v-card-text>
                Your information is being collected to provide you with your
                COVID‑19 test result under s. 26(c) of the
                <em>Freedom of Information and Protection of Privacy Act</em>.
                Contact the Ministry Privacy Officer at
                <a href="mailto:MOH.Privacy.Officer@gov.bc.ca"
                    >MOH.Privacy.Officer@gov.bc.ca</a
                >
                if you have any questions about this collection.
            </v-card-text>
        </v-card>
    </v-menu>
</template>
