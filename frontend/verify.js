#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const { spawn } = require('child_process');

const colors = {
    reset: '\x1b[0m',
    green: '\x1b[32m',
    red: '\x1b[31m',
    yellow: '\x1b[33m',
    cyan: '\x1b[36m'
};

const symbols = {
    pass: `${colors.green}✅${colors.reset}`,
    fail: `${colors.red}❌${colors.reset}`,
    info: `${colors.cyan}ℹ️ ${colors.reset}`,
    warn: `${colors.yellow}⚠️ ${colors.reset}`
};

class FrontendVerifier {
    constructor() {
        this.passed = 0;
        this.total = 0;
        this.rootDir = __dirname;
    }

    log(symbol, message) {
        console.log(`${symbol} ${message}`);
    }

    section(title) {
        console.log(`\n${colors.cyan}▶ ${title}${colors.reset}`);
    }

    async verify() {
        console.clear();
        console.log(`${colors.cyan}${'═'.repeat(60)}${colors.reset}`);
        console.log(`${colors.cyan}IPI PRO — FRONTEND VERIFICATION${colors.reset}`);
        console.log(`${colors.cyan}${'═'.repeat(60)}${colors.reset}\n`);

        // 1. Project Structure
        await this.checkProjectStructure();

        // 2. Dependencies
        await this.checkDependencies();

        // 3. Build
        await this.checkBuild();

        // 4. Components & Code Quality
        await this.checkComponents();

        // 5. API Integration
        await this.checkAPIIntegration();

        // 6. Styling
        await this.checkStyling();

        // Summary
        this.printSummary();
    }

    async checkProjectStructure() {
        this.section('PROJECT STRUCTURE');

        const files = [
            'src/main.js',
            'src/App.vue',
            'src/api.js',
            'src/style.css',
            'index.html',
            'vite.config.js',
            'tailwind.config.js',
            'package.json'
        ];

        for (const file of files) {
            const fullPath = path.join(this.rootDir, file);
            if (fs.existsSync(fullPath)) {
                this.log(symbols.pass, file);
                this.passed++;
            } else {
                this.log(symbols.fail, `${file} — NOT FOUND`);
            }
            this.total++;
        }
    }

    async checkDependencies() {
        this.section('DEPENDENCIES');

        const pkgPath = path.join(this.rootDir, 'package.json');
        if (!fs.existsSync(pkgPath)) {
            this.log(symbols.fail, 'package.json not found');
            this.total++;
            return;
        }

        const pkg = JSON.parse(fs.readFileSync(pkgPath, 'utf8'));
        const required = {
            'vue': 'Vue.js 3',
            'vite': 'Vite',
            'axios': 'Axios',
            'tailwindcss': 'Tailwind CSS'
        };

        for (const [key, label] of Object.entries(required)) {
            const hasDep = pkg.dependencies?.[key] || pkg.devDependencies?.[key];
            if (hasDep) {
                this.log(symbols.pass, `${label} (${hasDep})`);
                this.passed++;
            } else {
                this.log(symbols.fail, `${label} missing`);
            }
            this.total++;
        }
    }

    async checkBuild() {
        this.section('BUILD & COMPILE');

        const buildSuccess = await this.runCommand('npm', ['run', 'build']);
        if (buildSuccess) {
            this.log(symbols.pass, 'Production build succeeds');
            this.passed++;
        } else {
            this.log(symbols.warn, 'Build check requires manual review');
        }
        this.total++;
    }

    async checkComponents() {
        this.section('COMPONENTS & FEATURES');

        const appPath = path.join(this.rootDir, 'src', 'App.vue');
        if (!fs.existsSync(appPath)) {
            this.log(symbols.fail, 'App.vue not found');
            this.total++;
            return;
        }

        const appContent = fs.readFileSync(appPath, 'utf8');
        const features = [
            { name: 'Manifest worklist', pattern: /Recent Manifests|filteredManifests/ },
            { name: 'Manifest detail', pattern: /activeManifest|manifest detail/i },
            { name: 'Specimen table', pattern: /<table.*specimens/i },
            { name: 'Status badges', pattern: /status.*badge|SpecimenStatus/i },
            { name: 'KPI counts', pattern: /receivedCount|pendingCount|flaggedCount/ },
            { name: 'Mark received action', pattern: /markReceived/ },
            { name: 'Flag missing action', pattern: /flagMissing/ },
            { name: 'Close manifest action', pattern: /closeManifest/ },
            { name: 'Error handling', pattern: /errorMessage|error toast/i },
            { name: 'Loading states', pattern: /isLoading/ }
        ];

        for (const feature of features) {
            if (feature.pattern.test(appContent)) {
                this.log(symbols.pass, feature.name);
                this.passed++;
            } else {
                this.log(symbols.fail, `${feature.name} — not implemented`);
            }
            this.total++;
        }
    }

