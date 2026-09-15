import vuetify from 'eslint-config-vuetify';

export default vuetify(
  {
    antfu: false,
    perfectionist: false,
    stylistic: false,
    ts: true,
    unicorn: false,
  },
  {
    files: ['**/*.vue'],
    rules: {
      'vue/attributes-order': 'off',
      'vue/custom-event-name-casing': ['error', 'kebab-case', { ignores: ['/^update:/u'] }],
      'vue/max-attributes-per-line': 'off',
      'vue/padding-line-between-tags': 'off',
      'vue/script-indent': 'off',
    },
  },
  {
    files: ['src/features/**/*.{ts,vue}', 'src/shared/**/*.{ts,vue}'],
    rules: {
      'no-restricted-imports': ['error', {
        patterns: [{
          group: ['@/features/*'],
          message: 'Feature modules must communicate through shared presentation or explicit inputs and events.',
        }],
      }],
    },
  },
);
