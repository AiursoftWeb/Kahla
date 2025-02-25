// @ts-check
import js from '@eslint/js';
import tsEslint from 'typescript-eslint';
import ngEslint from 'angular-eslint';

export default tsEslint.config(
    {
        files: ['**/*.ts'],
        extends: [
            js.configs.recommended,
            ...tsEslint.configs.recommendedTypeChecked,
            ...tsEslint.configs.stylisticTypeChecked,
            ...ngEslint.configs.tsRecommended,
        ],
        processor: ngEslint.processInlineTemplates,
        rules: {
            '@angular-eslint/directive-selector': [
                'error',
                {
                    type: 'attribute',
                    prefix: 'app',
                    style: 'camelCase',
                },
            ],
            '@angular-eslint/component-selector': [
                'error',
                {
                    type: 'element',
                    prefix: 'app',
                    style: 'kebab-case',
                },
            ],
            '@angular-eslint/prefer-standalone': ['off'],
            '@typescript-eslint/no-misused-promises': ['off'],
        },
        languageOptions: {
            parserOptions: {
                projectService: true,
                tsconfigRootDir: import.meta.dirname,
            },
        },
    },
    {
        files: ['**/*.html'],
        extends: [
            ...ngEslint.configs.templateRecommended,
            ...ngEslint.configs.templateAccessibility,
        ],
        rules: {
            '@angular-eslint/template/label-has-associated-control': ['off'],
            '@angular-eslint/template/click-events-have-key-events': ['off'],
            '@angular-eslint/template/interactive-supports-focus': ['off'],
            '@angular-eslint/template/alt-text': ['off'],
        },
    }
);
