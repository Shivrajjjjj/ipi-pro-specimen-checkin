<template>
    <div class="min-h-screen bg-[#F0F4F8] text-slate-800 font-sans text-xs">
        <!-- Top Navigation Header -->
        <header class="bg-[#1C3644] text-white px-6 py-2.5 flex items-center justify-between shadow-sm">
            <div class="flex items-center space-x-4">
                <div class="bg-white text-[#1C3644] font-black px-2 py-0.5 rounded text-sm tracking-wide">IPI</div>
                <span class="border border-slate-400/50 text-slate-200 text-[10px] px-1.5 py-0.2 rounded font-semibold uppercase tracking-wider">UAT</span>
                <div class="text-slate-300 text-xs">
                    Mode: <span class="text-white font-semibold">{{ currentMode }}</span>
                    <span class="mx-3 text-slate-500">|</span>
                    Location: <span class="text-white font-semibold">{{ currentLocation }}</span>
                </div>
            </div>
            <div class="flex items-center space-x-2 text-xs">
                <span class="text-slate-200 font-medium">{{ currentUser }}</span>
                <div class="w-7 h-7 rounded-full bg-[#3B82F6] text-white flex items-center justify-center font-bold text-xs shadow-inner">{{ userInitials }}</div>
            </div>
        </header>

        <!-- Sub-Header Tabs -->
        <nav class="bg-white border-b border-slate-200 px-6 flex space-x-6 text-xs font-semibold text-slate-600">
            <button @click="activeTab = 'check-in'"
                    :class="['py-2.5 px-1 border-b-2 transition', activeTab === 'check-in' ? 'text-[#2B6CB0] border-[#2B6CB0] font-bold' : 'border-transparent hover:text-slate-900']">
                Check-In
            </button>
            <button @click="activeTab = 'scan-history'"
                    :class="['py-2.5 px-1 border-b-2 transition flex items-center space-x-1.5', activeTab === 'scan-history' ? 'text-[#2B6CB0] border-[#2B6CB0] font-bold' : 'border-transparent hover:text-slate-900']">
                <span>Scan History</span>
                <span class="bg-slate-100 text-slate-600 border border-slate-200 px-1.5 py-0.2 rounded-full text-[10px]">{{ scanHistoryCount }}</span>
            </button>
            <button @click="activeTab = 'manifests'"
                    :class="['py-2.5 px-1 border-b-2 transition', activeTab === 'manifests' ? 'text-[#2B6CB0] border-[#2B6CB0] font-bold' : 'border-transparent hover:text-slate-900']">
                Manifests
            </button>
            <button @click="activeTab = 'discrepancies'"
                    :class="['py-2.5 px-1 border-b-2 transition flex items-center space-x-1.5', activeTab === 'discrepancies' ? 'text-[#2B6CB0] border-[#2B6CB0] font-bold' : 'border-transparent hover:text-slate-900']">
                <span>Discrepancies</span>
                <span class="bg-red-100 text-red-600 border border-red-200 px-1.5 py-0.2 rounded-full text-[10px] font-bold">{{ discrepancyCount }}</span>
            </button>
        </nav>

        <!-- Error Toast -->
        <transition name="fade">
            <div v-if="errorMessage" class="fixed top-4 right-4 bg-red-500 text-white px-4 py-3 rounded shadow-lg max-w-sm z-50">
                <div class="flex justify-between items-start">
                    <span>{{ errorMessage }}</span>
                    <button @click="errorMessage = ''" class="ml-2 text-lg">&times;</button>
                </div>
            </div>
        </transition>

        <!-- Success Toast -->
        <transition name="fade">
            <div v-if="successMessage" class="fixed top-4 right-4 bg-emerald-500 text-white px-4 py-3 rounded shadow-lg max-w-sm z-50">
                <div class="flex justify-between items-start">
                    <span>{{ successMessage }}</span>
                    <button @click="successMessage = ''" class="ml-2 text-lg">&times;</button>
                </div>
            </div>
        </transition>

        <!-- Content -->
        <div class="p-5">
            <!-- CHECK-IN TAB -->
            <div v-if="activeTab === 'check-in'" class="grid grid-cols-12 gap-5 max-w-[1600px] mx-auto">
                <!-- Left Sidebar -->
                <aside class="col-span-3 space-y-4">
                    <!-- Workflow Selector -->
                    <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-2">
                        <div class="flex items-center space-x-2">
                            <span class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Verification Workflow</span>
                            <span class="bg-slate-100 text-slate-500 text-[9px] px-1.5 py-0.2 rounded border border-slate-200 font-semibold">LAB SETTING</span>
                        </div>
                        <div class="grid grid-cols-2 gap-1 bg-slate-100 p-1 rounded">
                            <button @click="workflowMode = 'fast'"
                                    :class="['py-1.5 rounded font-bold transition', workflowMode === 'fast' ? 'bg-[#2B5270] text-white shadow-sm' : 'bg-white text-slate-600 border border-slate-200']">
                                Fast Count
                            </button>
                            <button @click="workflowMode = 'full'"
                                    :class="['py-1.5 rounded font-bold transition', workflowMode === 'full' ? 'bg-[#2B5270] text-white shadow-sm' : 'bg-white text-slate-600 border border-slate-200']">
                                Full Scan
                            </button>
                        </div>
                    </div>

                    <!-- Search -->
                    <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-1.5">
                        <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Find Manifest</label>
                        <input v-model="searchQuery"
                               type="text"
                               placeholder="Search by code or clinic..."
                               class="w-full bg-slate-50/80 border border-slate-300 rounded px-3 py-2 text-xs focus:outline-none focus:ring-1 focus:ring-blue-500" />
                    </div>

                    <!-- Counter -->
                    <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-3">
                        <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Verify & Receive</label>
                        <p class="text-slate-600 font-medium text-[11px]">Total bottles counted</p>

                        <div class="flex items-center space-x-2">
                            <button @click="countedBottles = Math.max(0, countedBottles - 1)"
                                    class="w-10 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-600 hover:bg-slate-50">
                                −
                            </button>
                            <div class="flex-1 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-800 text-base bg-white">
                                {{ countedBottles }}
                            </div>
                            <button @click="countedBottles++"
                                    class="w-10 h-9 border border-slate-300 rounded flex items-center justify-center font-bold text-slate-600 hover:bg-slate-50">
                                +
                            </button>
                        </div>

                        <div v-if="activeManifest"
                             class="text-[11px] font-bold"
                             :class="countedBottles === activeManifest.specimens.length ? 'text-emerald-700' : 'text-amber-700'">
                            {{ countedBottles === activeManifest.specimens.length ? '✓ Ready to close' : `⚠️ Mismatch: ${countedBottles} vs ${activeManifest.specimens.length}` }}
                        </div>
                    </div>

                    <!-- Manifests List -->
                    <div class="bg-white p-3.5 rounded-md border border-slate-200 shadow-sm space-y-2.5">
                        <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Recent Manifests</label>

                        <div v-if="isLoading" class="space-y-2">
                            <div v-for="i in 3" :key="i" class="h-16 bg-slate-100 rounded animate-pulse"></div>
                        </div>

                        <div v-else-if="filteredManifests.length === 0" class="text-center text-slate-500 py-6 text-[11px]">
                            <p>No manifests available</p>
                        </div>

                        <div v-else class="space-y-2 max-h-96 overflow-y-auto">
                            <button v-for="m in filteredManifests"
                                    :key="m.id"
                                    @click="selectManifest(m.id)"
                                    :class="['w-full text-left p-3 rounded border transition', activeManifest?.id === m.id ? 'border-blue-400 bg-blue-50 shadow-md' : 'border-slate-200 hover:bg-slate-50']">
                                <div class="flex justify-between items-start mb-1">
                                    <span class="font-bold text-slate-900 text-xs">{{ m.code }}</span>
                                    <span class="text-[10px] text-slate-500">{{ getReceivedCount(m) }}/{{ m.specimens.length }}</span>
                                </div>
                                <div class="text-[11px] text-slate-500 mb-2">{{ m.originClinic }}</div>
                                <span :class="getBadgeStyles(m.status)" class="text-[9px] px-2 py-0.5 rounded font-bold inline-block">
                                    {{ formatStatusText(m.status) }}
                                </span>
                            </button>
                        </div>
                    </div>
                </aside>

                <!-- Right Panel -->
                <main v-if="activeManifest" class="col-span-9 space-y-4">
                    <!-- Header -->
                    <div class="bg-white p-5 rounded-md border border-slate-200 shadow-sm flex justify-between items-start">
                        <div class="space-y-1">
                            <div class="flex items-center space-x-2">
                                <h1 class="text-lg font-bold text-slate-900">{{ activeManifest.code }}</h1>
                                <span class="bg-slate-100 text-slate-600 border border-slate-200 text-[10px] px-2 py-0.5 rounded font-bold">
                                    {{ workflowMode === 'fast' ? 'FAST COUNT' : 'FULL SCAN' }}
                                </span>
                            </div>
                            <p class="text-slate-500 text-xs">
                                <strong>From:</strong> {{ activeManifest.originClinic }} |
                                <strong>Sent:</strong> {{ formatDate(activeManifest.sentAt) }} |
                                <strong>Expected:</strong> {{ activeManifest.specimens.length }} specimens
                            </p>
                        </div>
                        <button @click="closeManifest"
                                :disabled="pendingCount > 0 || isProcessing"
                                :class="['px-4 py-2 rounded font-bold text-xs text-white transition', pendingCount === 0 && !isProcessing ? 'bg-[#2B5270] hover:bg-[#1C3644]' : 'bg-slate-300 cursor-not-allowed']">
                            {{ isProcessing ? '⏳ Closing...' : 'Close Manifest' }}
                        </button>
                    </div>

                    <!-- KPI Cards -->
                    <div class="grid grid-cols-4 gap-4">
                        <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                            <div class="text-3xl font-black text-slate-800">{{ activeManifest.specimens.length }}</div>
                            <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-2">Expected</div>
                        </div>
                        <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                            <div class="text-3xl font-black text-emerald-600">{{ receivedCount }}</div>
                            <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-2">Received</div>
                        </div>
                        <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                            <div class="text-3xl font-black text-amber-500">{{ pendingCount }}</div>
                            <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-2">Pending</div>
                        </div>
                        <div class="bg-white p-4 rounded-md border border-slate-200 text-center shadow-sm">
                            <div class="text-3xl font-black text-red-500">{{ flaggedCount }}</div>
                            <div class="text-[10px] font-bold text-slate-400 tracking-wider uppercase mt-2">Flagged</div>
                        </div>
                    </div>

                    <!-- Table -->
                    <div class="bg-white rounded-md border border-slate-200 shadow-sm overflow-hidden">
                        <div class="px-5 py-3 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                            <h2 class="text-xs font-bold text-slate-700">{{ activeManifest.specimens.length }} Specimens</h2>
                            <span class="text-[11px] bg-emerald-100 text-emerald-800 border border-emerald-200 px-2.5 py-0.5 rounded font-bold">
                                {{ receivedCount }} received
                            </span>
                        </div>

                        <div v-if="activeManifest.specimens.length === 0" class="px-5 py-8 text-center text-slate-500">
                            No specimens
                        </div>

                        <div v-else class="overflow-x-auto">
                            <table class="w-full text-left text-xs">
                                <thead class="bg-slate-50 border-b border-slate-200">
                                    <tr>
                                        <th class="px-4 py-2 font-bold text-slate-600">STATUS</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">ID</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">PATIENT</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">SITE</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">PROVIDER</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">RECEIVED BY</th>
                                        <th class="px-4 py-2 font-bold text-slate-600">AT</th>
                                        <th class="px-4 py-2 font-bold text-slate-600 text-right">ACTIONS</th>
                                    </tr>
                                </thead>
                                <tbody class="divide-y divide-slate-100">
                                    <tr v-for="s in activeManifest.specimens" :key="s.id" class="hover:bg-slate-50">
                                        <td class="px-4 py-2">
                                            <span v-if="s.status === 1" class="bg-emerald-100 text-emerald-800 border border-emerald-200 px-2 py-0.5 rounded text-[10px] font-bold inline-block">✓ Received</span>
                                            <span v-else-if="s.status === 2" class="bg-red-100 text-red-800 border border-red-200 px-2 py-0.5 rounded text-[10px] font-bold inline-block">🚩 Flagged</span>
                                            <span v-else class="bg-slate-100 text-slate-600 border border-slate-200 px-2 py-0.5 rounded text-[10px] font-bold inline-block">Pending</span>
                                        </td>
                                        <td class="px-4 py-2 font-bold text-slate-900">{{ s.code }}</td>
                                        <td class="px-4 py-2 text-slate-700">{{ s.patientName }}</td>
                                        <td class="px-4 py-2 text-slate-600">{{ s.site }}</td>
                                        <td class="px-4 py-2 text-slate-600">{{ s.provider }}</td>
                                        <td class="px-4 py-2 text-slate-600">{{ s.receivedBy || '—' }}</td>
                                        <td class="px-4 py-2 text-slate-600">{{ s.receivedAt ? formatTime(s.receivedAt) : '—' }}</td>
                                        <td class="px-4 py-2 text-right space-x-1">
                                            <button @click="markReceived(s.id)"
                                                    :disabled="isProcessing || s.status === 1"
                                                    class="px-2 py-1 border border-slate-200 rounded text-slate-600 hover:bg-slate-100 font-bold disabled:opacity-50">
                                                ✏️
                                            </button>
                                            <button @click="flagMissing(s.id)"
                                                    :disabled="isProcessing || s.status === 2"
                                                    class="px-2 py-1 border border-slate-200 rounded text-slate-600 hover:bg-slate-100 font-bold disabled:opacity-50">
                                                🚩
                                            </button>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </main>

                <!-- Empty State -->
                <main v-else class="col-span-9 flex items-center justify-center min-h-96">
                    <div class="text-center text-slate-500">
                        <p class="text-lg font-semibold mb-2">📦 No Manifest Selected</p>
                        <p class="text-sm">Choose one from the list to begin</p>
                    </div>
                </main>
            </div>

            <!-- SCAN HISTORY TAB -->
            <div v-else-if="activeTab === 'scan-history'" class="max-w-4xl mx-auto">
                <div class="bg-white p-6 rounded-md border border-slate-200 shadow-sm">
                    <h2 class="text-lg font-bold text-slate-900 mb-4">📋 Scan History</h2>
                    <div v-if="scanHistory.length === 0" class="text-center text-slate-500 py-8">
                        No scans recorded yet
                    </div>
                    <div v-else class="space-y-3">
                        <div v-for="(scan, idx) in scanHistory" :key="idx" class="p-3 border border-slate-200 rounded flex justify-between items-start">
                            <div>
                                <p class="font-semibold text-slate-900">{{ scan.action }}</p>
                                <p class="text-[11px] text-slate-500">{{ scan.specimen }} · {{ scan.manifest }}</p>
                            </div>
                            <p class="text-[11px] text-slate-600">{{ scan.timestamp }}</p>
                        </div>
                    </div>
                </div>
            </div>

            <!-- MANIFESTS TAB -->
            <div v-else-if="activeTab === 'manifests'" class="max-w-5xl mx-auto">
                <div class="bg-white p-6 rounded-md border border-slate-200 shadow-sm">
                    <h2 class="text-lg font-bold text-slate-900 mb-4">📦 All Manifests</h2>
                    <div v-if="manifests.length === 0" class="text-center text-slate-500 py-8">
                        No manifests available
                    </div>
                    <div v-else class="overflow-x-auto">
                        <table class="w-full text-left text-xs">
                            <thead class="bg-slate-50 border-b border-slate-200">
                                <tr>
                                    <th class="px-4 py-3 font-bold text-slate-600">CODE</th>
                                    <th class="px-4 py-3 font-bold text-slate-600">CLINIC</th>
                                    <th class="px-4 py-3 font-bold text-slate-600">SENT</th>
                                    <th class="px-4 py-3 font-bold text-slate-600">SPECIMENS</th>
                                    <th class="px-4 py-3 font-bold text-slate-600">RECEIVED</th>
                                    <th class="px-4 py-3 font-bold text-slate-600">STATUS</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-slate-100">
                                <tr v-for="m in manifests" :key="m.id" class="hover:bg-slate-50">
                                    <td class="px-4 py-3 font-bold text-slate-900">{{ m.code }}</td>
                                    <td class="px-4 py-3 text-slate-700">{{ m.originClinic }}</td>
                                    <td class="px-4 py-3 text-slate-600">{{ formatDate(m.sentAt) }}</td>
                                    <td class="px-4 py-3 text-slate-600">{{ m.specimens.length }}</td>
                                    <td class="px-4 py-3 text-slate-600">{{ getReceivedCount(m) }}</td>
                                    <td class="px-4 py-3">
                                        <span :class="getBadgeStyles(m.status)" class="text-[10px] px-2 py-0.5 rounded font-bold inline-block">
                                            {{ formatStatusText(m.status) }}
                                        </span>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <!-- DISCREPANCIES TAB -->
            <div v-else-if="activeTab === 'discrepancies'" class="max-w-4xl mx-auto">
                <div class="bg-white p-6 rounded-md border border-slate-200 shadow-sm">
                    <h2 class="text-lg font-bold text-slate-900 mb-4">⚠️ Discrepancies</h2>
                    <div v-if="discrepancies.length === 0" class="text-center text-slate-500 py-8">
                        No discrepancies recorded
                    </div>
                    <div v-else class="space-y-3">
                        <div v-for="d in discrepancies" :key="d.id" class="p-4 border border-red-200 bg-red-50 rounded">
                            <div class="flex justify-between items-start">
                                <div>
                                    <p class="font-semibold text-red-900">🚩 Missing Specimen</p>
                                    <p class="text-[11px] text-red-700 mt-1">
                                        <strong>Manifest:</strong> {{ d.manifestCode }} |
                                        <strong>Specimen:</strong> {{ d.specimenCode }}
                                    </p>
                                </div>
                                <span class="text-[10px] bg-red-100 text-red-800 px-2 py-1 rounded font-bold">{{ d.status }}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<style scoped>
    .fade-enter-active, .fade-leave-active {
        transition: opacity 0.3s;
    }

    .fade-enter-from, .fade-leave-to {
        opacity: 0;
    }
