import vuetify from "eslint-config-vuetify";
import stylistic from "@stylistic/eslint-plugin";

const featureNames = [
  "auth",
  "contributions",
  "family",
  "onboarding",
  "profile",
  "savings",
  "shop",
  "streaks",
  "world",
];
const parentImportPattern = {
  group: ["../*", "../**"],
  message: "Use the @/ alias for parent-directory imports; reserve relative imports for files in the same directory.",
};

export default vuetify(
  {
    antfu: false,
    perfectionist: false,
    stylistic: false,
    ts: true,
    unicorn: false,
  },
  {
    plugins: {
      "@stylistic": stylistic,
    },
    rules: {
      "@stylistic/quotes": ["error", "double", { allowTemplateLiterals: "avoidEscape" }],
      "@stylistic/semi": ["error", "always"],
      "func-style": ["error", "expression", { allowArrowFunctions: true }],
    },
  },
  {
    files: ["src/**/*.{ts,vue}"],
    rules: {
      "no-nested-ternary": "error",
      "no-restricted-imports": ["error", {
        patterns: [parentImportPattern],
      }],
    },
  },
  {
    files: ["**/*.vue"],
    rules: {
      "vue/attributes-order": "off",
      "vue/custom-event-name-casing": ["error", "kebab-case", { ignores: ["/^update:/u"] }],
      "vue/max-attributes-per-line": "off",
      "vue/no-restricted-syntax": ["error",
        { selector: "VExpressionContainer ConditionalExpression", message: "Move conditional template logic to a computed value or typed view model." },
      ],
      "vue/padding-line-between-tags": "off",
      "vue/script-indent": "off",
    },
  },
  {
    files: ["src/shared/**/*.{ts,vue}"],
    rules: {
      "no-restricted-imports": ["error", {
        patterns: [
          parentImportPattern,
          {
            group: ["@/features/*", "@/features/**"],
            message: "Shared modules must not depend on feature modules.",
          },
        ],
      }],
    },
  },
  ...featureNames.map((featureName) => ({
    files: [`src/features/${featureName}/**/*.{ts,vue}`],
    rules: {
      "no-restricted-imports": ["error", {
        patterns: [
          parentImportPattern,
          {
            group: featureNames
              .filter((otherFeatureName) => otherFeatureName !== featureName)
              .map((otherFeatureName) => `@/features/${otherFeatureName}/**`),
            message: "Feature modules must communicate through shared presentation or explicit inputs and events.",
          },
        ],
      }],
    },
  })),
);
