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
  message: "Use the @/ alias for imports outside the current directory; reserve relative imports for sibling files.",
};
const nestedRelativeImportPattern = {
  regex: String.raw`^\./.+/.+$`,
  message: "Use the @/ alias for imports outside the current directory; reserve relative imports for sibling files.",
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
      "func-style": ["error", "declaration", { allowArrowFunctions: true }],
      curly: ["error", "all"],
      "@stylistic/padding-line-between-statements": [
        "error",
        { blankLine: "always", prev: "*", next: ["if", "for", "while", "try"] },
        { blankLine: "always", prev: ["if", "for", "while", "try"], next: "*" },
        { blankLine: "always", prev: "*", next: "return" },
      ],
      "vue/html-closing-bracket-newline": [
        "error",
        { singleline: "never", multiline: "always", selfClosingTag: { multiline: "always" } },
      ],
      "vue/html-self-closing": [
        "error",
        {
          html: {
            void: "always",
            normal: "never",
            component: "always",
          },
          svg: "always",
          math: "always",
        },
      ],
    },
  },
  {
    files: ["src/**/*.{ts,vue}"],
    rules: {
      "no-nested-ternary": "error",
      "no-restricted-imports": [
        "error",
        {
          patterns: [parentImportPattern, nestedRelativeImportPattern],
        },
      ],
    },
  },
  {
    files: ["**/*.vue"],
    rules: {
      "vue/attributes-order": "off",
      "vue/custom-event-name-casing": ["error", "kebab-case", { ignores: ["/^update:/u"] }],
      "vue/max-attributes-per-line": "off",
      "vue/no-restricted-syntax": [
        "error",
        {
          selector: "VExpressionContainer ConditionalExpression",
          message: "Move conditional template logic to a computed value or typed view model.",
        },
      ],
      "vue/padding-line-between-tags": "off",
      "vue/script-indent": "off",
    },
  },
  {
    files: ["src/shared/**/*.{ts,vue}"],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            parentImportPattern,
            nestedRelativeImportPattern,
            {
              group: ["@/features/*", "@/features/**"],
              message: "Shared modules must not depend on feature modules.",
            },
          ],
        },
      ],
    },
  },
  ...featureNames.map((featureName) => ({
    files: [`src/features/${featureName}/**/*.{ts,vue}`],
    rules: {
      "no-restricted-imports": [
        "error",
        {
          patterns: [
            parentImportPattern,
            nestedRelativeImportPattern,
            {
              group: featureNames
                .filter((otherFeatureName) => otherFeatureName !== featureName)
                .map((otherFeatureName) => `@/features/${otherFeatureName}/**`),
              message: "Feature modules must communicate through shared presentation or explicit inputs and events.",
            },
          ],
        },
      ],
    },
  })),
);
