<template>
  <p class="contribution-description text-caption text-medium-emphasis">
    <template v-for="(part, index) in parts" :key="`${index}-${part.value}`">
      <a
        v-if="part.link"
        class="contribution-description__link"
        :href="part.value"
        rel="noopener noreferrer"
        target="_blank"
      >
        {{ part.value }}
      </a>
      <template v-else>{{ part.value }}</template>
    </template>
  </p>
</template>

<script lang="ts" setup>
import { computed } from "vue";

interface Props {
  text: string;
}

interface DescriptionPart {
  link: boolean;
  value: string;
}

const props = defineProps<Props>();
const webLinkPattern = /https?:\/\/[^\s<>"']+/giu;
const parts = computed<DescriptionPart[]>(() => {
  const result: DescriptionPart[] = [];
  let cursor = 0;

  for (const match of props.text.matchAll(webLinkPattern)) {
    const index = match.index;

    if (index > cursor) {
      result.push({ link: false, value: props.text.slice(cursor, index) });
    }

    result.push({ link: true, value: match[0] });
    cursor = index + match[0].length;
  }

  if (cursor < props.text.length) {
    result.push({ link: false, value: props.text.slice(cursor) });
  }

  return result;
});
</script>

<style lang="scss" scoped>
.contribution-description {
  white-space: pre-wrap;
  overflow-wrap: anywhere;
}

.contribution-description__link {
  color: currentColor;
  font-weight: 600;
  text-decoration: underline;
  text-underline-offset: 0.15em;
}
</style>
