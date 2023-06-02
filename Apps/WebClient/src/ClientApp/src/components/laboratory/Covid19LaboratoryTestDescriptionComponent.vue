<script setup lang="ts">
interface Props {
    description: string[];
    link: string;
}
const props = defineProps<Props>();

function isLastEntry(index: number): boolean {
    return index + 1 === props.description.length;
}

function shouldDisplayLink(index: number): boolean {
    return isLastEntry(index) && Boolean(props.link);
}
</script>

<template>
    <div>
        <p
            v-for="(paragraph, index) in description"
            :key="index"
            :class="{ 'mb-0': isLastEntry(index) }"
        >
            <span>{{ paragraph }}</span>
            <span v-if="shouldDisplayLink(index)">
                <a
                    data-testid="result-link"
                    class="link"
                    :href="link"
                    rel="noopener"
                    target="_blank"
                    >this page</a
                >.
            </span>
        </p>
    </div>
</template>

<style scoped>
a {
    cursor: pointer !important;
}
</style>
