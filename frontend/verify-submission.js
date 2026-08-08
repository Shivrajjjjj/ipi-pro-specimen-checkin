#!/usr/bin/env node

const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

const colors = {
  reset: '\x1b[0m',
  green: '\x1b[32m',
  red: '\x1b[31m',
  yellow: '\x1b[33m',
  blue: '\x1b[36m'
};

const log = {
  pass: (msg) => console.log(`${colors.green}✅${colors.reset} ${msg}`),
  fail: (msg) => console.log(`${colors.red}❌${colors.reset} ${msg}`),
  info: (msg) => console.log(`${colors.blue}ℹ️${colors.reset}  ${msg}`),
  warn: (msg) => console.log(`${colors.yellow}⚠️${colors.reset}  ${msg}`)
};

class SubmissionVerifier {
  constructor(rootDir = '.') {
    this.rootDir = rootDir;
    this.results = {
      backend: [],
      frontend: [],
      tests: [],
      docs: [],
      config: []
    };
  }

  checkFileExists(filePath, category = 'config') {
    const fullPath = path.join(this.rootDir, filePath);
    const exists = fs.existsSync(fullPath);
    const status = exists ? 'PASS' : 'FAIL';
    this.results[category].push({ file: filePath, status });
    return exists;
  }

  checkBackendStructure() {
    console.log(`\n${colors.blue}▶ Backend Structure${colors.reset}`);
    const files = [
      'backend/IpiPro.Api/Program.cs',
      'backend/IpiPro.Api/Context/AppDbContext.cs',
      'backend/IpiPro.Api/Context/DbInitializer.cs',
      'backend/IpiPro.Api/Models/Entities.cs',
      'backend/IpiPro.Api/Controllers/ManifestsController.cs',
      'backend/IpiPro.Api/Services/TenantProvider.cs',
      'backend/IpiPro.Api/appsettings.json',
      'backend/IpiPro.Api/Properties/launchSettings.json'
    ];

    let passed = 0;
    files.forEach(file => {
      if (this.checkFileExists(file, 'backend')) {
        log.pass(file);
        passed++;
      } else {
        log.fail(file);
      }
    });

    return { total: files.length, passed };
  }

  checkFrontendStructure() {
    console.log(`\n${colors.blue}▶ Frontend Structure${colors.reset}`);
    const files = [
      'frontend/src/App.vue',
      'frontend/src/main.js',
      'frontend/src/api.js',
      'frontend/src/style.css',
      'frontend/vite.config.js',
      'frontend/tailwind.config.js',
      'frontend/postcss.config.js',
      'frontend/package.json'
    ];

    let passed = 0;
    files.forEach(file => {
      if (this.checkFileExists(file, 'frontend')) {
        log.pass(file);
        passed++;
      } else {
        log.fail(file);
      }
    });

    return { total: files.length, passed };
  }

  checkTests() {
    console.log(`\n${colors.blue}▶ Testing${colors.reset}`);
    const files = [
      'backend/IpiPro.Tests/TenantIsolationTests.cs',
      'backend/IpiPro.Tests/ReconciliationTests.cs',
      'backend/IpiPro.Tests/IpiPro.Tests.csproj'
    ];

    let passed = 0;
    files.forEach(file => {
      if (this.checkFileExists(file, 'tests')) {
        log.pass(file);
        passed++;
      } else {
        log.fail(file);
      }
    });

    return { total: files.length, passed };
  }

  checkDocumentation() {
    console.log(`\n${colors.blue}▶ Documentation${colors.reset}`);
    const files = [
      'README.md',
      'backend/ARCHITECTURE.md'
    ];

    let passed = 0;
    files.forEach(file => {
      if (this.checkFileExists(file, 'docs')) {
        log.pass(file);
        passed++;
      } else {
        log.fail(file);
      }
    });

    return { total: files.length, passed };
  }

  checkConfiguration() {
    console.log(`\n${colors.blue}▶ Configuration${colors.reset}`);
    const files = [
      'backend/.gitignore',
      'frontend/.gitignore',
      '.env.example'
    ];

    let passed = 0;
    files.forEach(file => {
      if (this.checkFileExists(file, 'config')) {
        log.pass(file);
        passed++;
      } else {
        log.fail(file);
      }
    });

    return { total: files.length, passed };
  }

