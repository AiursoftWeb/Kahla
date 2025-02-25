import js from '@eslint/js';
import globals from 'globals';
import tseslint from 'typescript-eslint';

export default tseslint.config(
    { ignores: ['dist', '*.pnp.*', 'node_modules'] },
    {
        files: ['src/**/*.ts', 'tests/**/*.ts'],
        extends: [js.configs.recommended, ...tseslint.configs.recommendedTypeChecked],
        languageOptions: {
            ecmaVersion: 2023,
            globals: globals.browser,
            parserOptions: {
                // projectService: true,
                project: ['tsconfig.json', 'tsconfig.spec.json'],
                tsconfigRootDir: import.meta.dirname,
            },
        },
    }
);