    async checkAPIIntegration() {
        this.section('API INTEGRATION');

        const apiPath = path.join(this.rootDir, 'src', 'api.js');
        if (!fs.existsSync(apiPath)) {
            this.log(symbols.fail, 'api.js not found');
            this.total++;
            return;
        }

        const apiContent = fs.readFileSync(apiPath, 'utf8');
        const checks = [
            { name: 'Axios instance', pattern: /axios.create/ },
            { name: 'Base URL configured', pattern: /baseURL|localhost:5052/ },
            { name: 'X-Lab-Id header', pattern: /X-Lab-Id/ },
            { name: 'Error interceptor', pattern: /interceptors/ }
        ];

        for (const check of checks) {
            if (check.pattern.test(apiContent)) {
                this.log(symbols.pass, check.name);
                this.passed++;
            } else {
                this.log(symbols.fail, `${check.name} — not configured`);
            }
            this.total++;
        }

        // Check API calls in App.vue
        const appPath = path.join(this.rootDir, 'src', 'App.vue');
        const appContent = fs.readFileSync(appPath, 'utf8');
        const endpoints = [
            { route: 'GET /manifests', pattern: /api.get.*manifests/ },
            { route: 'GET /manifests/{id}', pattern: /api.get.*manifests.*id/ },
            { route: 'POST /receive', pattern: /api.post.*receive/ },
            { route: 'POST /flag', pattern: /api.post.*flag/ },
            { route: 'POST /close', pattern: /api.post.*close/ }
        ];

        for (const endpoint of endpoints) {
            if (endpoint.pattern.test(appContent)) {
                this.log(symbols.pass, endpoint.route);
                this.passed++;
            } else {
                this.log(symbols.fail, `${endpoint.route} — not integrated`);
            }
            this.total++;
        }
    }

    async checkStyling() {
        this.section('STYLING');

        const checks = [
            { file: 'tailwind.config.js', name: 'Tailwind config' },
            { file: 'postcss.config.js', name: 'PostCSS config' },
            { file: 'src/style.css', name: 'Global styles' }
        ];

        for (const check of checks) {
            const fullPath = path.join(this.rootDir, check.file);
            if (fs.existsSync(fullPath)) {
                this.log(symbols.pass, check.name);
                this.passed++;
            } else {
                this.log(symbols.fail, `${check.name} — not found`);
            }
            this.total++;
        }

        // Check Tailwind usage
        const appPath = path.join(this.rootDir, 'src', 'App.vue');
        const appContent = fs.readFileSync(appPath, 'utf8');
        if (/class="[^"]*(?:bg-|text-|px-|py-|rounded)/i.test(appContent)) {
            this.log(symbols.pass, 'Tailwind classes applied');
            this.passed++;
        } else {
            this.log(symbols.warn, 'Tailwind usage limited');
        }
        this.total++;
    }

    async runCommand(cmd, args) {
        return new Promise((resolve) => {
            const proc = spawn(cmd, args, { cwd: this.rootDir, stdio: 'pipe' });
            let finished = false;

            proc.on('close', (code) => {
                finished = true;
                resolve(code === 0);
            });

            setTimeout(() => {
                if (!finished) {
                    proc.kill();
                    resolve(false);
                }
            }, 30000); // 30 second timeout
        });
    }

    printSummary() {
        console.log(`\n${colors.cyan}${'═'.repeat(60)}${colors.reset}`);
        console.log(`${colors.cyan}📊 VERIFICATION SUMMARY${colors.reset}`);
        console.log(`${colors.cyan}${'═'.repeat(60)}${colors.reset}\n`);

        const percentage = Math.round((this.passed * 100) / this.total);
        const statusColor = percentage >= 90 ? colors.green : percentage >= 70 ? colors.yellow : colors.red;

        console.log(`${statusColor}FRONTEND: ${this.passed}/${this.total} (${percentage}% complete)${colors.reset}`);

        if (this.passed === this.total) {
            console.log(`${colors.green}✅ Frontend is ready for submission!${colors.reset}`);
        } else {
            console.log(`${colors.yellow}⚠️  ${this.total - this.passed} items need attention${colors.reset}`);
        }

        console.log();
    }
}

new FrontendVerifier().verify().catch(console.error);