  validateBackendProject() {
    console.log(`\n${colors.blue}▶ Backend Project Validation${colors.reset}`);
    try {
      const csprojPath = path.join(this.rootDir, 'backend/IpiPro.Api/IpiPro.Api.csproj');
      if (fs.existsSync(csprojPath)) {
        const content = fs.readFileSync(csprojPath, 'utf8');
        const checks = [
          { name: 'Entity Framework Core', pattern: /EntityFrameworkCore/ },
          { name: 'ASP.NET Core', pattern: /(Mvc|AspNetCore)/ },
          { name: 'Target Framework', pattern: /net[6789]/ }
        ];

        let passed = 0;
        checks.forEach(check => {
          if (check.pattern.test(content)) {
            log.pass(`${check.name} referenced`);
            passed++;
          } else {
            log.warn(`${check.name} not found`);
          }
        });
        return { total: checks.length, passed };
      }
    } catch (e) {
      log.fail(`Error validating backend project: ${e.message}`);
    }
    return { total: 0, passed: 0 };
  }

  validateFrontendPackage() {
    console.log(`\n${colors.blue}▶ Frontend Dependencies${colors.reset}`);
    try {
      const packagePath = path.join(this.rootDir, 'frontend/package.json');
      if (fs.existsSync(packagePath)) {
        const pkg = JSON.parse(fs.readFileSync(packagePath, 'utf8'));
        const checks = [
          { name: 'Vue.js', key: 'vue' },
          { name: 'Vite', key: 'vite' },
          { name: 'Axios', key: 'axios' },
          { name: 'Tailwind CSS', key: 'tailwindcss' }
        ];

        let passed = 0;
        checks.forEach(check => {
          const hasDep = pkg.dependencies?.[check.key] || pkg.devDependencies?.[check.key];
          if (hasDep) {
            log.pass(`${check.name} (${hasDep})`);
            passed++;
          } else {
            log.fail(`${check.name} not found`);
          }
        });
        return { total: checks.length, passed };
      }
    } catch (e) {
      log.fail(`Error validating package.json: ${e.message}`);
    }
    return { total: 0, passed: 0 };
  }

  printSummary(results) {
    console.log(`\n${'═'.repeat(50)}`);
    console.log(`${colors.blue}📊 Verification Summary${colors.reset}`);
    console.log(`${'═'.repeat(50)}\n`);

    const categories = Object.entries(results);
    let totalChecks = 0;
    let totalPassed = 0;

    categories.forEach(([category, count]) => {
      const percentage = count.total > 0 ? ((count.passed / count.total) * 100).toFixed(0) : 0;
      const status = count.passed === count.total ? colors.green : colors.yellow;
      console.log(`${status}${category.toUpperCase()}: ${count.passed}/${count.total} (${percentage}%)${colors.reset}`);
      totalChecks += count.total;
      totalPassed += count.passed;
    });

    console.log(`\n${colors.blue}Overall: ${totalPassed}/${totalChecks}${colors.reset}`);
    if (totalPassed === totalChecks) {
      log.pass('All checks passed! Ready for submission.');
    } else {
      log.warn(`${totalChecks - totalPassed} items need attention.`);
    }
  }

  run() {
    console.log(`\n${colors.blue}${'═'.repeat(50)}${colors.reset}`);
    console.log(`${colors.blue}IPI Pro Submission Verifier${colors.reset}`);
    console.log(`${colors.blue}${'═'.repeat(50)}${colors.reset}`);

    const results = {
      backend: this.checkBackendStructure(),
      frontend: this.checkFrontendStructure(),
      tests: this.checkTests(),
      docs: this.checkDocumentation(),
      config: this.checkConfiguration()
    };

    this.validateBackendProject();
    this.validateFrontendPackage();

    this.printSummary(results);
  }
}

// Run verifier
const verifier = new SubmissionVerifier(path.dirname(__dirname));
verifier.run();