</style>

<script setup>
    import { ref, computed, onMounted } from 'vue';
    import api from './api';

    // UI State
    const activeTab = ref('check-in');
    const workflowMode = ref('fast');
    const manifests = ref([]);
    const activeManifest = ref(null);
    const countedBottles = ref(0);
    const searchQuery = ref('');
    const isLoading = ref(false);
    const isProcessing = ref(false);
    const errorMessage = ref('');
    const successMessage = ref('');

    // Context
    const currentMode = ref('Check-In');
    const currentLocation = ref('Central Lab — Receiving');
    const currentUser = ref('Lab Tech 1');
    const userInitials = ref('LT');

    // History & Discrepancies
    const scanHistory = ref([]);
    const discrepancies = ref([]);

    // Computed
    const scanHistoryCount = computed(() => scanHistory.value.length);
    const discrepancyCount = computed(() => discrepancies.value.length);
    const receivedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 1).length || 0);
    const flaggedCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 2).length || 0);
    const pendingCount = computed(() => activeManifest.value?.specimens.filter(s => s.status === 0).length || 0);

    const filteredManifests = computed(() => {
        if (!searchQuery.value) return manifests.value;
        const query = searchQuery.value.toLowerCase();
        return manifests.value.filter(m =>
            m.code.toLowerCase().includes(query) ||
            m.originClinic.toLowerCase().includes(query)
        );
    });

    // API Methods
    const fetchManifests = async () => {
        isLoading.value = true;
        try {
            const res = await api.get('/manifests');
            manifests.value = res.data;
            console.log('✅ Manifests loaded:', manifests.value.length);
            if (manifests.value.length > 0) {
                await selectManifest(manifests.value[0].id);
            }
        } catch (err) {
            errorMessage.value = "❌ Cannot connect to backend. Ensure it's running on localhost:5052.";
            console.error("Error:", err.message);
        } finally {
            isLoading.value = false;
        }
    };

    const selectManifest = async (id) => {
        try {
            const res = await api.get(`/manifests/${id}`);
            activeManifest.value = res.data;
            countedBottles.value = activeManifest.value.specimens.filter(s => s.status === 1).length;
            console.log('✅ Manifest selected:', activeManifest.value.code);
        } catch (err) {
            errorMessage.value = "Failed to load manifest details.";
            console.error("Error:", err);
        }
    };

    const markReceived = async (specimenId) => {
        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/receive`);
            const specimen = activeManifest.value.specimens.find(s => s.id === specimenId);

            scanHistory.value.unshift({
                action: '✓ Marked Received',
                specimen: specimen?.code || 'Unknown',
                manifest: activeManifest.value.code,
                timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
            });

            await selectManifest(activeManifest.value.id);
            successMessage.value = `✓ ${specimen?.code} marked as received`;
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to mark specimen received.";
            console.error("Error:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    const flagMissing = async (specimenId) => {
        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/specimens/${specimenId}/flag`);
            const specimen = activeManifest.value.specimens.find(s => s.id === specimenId);

            discrepancies.value.unshift({
                id: `disc-${Date.now()}`,
                manifestCode: activeManifest.value.code,
                specimenCode: specimen?.code || 'Unknown',
                status: 'Open'
            });

            scanHistory.value.unshift({
                action: '🚩 Flagged Missing',
                specimen: specimen?.code || 'Unknown',
                manifest: activeManifest.value.code,
                timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
            });

            await selectManifest(activeManifest.value.id);
            successMessage.value = `🚩 ${specimen?.code} flagged as missing`;
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to flag specimen.";
            console.error("Error:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    const closeManifest = async () => {
        if (pendingCount.value > 0) {
            errorMessage.value = `Cannot close: ${pendingCount.value} specimen(s) still pending`;
            return;
        }

        isProcessing.value = true;
        try {
            await api.post(`/manifests/${activeManifest.value.id}/close`);

            scanHistory.value.unshift({
                action: '✓ Closed Manifest',
                specimen: '—',
                manifest: activeManifest.value.code,
                timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false })
            });

            successMessage.value = `✓ Manifest closed successfully`;
            await fetchManifests();
            setTimeout(() => successMessage.value = '', 3000);
        } catch (err) {
            errorMessage.value = err.response?.data?.error || "Failed to close manifest.";
            console.error("Error:", err);
        } finally {
            isProcessing.value = false;
        }
    };

    // Helpers
    const getReceivedCount = (manifest) => manifest.specimens.filter(s => s.status === 1).length;

    const formatTime = (dt) => {
        if (!dt) return '—';
        return new Date(dt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
    };

    const formatDate = (dt) => {
        if (!dt) return '—';
        const date = new Date(dt);
        return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' +
            date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit', hour12: false });
    };

    const formatStatusText = (status) => {
        if (status === 0) return 'In Transit';
        if (status === 1) return 'Received';
        if (status === 2) return 'With Discrepancy';
        return 'Closed';
    };

    const getBadgeStyles = (status) => {
        if (status === 0) return 'bg-slate-100 text-slate-600 border border-slate-200';
        if (status === 1) return 'bg-emerald-100 text-emerald-800 border border-emerald-200';
        return 'bg-red-100 text-red-700 border border-red-200';
    };

    // Lifecycle
    onMounted(() => {
        console.log('🚀 Frontend initialized');
        fetchManifests();
    });
</script>